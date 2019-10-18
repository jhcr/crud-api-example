using System;
using System.Collections.Generic;
using System.Text;
using GuidDataCRUD.Business.Contracts;
using GuidDataCRUD.Business.Exceptions;

namespace GuidDataCRUD.Business.Validators
{
    /// <summary>
    /// Validator for GuidDataRequest
    /// </summary>
    public static class GuidDataRequestValidator
    {
        public static void EnsureValid(GuidDataRequest data, string parent = null)
        {
            if (!ValidationFunctions.MaxLengthNullable(data.User, 255, out var messageUser))
            {
                throw new ValidationException("user"._path(parent), messageUser);
            }
            if (!ValidationFunctions.ExpireNullable(data.Expire, out var expire, out var messageExpire))
            {
                throw new ValidationException("expire"._path(parent), messageExpire);
            }
        }
    }
}
