using System;
using System.Threading.Tasks;
using GuidDataCRUD.Business.Models;

namespace GuidDataCRUD.Business
{
    /// <summary>
    /// Interface of repository sepcific to <see cref="GuidData"/>
    /// </summary>
    /// <remarks>
    /// Reside in Business layer to reverse the dependency flow from Repository to Business
    /// </remarks>
    public interface IGuidDataRepository
    {
        Task<GuidData> GetGuidData(Guid guid);
        Task<(GuidData, bool)> UpsertGuidData(GuidData data);
        Task<bool> DeleteGuidData(Guid guid);
        Task<GuidData> CreateGuidData(GuidData GuidData);
    }
}
