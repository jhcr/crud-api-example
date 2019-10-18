using System;
using System.Collections.Generic;
using System.Text;

namespace GuidDataCRUD.Business.Contracts
{
    /// <summary>
    /// GuidData Response
    /// </summary>
    public class GuidDataResponse
    {
        /// <summary>
        /// Identifier of the GuidData
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// User of the GuidData
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Absolute expiration of the GuidData (Unix Epoc time)
        /// </summary>
        public string Expire { get; set; }

    }
}
