using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Audit.Core;
using Audit.EntityFramework;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Audit;
using CeyPASS.Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.Infrastructure.Audit
{
    /// <summary>Audit.NET olaylarını dbo.SistemLoglari’na yazar; hata yutulur.</summary>
    public sealed class SistemLogAuditDataProvider : AuditDataProvider
    {
        private static readonly string[] SensitiveKeys =
        {
            "sifre", "password", "passwd", "token", "secret", "pwd"
        };

        private readonly IServiceProvider _root;
        private readonly string _client;

        public SistemLogAuditDataProvider(IServiceProvider root, string client)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _client = string.IsNullOrWhiteSpace(client) ? "Unknown" : client;
        }

        public override object InsertEvent(AuditEvent auditEvent)
        {
            try { Write(auditEvent); }
            catch { /* audit asla business akışını düşürmez */ }
            return Guid.NewGuid().ToString("N");
        }

        public override Task<object> InsertEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
        {
            InsertEvent(auditEvent);
            return Task.FromResult<object>(Guid.NewGuid().ToString("N"));
        }

        private void Write(AuditEvent auditEvent)
        {
            if (auditEvent == null) return;

            IServiceScope? owned = null;
            try
            {
                var sp = ResolveProvider(out owned);
                var svc = sp.GetService<ISistemLogService>();
                if (svc == null) return;

                var session = sp.GetService<ISessionContext>();
                int? uid = session?.AktifKullaniciId;
                string ip = GetLocalIp() ?? "";
                string pc = Environment.MachineName ?? "";

                string kaynak;
                string islem;
                string mesaj;
                object detailPayload;

                if (auditEvent is AuditEventEntityFramework efEv && efEv.EntityFrameworkEvent != null)
                {
                    var entries = (efEv.EntityFrameworkEvent.Entries ?? new List<EventEntry>())
                        .Where(e => !string.Equals(e.Table, "SistemLoglari", StringComparison.OrdinalIgnoreCase)
                                    && !string.Equals(e.Name, "SistemLoglari", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (entries.Count == 0) return;

                    var first = entries[0];
                    kaynak = first.Table ?? first.Name ?? "Entity";
                    islem = first.Action ?? "SaveChanges";
                    mesaj = entries.Count == 1
                        ? $"{kaynak} {islem}"
                        : $"{entries.Count} entity değişikliği ({islem})";
                    detailPayload = new
                    {
                        Client = _client,
                        EventType = auditEvent.EventType,
                        Entries = entries.Select(e => new
                        {
                            e.Table,
                            e.Name,
                            e.Action,
                            PrimaryKey = e.PrimaryKey,
                            Changes = e.Changes?.Select(c => new
                            {
                                c.ColumnName,
                                Original = MaskIfSensitive(c.ColumnName, c.OriginalValue),
                                New = MaskIfSensitive(c.ColumnName, c.NewValue)
                            })
                        })
                    };
                }
                else
                {
                    ExtractCommandFields(auditEvent, out var commandText, out var parameters,
                        out var durationMs, out var success);

                    kaynak = ExtractSqlName(commandText) ?? "SqlRaw";
                    islem = "ExecuteSql";
                    mesaj = Truncate(
                        !string.IsNullOrWhiteSpace(commandText) ? commandText : (auditEvent.EventType ?? "ExecuteSql"),
                        2000);
                    detailPayload = new
                    {
                        Client = _client,
                        CommandText = Truncate(commandText, 500),
                        Parameters = MaskParameterMap(parameters),
                        DurationMs = durationMs,
                        Success = success
                    };
                }

                string detayJson;
                try { detayJson = JsonSerializer.Serialize(detailPayload); }
                catch { detayJson = "{\"Client\":\"" + _client + "\"}"; }

                using (AuditWriteSuppress.Enter())
                {
                    svc.Info(uid, Truncate(kaynak, 100), Truncate(islem, 100), Truncate(mesaj, 2000),
                        Truncate(ip, 100), Truncate(pc, 100), detayJson, Guid.NewGuid().ToString("N"));
                }
            }
            finally
            {
                owned?.Dispose();
            }
        }

        private IServiceProvider ResolveProvider(out IServiceScope? owned)
        {
            owned = null;
            var http = _root.GetService<IHttpContextAccessor>();
            if (http?.HttpContext?.RequestServices != null)
                return http.HttpContext.RequestServices;

            owned = _root.CreateScope();
            return owned.ServiceProvider;
        }

        private static object? MaskIfSensitive(string? column, object? value)
        {
            if (value == null) return null;
            if (string.IsNullOrEmpty(column)) return value;
            var c = column.ToLowerInvariant();
            if (SensitiveKeys.Any(k => c.Contains(k)))
                return "***";
            return value;
        }

        /// <summary>Command interceptor olayından düz alanlar (ToJson gömülmez).</summary>
        private static void ExtractCommandFields(
            AuditEvent auditEvent,
            out string? commandText,
            out Dictionary<string, object?>? parameters,
            out double? durationMs,
            out bool? success)
        {
            commandText = null;
            parameters = null;
            durationMs = auditEvent.Duration;
            success = null;

            try
            {
                using var doc = JsonDocument.Parse(auditEvent.ToJson() ?? "{}");
                var root = doc.RootElement;

                if (root.TryGetProperty("Duration", out var durEl) && durEl.ValueKind == JsonValueKind.Number)
                    durationMs = durEl.GetDouble();

                if (!root.TryGetProperty("CommandEvent", out var cmd) || cmd.ValueKind != JsonValueKind.Object)
                    return;

                if (cmd.TryGetProperty("CommandText", out var ct) && ct.ValueKind == JsonValueKind.String)
                    commandText = ct.GetString();

                if (cmd.TryGetProperty("Success", out var suc))
                {
                    if (suc.ValueKind == JsonValueKind.True) success = true;
                    else if (suc.ValueKind == JsonValueKind.False) success = false;
                }

                if (cmd.TryGetProperty("Parameters", out var pars) && pars.ValueKind == JsonValueKind.Object)
                {
                    parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    foreach (var p in pars.EnumerateObject())
                        parameters[p.Name] = JsonElementToObject(p.Value);
                }
            }
            catch
            {
                /* düşürülmez; boş alanlarla devam */
            }
        }

        private static object? JsonElementToObject(JsonElement el) => el.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => el.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => el.TryGetInt64(out var l) ? l : el.GetDouble(),
            _ => el.ToString()
        };

        private static Dictionary<string, object?> MaskParameterMap(Dictionary<string, object?>? parameters)
        {
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (parameters == null) return result;
            foreach (var kv in parameters)
                result[kv.Key] = MaskIfSensitive(kv.Key, kv.Value);
            return result;
        }

        /// <summary>EXEC dbo.sp_Name → sp_Name; yoksa SqlRaw.</summary>
        private static string? ExtractSqlName(string? commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return null;
            var m = Regex.Match(commandText, @"\b(sp_[A-Za-z0-9_]+)\b", RegexOptions.IgnoreCase);
            if (m.Success) return m.Groups[1].Value;
            m = Regex.Match(commandText, @"\b(?:INSERT|UPDATE|DELETE|MERGE)\b\s+(?:INTO\s+)?(?:\[?dbo\]?\.)?\[?([A-Za-z0-9_]+)\]?",
                RegexOptions.IgnoreCase);
            if (m.Success) return m.Groups[1].Value;
            return null;
        }

        private static string Truncate(string? s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max);
        }

        private static string? GetLocalIp()
        {
            try
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch { }
            return null;
        }
    }
}
