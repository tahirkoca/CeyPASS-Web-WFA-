using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CeyPASS.Business.Services
{
    public class FcmPushService : IPushNotificationService
    {
        private readonly IUserDeviceTokenRepository _tokenRepository;
        private readonly ILogger<FcmPushService> _logger;
        private readonly IConfiguration _configuration;
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private const string ExpoPushUrl = "https://exp.host/--/api/v2/push/send";

        public FcmPushService(
            IUserDeviceTokenRepository tokenRepository,
            ILogger<FcmPushService> logger,
            IConfiguration configuration)
        {
            _tokenRepository = tokenRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendPushToUserAsync(string? personelId, string? kullaniciId, string title, string body, object? data = null)
        {
            var tokens = _tokenRepository.GetTokensByUser(personelId, kullaniciId);
            if (tokens == null || tokens.Count == 0) return;

            // Expo Go: tokens look like "ExponentPushToken[xxxxx]". Keep legacy FCM tokens ignored for now.
            var expoTokens = tokens.Where(IsExpoToken).Distinct().ToList();
            if (expoTokens.Count == 0) return;

            await SendExpoBatchAsync(expoTokens, title, body, data);
        }

        private static bool IsExpoToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            var t = token.Trim();
            return t.StartsWith("ExponentPushToken[", StringComparison.OrdinalIgnoreCase) && t.EndsWith("]");
        }

        private async Task SendExpoBatchAsync(List<string> expoTokens, string title, string body, object? data)
        {
            try
            {
                // Expo limit: send up to 100 messages per request.
                const int batchSize = 100;
                for (int i = 0; i < expoTokens.Count; i += batchSize)
                {
                    var batch = expoTokens.Skip(i).Take(batchSize).ToList();
                    var messages = batch.Select(t => new
                    {
                        to = t,
                        title,
                        body,
                        data
                    }).ToList();

                    var json = JsonSerializer.Serialize(messages, _jsonOptions);
                    using var req = new HttpRequestMessage(HttpMethod.Post, ExpoPushUrl)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };

                    _logger.LogInformation("Expo push sending batch: {Count} msgs", messages.Count);

                    using var res = await _httpClient.SendAsync(req);
                    var resText = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Expo push failed HTTP {Status}. Body: {Body}", (int)res.StatusCode, resText);
                        continue;
                    }

                    // Best-effort parsing for logging; don't fail the caller.
                    try
                    {
                        using var doc = JsonDocument.Parse(resText);
                        if (doc.RootElement.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
                        {
                            int ok = 0, err = 0;
                            foreach (var it in dataArr.EnumerateArray())
                            {
                                var status = it.TryGetProperty("status", out var st) ? st.GetString() : null;
                                if (string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase)) ok++;
                                else err++;
                            }
                            _logger.LogInformation("Expo push result: ok={Ok} err={Err}", ok, err);
                        }
                    }
                    catch
                    {
                        _logger.LogInformation("Expo push response (unparsed): {Body}", resText);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Expo push send error");
            }
        }
    }
}
