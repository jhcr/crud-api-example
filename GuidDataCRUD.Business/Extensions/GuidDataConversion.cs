using System;
using System.Collections.Generic;
using System.Text;


namespace GuidDataCRUD.Business.Extensions
{
    /// <summary>
    /// Static extension for all DTO conversion
    /// </summary>
    public static class GuidDataConversion
    {
        /// <summary>
        /// Convert GuidData Contract to Model
        /// </summary>
        /// <param name="contract">The contract</param>
        /// <param name="defaultExpireDays">default of 30 days from the time of creation, if an expiration time is not provided.</param>
        /// <returns></returns>
        public static Models.GuidData ToModel(this Contracts.GuidDataRequest contract, int defaultExpireDays)
        {
            if (contract == null)
                return null;

            var model = new Models.GuidData
            {
                User = contract.User
            };

            if (long.TryParse(contract.Expire, out var expireValue))
                model.Expire = expireValue;
            else
                model.Expire = DateTimeOffset.UtcNow.AddDays(1.0 * defaultExpireDays).ToUnixTimeSeconds();

            return model;

        }

        /// <summary>
        /// Convert GuidData Model to Contract
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Contracts.GuidDataResponse ToContract(this Models.GuidData model)
        {
            if (model == null)
                return null;

            return new Contracts.GuidDataResponse
            {
                Guid = model.Guid.ToString("N").ToUpper(),
                User = model.User,
                Expire = model.Expire.ToString()
            };

        }

    }
}
