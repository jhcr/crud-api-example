using GuidDataCRUD.Business.Models;
using GuidDataCRUD.Business;
using System;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using GuidDataCRUD.Business.Exceptions;
using Microsoft.Extensions.Options;

namespace GuidDataCRUD.Infrastructure.Database
{
    /// <summary>
    /// Concrete implementation of <see cref="IGuidDataRepository"/> interface, using SQL database
    /// </summary>
    public class SqlGuidDataRepository: IGuidDataRepository
    {
        private readonly string _connectionString;
        private readonly int _commandTimeoutInSeconds;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="setting">The app settings</param>
        public SqlGuidDataRepository(IOptions<AppSettings> setting)
        {
            if (string.IsNullOrWhiteSpace(setting?.Value?.Sql_ConnectionString_GuidData))
                throw new ArgumentNullException("Sql_ConnectionString_GuidData cannot be empty");

            _connectionString = setting?.Value?.Sql_ConnectionString_GuidData;
            _commandTimeoutInSeconds = setting?.Value?.Sql_CommandTimeoutInSeconds?? 30;
        }

        /// <summary>
        /// Get a specific guid and its data
        /// </summary>
        /// <param name="guid">The guid</param>
        /// <returns>the found guidData</returns>
        async Task<GuidData> IGuidDataRepository.GetGuidData(Guid guid)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                db.Open();

                return await db.QueryFirstOrDefaultAsync<GuidData>(
                    "dbo.GetGuidDataByGuid",
                    new { guid },
                    commandTimeout: _commandTimeoutInSeconds,
                    commandType: CommandType.StoredProcedure);
            }
            
        }

        /// <summary>
        /// Create GuidData
        /// </summary>
        /// <param name="guidData" cref="GuidData">The guidData</param>
        /// <returns>The created guidData</returns>
        /// <exception cref="ConflictResourceException">If the guid already exists</exception>
        async Task<GuidData> IGuidDataRepository.CreateGuidData(GuidData guidData)
        {
            try
            {
                using (var db = new SqlConnection(_connectionString))
                {
                    db.Open();

                    await db.ExecuteAsync(
                        "dbo.CreateGuidData",
                        new { guid = guidData.Guid, user = guidData.User, expire = guidData.Expire },
                        commandTimeout: _commandTimeoutInSeconds,
                        commandType: CommandType.StoredProcedure);

                    return guidData;
                }
            }
            catch (SqlException ex) when (ex.Number == 2601 ||ex.Number == 2627)
            {
                throw new ConflictResourceException(ex);
            }
        }

        /// <summary>
        /// Upsert GuidData
        /// </summary>
        /// <param name="guidData" cref="GuidData">The guidData</param>
        /// <returns>The affected data and: <c>true</c> if updating was performed vs creating; otherwise, <c>false</c></returns>
        async Task<(GuidData, bool)> IGuidDataRepository.UpsertGuidData(GuidData guidData)
        {
            var result = default(GuidData);
            var isUpdated = true;

            using (var db = new SqlConnection(_connectionString))
            {
                db.Open();

                using (var tran = db.BeginTransaction())
                {
                    result = await db.QueryFirstOrDefaultAsync<GuidData>(
                    "dbo.UpdateGuidData",
                    new { guid = guidData.Guid, user = guidData.User, expire = guidData.Expire },
                    tran,
                    commandTimeout: _commandTimeoutInSeconds,
                    commandType: CommandType.StoredProcedure);

                    if (result == null)
                    {
                        isUpdated = false;
                        result = await db.QueryFirstOrDefaultAsync<GuidData>(
                        "dbo.CreateGuidData",
                        new { guid = guidData.Guid, user = guidData.User, expire = guidData.Expire },
                        tran,
                        commandTimeout: _commandTimeoutInSeconds,
                        commandType: CommandType.StoredProcedure);
                    }

                    tran.Commit();
                }
            }
            return (result, isUpdated);
        }

        /// <summary>
        /// Delete a guid and its data
        /// </summary>
        /// <param name="guid">the guid</param>
        /// <returns><c>true</c> if found and deleted; otherwise, <c>false</c></returns>
        async Task<bool> IGuidDataRepository.DeleteGuidData(Guid guid)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                db.Open();

                var affected = await db.ExecuteAsync(
                    "dbo.DeleteGuidData",
                    new { guid},
                    commandTimeout: _commandTimeoutInSeconds,
                    commandType: CommandType.StoredProcedure);

                return affected > 0;

            }
        }

        
    }
}
