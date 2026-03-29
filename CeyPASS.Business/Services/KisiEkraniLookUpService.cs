using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.DataAccess.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class KisiEkraniLookupService : IKisiEkraniLookUpService
    {
        private readonly ICalismaStatuRepository _statuRepo;
        private readonly IDepartmanRepository _departmanRepo;
        private readonly IPozisyonRepository _pozisyonRepo;
        private readonly IIsyeriRepository _isyeriRepo;
        private readonly IFirmaRepository _firmaRepo;
        private readonly IBolumRepository _bolumRepo;
        private readonly IMemoryCache _cache;

        private const string CacheKeyPrefix = "lookup_";
        private const string MasterVerKey = "lookup_master_version";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public KisiEkraniLookupService(
            ICalismaStatuRepository statuRepo, 
            IDepartmanRepository departmanRepo, 
            IPozisyonRepository pozisyonRepo, 
            IIsyeriRepository isyeriRepo, 
            IFirmaRepository firmaRepo, 
            IBolumRepository bolumRepo,
            IMemoryCache cache)
        {
            _statuRepo = statuRepo;
            _departmanRepo = departmanRepo;
            _pozisyonRepo = pozisyonRepo;
            _isyeriRepo = isyeriRepo;
            _firmaRepo = firmaRepo;
            _bolumRepo = bolumRepo;
            _cache = cache;
        }

        public List<LookupItem> GetCalismaStatuleri(int? firmId = null) 
        {
            var keyStr = firmId.HasValue ? $"statuler_{firmId}" : "statuler";
            return GetCached(keyStr, () => _statuRepo.GetByFirma());
        }

        public List<LookupItem> GetDepartmanlar(int? firmId = null) 
        {
            var keyStr = firmId.HasValue ? $"departmanlar_{firmId}" : "departmanlar";
            return GetCached(keyStr, () => _departmanRepo.GetByFirma());
        }

        public List<LookupItem> GetPozisyonlar(int? firmId = null) 
        {
            var keyStr = firmId.HasValue ? $"pozisyonlar_{firmId}" : "pozisyonlar";
            return GetCached(keyStr, () => _pozisyonRepo.GetByFirma());
        }

        public List<LookupItem> GetIsyerleri(int firmId) 
        {
            return GetCached($"isyerleri_{firmId}", () => _isyeriRepo.GetByFirma(firmId));
        }

        public List<LookupItem> GetFirma(int firmId) 
        {
            return _firmaRepo.GetSingle(firmId);
        }

        public List<LookupItem> GetBolumler(int firmId) 
        {
            return GetCached($"bolumler_{firmId}", () => _bolumRepo.GetByFirma(firmId));
        }

        public void InvalidateCache()
        {
            if (!_cache.TryGetValue(MasterVerKey, out int ver))
                ver = 0;
            
            ver++;
            _cache.Set(MasterVerKey, ver, TimeSpan.FromDays(1));
        }

        private List<LookupItem> GetCached(string keySuffix, Func<List<LookupItem>> factory)
        {
            if (!_cache.TryGetValue(MasterVerKey, out int ver))
            {
                ver = 0;
                _cache.Set(MasterVerKey, ver, TimeSpan.FromDays(1));
            }

            var key = $"{CacheKeyPrefix}{keySuffix}_v{ver}";
            if (!_cache.TryGetValue(key, out List<LookupItem> items))
            {
                items = factory();
                _cache.Set(key, items, CacheDuration);
            }
            return items;
        }
    }
}
