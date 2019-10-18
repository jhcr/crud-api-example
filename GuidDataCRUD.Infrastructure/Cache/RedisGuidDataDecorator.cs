using GuidDataCRUD.Business;
using GuidDataCRUD.Business.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace GuidDataCRUD.Infrastructure.Cache
{
    /// <summary>
    /// Add redis cache and delegate GuidData CRUD operations to decorated repository
    /// </summary>
    public class RedisGuidDataDecorator : IGuidDataRepository
    {
        IGuidDataRepository _decorated;
        IDistributedCache _cache;

        private const string _prefix = "GuidData_";

        private static Func<Guid, string> _cacheKey => (Guid guid) =>  _prefix + guid.ToString("N").ToUpper();

        private static Func<long, DistributedCacheEntryOptions> _expire => (long expire) =>
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(expire)
            };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="decorated">The repository delegate further processing data</param>
        /// <param name="cache">The distributed cache</param>
        public RedisGuidDataDecorator(IGuidDataRepository decorated, IDistributedCache cache)
        {
            _decorated = decorated;
            _cache = cache;
        }

        /// <summary>
        /// Set cache after data created
        /// </summary>
        /// <param name="guidData"  cref="GuidData">The guidData</param>
        /// <returns>The newly created GuidData</returns>
        public async Task<GuidData> CreateGuidData(GuidData guidData)
        {
            var created = await _decorated.CreateGuidData(guidData);
            if (created != null)
            {
                await _cache.SetStringAsync(_cacheKey(created.Guid), JsonConvert.SerializeObject(created), _expire(created.Expire));
            }
            return created;
        }

        /// <summary>
        /// Set cache after data created or updated
        /// </summary>
        /// <param name="guidData">The guidData</param>
        /// <returns>The affected data and: <c>true</c> if updating was performed vs creating; otherwise, <c>false</c></returns>
        public async Task<(GuidData, bool)> UpsertGuidData(GuidData guidData)
        {
            var (upserted, isUpdated) = await _decorated.UpsertGuidData(guidData);
            if (upserted != null)
            {
                await _cache.SetStringAsync(_cacheKey(upserted.Guid), JsonConvert.SerializeObject(upserted), _expire(upserted.Expire));
            }
            return (upserted, isUpdated);
        }

        /// <summary>
        /// Remove from cache after data deleted
        /// </summary>
        /// <param name="guid">The guid</param>
        /// <returns>True if found and deleted</returns>
        public async Task<bool> DeleteGuidData(Guid guid)
        {
            var success = await _decorated.DeleteGuidData(guid);
            if (success)
            {
                await _cache.RemoveAsync(_cacheKey(guid));
            }
            return success;
        }

        /// <summary>
        /// Get from cache, if not cached, get from repository, then set cache
        /// </summary>
        /// <param name="guid">The guid</param>
        /// <returns>The GuidData</returns>
        public async Task<GuidData> GetGuidData(Guid guid)
        {
            var cahced = await _cache.GetStringAsync(_cacheKey(guid));
            if (cahced != null)
            {
                return JsonConvert.DeserializeObject<GuidData>(cahced);
            }
            else
            {
                var stored = await _decorated.GetGuidData(guid);
                if (stored != null)
                {
                    await _cache.SetStringAsync(_cacheKey(guid), JsonConvert.SerializeObject(stored), _expire(stored.Expire));
                }
                return stored;
            }
        }
    }
}
