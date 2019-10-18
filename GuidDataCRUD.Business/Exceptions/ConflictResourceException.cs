using System;
using System.Collections.Generic;
using System.Text;

namespace GuidDataCRUD.Business.Exceptions
{
    /// <summary>
    /// Exception raised when resource identifer already exist in store (e.g, database)
    /// </summary>
    public class ConflictResourceException: Exception
    {
        public ConflictResourceException() : base("Conflict Resource")
        { }

        public ConflictResourceException(Exception ex) : base("Conflict Resource", ex)
        { }
    }
}
