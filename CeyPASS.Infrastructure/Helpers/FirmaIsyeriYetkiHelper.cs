using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Infrastructure.Helpers
{
    public static class FirmaIsyeriYetkiHelper
    {
        public static bool IsAdmin(int? rolId) => rolId == 1 || rolId == 2;

        public static List<Firma> FilterFirmalar(IEnumerable<Firma> firmalar, IReadOnlyList<FirmaIsyeriYetkiDTO> yetkiler, bool isAdmin)
        {
            var list = firmalar?.ToList() ?? new List<Firma>();
            if (isAdmin || yetkiler == null || yetkiler.Count == 0)
                return list;

            var ids = yetkiler.Select(y => y.FirmaId).Distinct().ToHashSet();
            return list.Where(f => ids.Contains(f.FirmaId)).ToList();
        }

        public static List<IsyeriItem> FilterIsyeriler(IEnumerable<IsyeriItem> isyerleri, int firmaId, IReadOnlyList<FirmaIsyeriYetkiDTO> yetkiler, bool isAdmin)
        {
            var list = isyerleri?.ToList() ?? new List<IsyeriItem>();
            if (isAdmin || yetkiler == null || yetkiler.Count == 0)
                return list;

            var firmaYetkileri = yetkiler.Where(y => y.FirmaId == firmaId).ToList();
            if (firmaYetkileri.Count == 0)
                return new List<IsyeriItem>();

            if (firmaYetkileri.Any(y => !y.IsyeriId.HasValue))
                return list;

            var ids = firmaYetkileri
                .Where(y => y.IsyeriId.HasValue)
                .Select(y => y.IsyeriId!.Value)
                .ToHashSet();

            return list.Where(i => ids.Contains(i.IsyeriId)).ToList();
        }

        public static bool IsFirmaAuthorized(int firmaId, IReadOnlyList<FirmaIsyeriYetkiDTO> yetkiler, bool isAdmin)
        {
            if (isAdmin || yetkiler == null || yetkiler.Count == 0)
                return true;
            return yetkiler.Any(y => y.FirmaId == firmaId);
        }

        public static bool IsIsyeriAuthorized(int firmaId, int isyeriId, IReadOnlyList<FirmaIsyeriYetkiDTO> yetkiler, bool isAdmin)
        {
            if (isAdmin || yetkiler == null || yetkiler.Count == 0)
                return true;
            if (!IsFirmaAuthorized(firmaId, yetkiler, false))
                return false;

            var firmaYetkileri = yetkiler.Where(y => y.FirmaId == firmaId).ToList();
            if (firmaYetkileri.Any(y => !y.IsyeriId.HasValue))
                return true;
            if (isyeriId <= 0)
                return true;

            return firmaYetkileri.Any(y => y.IsyeriId == isyeriId);
        }

        public static List<LookupItem> FilterIsyeriLookup(IEnumerable<LookupItem> items, int firmaId, IReadOnlyList<FirmaIsyeriYetkiDTO> yetkiler, bool isAdmin)
        {
            var list = items?.ToList() ?? new List<LookupItem>();
            if (isAdmin || yetkiler == null || yetkiler.Count == 0)
                return list;

            return list.Where(i => i.Id <= 0 || IsIsyeriAuthorized(firmaId, i.Id, yetkiler, false)).ToList();
        }

        /// <summary>
        /// Kişi listesi sorgusu için işyeri filtresi.
        /// single dolu → tek işyeri; isyeriIdIn dolu → IN ("Tümü" + kısıtlı yetki);
        /// ikisi null → işyeri şartı yok; boş isyeriIdIn → firmaya yetki yok.
        /// </summary>
        public static (int? singleIsyeriId, IReadOnlyList<int>? isyeriIdIn) ResolveKisiQueryIsyeriFilter(
            int firmaId,
            int? selectedIsyeriId,
            IReadOnlyList<FirmaIsyeriYetkiDTO> yetkiler,
            bool isAdmin)
        {
            if (selectedIsyeriId.HasValue && selectedIsyeriId.Value > 0)
                return (selectedIsyeriId.Value, null);

            if (isAdmin || yetkiler == null || yetkiler.Count == 0)
                return (null, null);

            var firmaYetkileri = yetkiler.Where(y => y.FirmaId == firmaId).ToList();
            if (firmaYetkileri.Count == 0)
                return (null, Array.Empty<int>());

            if (firmaYetkileri.Any(y => !y.IsyeriId.HasValue))
                return (null, null);

            var ids = firmaYetkileri
                .Where(y => y.IsyeriId.HasValue)
                .Select(y => y.IsyeriId!.Value)
                .Distinct()
                .ToList();

            return (null, ids);
        }

        public enum RaporIsyeriListStatus
        {
            Ok,
            NoAccess,
            UnauthorizedSelection
        }

        public static List<int> ParseIsyeriIds(string? isyeriIds)
        {
            if (string.IsNullOrWhiteSpace(isyeriIds))
                return new List<int>();

            return isyeriIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : -1)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Rapor SP @IsyeriIdList: yetki tavanı (maxCsv) ile kullanıcı seçimini birleştirir.
        /// Seçim yoksa tavan; seçim varsa yetki doğrulaması sonrası alt küme (+0).
        /// </summary>
        public static (string? csv, RaporIsyeriListStatus status) ResolveRaporIsyeriIdListCsv(
            int firmaId,
            IReadOnlyList<int>? selectedIsyeriIds,
            string? maxIsyeriIdCsv,
            IReadOnlyList<FirmaIsyeriYetkiDTO>? yetkiler,
            bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(maxIsyeriIdCsv))
                return (null, RaporIsyeriListStatus.NoAccess);

            if (selectedIsyeriIds == null || selectedIsyeriIds.Count == 0)
                return (maxIsyeriIdCsv, RaporIsyeriListStatus.Ok);

            var selected = selectedIsyeriIds.Where(id => id > 0).Distinct().ToList();
            if (selected.Count == 0)
                return (maxIsyeriIdCsv, RaporIsyeriListStatus.Ok);

            foreach (var id in selected)
            {
                if (!IsIsyeriAuthorized(firmaId, id, yetkiler, isAdmin))
                    return (null, RaporIsyeriListStatus.UnauthorizedSelection);
            }

            var result = new List<int>(selected);
            if (!result.Contains(0))
                result.Add(0);

            return (string.Join(",", result), RaporIsyeriListStatus.Ok);
        }

        /// <summary>Non-admin: AktifFirmaId yetkili firmalardan biri olmalı.</summary>
        public static int? ResolveAktifFirmaId(int? kullaniciFirmaId, IReadOnlyList<FirmaIsyeriYetkiDTO> yetkiler, bool isAdmin)
        {
            if (isAdmin)
                return kullaniciFirmaId;

            if (yetkiler == null || yetkiler.Count == 0)
                return kullaniciFirmaId;

            var ids = yetkiler.Select(y => y.FirmaId).Distinct().ToList();
            if (ids.Count == 0)
                return kullaniciFirmaId;

            if (kullaniciFirmaId.HasValue && ids.Contains(kullaniciFirmaId.Value))
                return kullaniciFirmaId;

            return ids[0];
        }
    }
}
