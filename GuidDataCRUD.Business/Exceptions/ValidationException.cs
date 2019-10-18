using System;
using System.Collections.Generic;
using System.Text;

namespace GuidDataCRUD.Business.Exceptions
{
    /// <summary>
    /// Exception raised when data validation failed
    /// </summary>
    public class ValidationException : Exception
    {
        public int StatusCode { get; set; }
        public string Property { get; set; }
        public string ValidationMessage { get; set; }

        public ValidationException(string property, string validationMessage) : base($"{property}: {validationMessage}")
        {
            Property = property;
            ValidationMessage = validationMessage;
            StatusCode = 400;
        }

        public ValidationException(int statusCode, string property, string validationMessage) : base($"{property}: {validationMessage}")
        {
            Property = property;
            Property = validationMessage;
            StatusCode = statusCode;
        }
    }

    
}
