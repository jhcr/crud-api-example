using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GuidDataCRUD.Business.Contracts;

namespace GuidDataCRUD.Business
{
    /// <summary>
    /// Interface of <see cref="GuidData"/> business logic
    /// </summary>
    public interface IGuidDataService
    {
        Task<GuidDataResponse> GetGuidData(string guid);
        Task<(GuidDataResponse, bool)> UpsertGuidData(string guid, GuidDataRequest request);
        Task<GuidDataResponse> CreateGuidData(GuidDataRequest request);
        Task<bool> DeleteGuidData(string guid);
    }
}
