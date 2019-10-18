using System;
using System.Collections.Generic;
using System.Text;
using GuidDataCRUD.Business.Exceptions;

namespace GuidDataCRUD.Business.Validators
{
    public static class GuidValidator
    {
        /// <summary>
        /// Validator for Guid
        /// </summary>
        public static void EnsureValid(string data, string parent = null)
        {
            var property = "guid"._path(parent);
            if (!ValidationFunctions.StringRequired(data, out var messageRequired))
            {
                throw new ValidationException(property, messageRequired);
            }

            if (!ValidationFunctions.GuidNullable(data, out var guid, out var messageGuid))
            {
                throw new ValidationException(property, messageGuid);
            }

        }
    }
}
