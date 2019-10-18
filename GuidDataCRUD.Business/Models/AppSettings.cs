using System;
using System.Collections.Generic;
using System.Text;

namespace GuidDataCRUD.Business.Models
{
    /// <summary>
    /// Mapping all configuration items from json config
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// The default expire when value not proivided in request
        /// </summary>
        public int DefaultExpireDays { get; set; }
        /// <summary>
        /// Database connection string for GuidData Database
        /// </summary>
        public string Sql_ConnectionString_GuidData { get; set; }
        /// <summary>
        /// The sql command timeout in seconds
        /// </summary>
        public int Sql_CommandTimeoutInSeconds { get; set; }
    }
}
