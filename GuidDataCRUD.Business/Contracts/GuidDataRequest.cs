using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuidDataCRUD.Business.Contracts
{
    /// <summary>
    /// GuidData Request
    /// </summary>
    public class GuidDataRequest
    {
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
