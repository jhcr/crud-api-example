using System;
using System.Collections.Generic;
using System.Text;

namespace GuidDataCRUD.Business.Models
{
    /// <summary>
    /// GuidData domain model
    /// </summary>
    public class GuidData
    {
        /// <summary>
        /// Identifier of the GuidData
        /// </summary>
        public Guid Guid { get; set;}

        /// <summary>
        /// User of GuidData
        /// </summary>
        public string User { get; set;}

        /// <summary>
        /// Absolute expiration of GuidData (Unix Epoc time)
        /// </summary>
        public long Expire { get; set; }
    }
}
