using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using GuidDataCRUD.Business.Contracts;
using GuidDataCRUD.Business.Extensions;
using GuidDataCRUD.Business.Models;

namespace GuidDataCRUD.Business
{
    /// <summary>
    /// Concrete implementation of <see cref="IGuidDataService"/> interface
    /// </summary>
    /// <remarks>
    /// This defines all the business logic specific to <see cref="GuidData"/> 
    /// </remarks>
    public class GuidDataService: IGuidDataService
    {
        private readonly IGuidDataRepository _repo;
        private readonly int _defaultExpireDays; 

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repo">The GuidData repository</param>
        /// <param name="settings" cref="AppSettings">The settings from app config</param>
        public GuidDataService(IGuidDataRepository repo, IOptions<AppSettings> settings)
        {
            _repo = repo;
            _defaultExpireDays = settings?.Value.DefaultExpireDays ?? 30;
        }

        /// <summary>
        /// Get a specific GuidData
        /// </summary>
        /// <param name="guid">The guid</param>
        /// <returns></returns>
        async Task<GuidDataResponse> IGuidDataService.GetGuidData(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException();

            var result = await _repo.GetGuidData(Guid.Parse(guid));

            return result?.ToContract();
        }
        /// <summary>
        /// Upsert a specific GuidData
        /// </summary>
        /// <param name="guid">The guid</param>
        /// <param name="request">The guidData request</param>
        /// <returns>the affected GuidData</returns>
        async Task<(GuidDataResponse, bool)> IGuidDataService.UpsertGuidData(string guid, GuidDataRequest request)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException();
            if(request == null)
                throw new ArgumentNullException();

            var model = request.ToModel(_defaultExpireDays);

            model.Guid = Guid.Parse(guid);

            var (result, isUpdated) = await _repo.UpsertGuidData(model);

            return (result?.ToContract(), isUpdated);
        }

        /// <summary>
        /// Create a new GuidData
        /// </summary>
        /// <param name="request">The guidData request</param>
        /// <returns>the updated GuidData</returns>
        async Task<GuidDataResponse> IGuidDataService.CreateGuidData(GuidDataRequest request)
        {
            if (request == null)
                throw new ArgumentNullException();

            var model = request.ToModel(_defaultExpireDays);

            model.Guid = Guid.NewGuid();

            var result = await _repo.CreateGuidData(model);

            return result?.ToContract();
        }

        /// <summary>
        /// Delete a specific GuidData
        /// </summary>
        /// <param name="guid">the guid</param>
        /// <returns><c>true</c> if found and deleted; otherwise, <c>false</c></returns>
        async Task<bool> IGuidDataService.DeleteGuidData(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException();

            return await _repo.DeleteGuidData(Guid.Parse(guid));
        }
    }
}
