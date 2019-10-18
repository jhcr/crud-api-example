using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GuidDataCRUD.Business.Validators
{
    /// <summary>
    /// Static Class for all validation functions for reuse 
    /// </summary>
    public static class ValidationFunctions
    {
        /// <summary>
        /// Validate a string is required
        /// </summary>
        /// <param name="input"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool StringRequired(string input, out string message)
        {
            message = string.Empty;
            if (!string.IsNullOrWhiteSpace(input))
            {
                return true;
            }
            else
            {
                message = "must be provided";
                return false;
            }
        }

        /// <summary>
        /// Validate max length limit
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxLength"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool MaxLengthNullable(string input, int maxLength, out string message)
        {
            message = string.Empty;
            if (!string.IsNullOrWhiteSpace(input))
            {
                if (maxLength > 0 && input.Length > maxLength)
                {
                    message = $"must not exceed {maxLength} characters";
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Validate the specific Guid format (32-bit hexadecimal in uppercase)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool GuidNullable(string input, out Guid? value, out string message)
        {
            var defaultErrorMessage = "must be 32-bit hexadecimal in uppercase";

            message = string.Empty;
            value = null;
            if (input == null)
            {
                return true;
            }
            else if (new Regex("^[0-9A-F]{32}$").IsMatch(input))
            {
                if (Guid.TryParse(input, out var guidValue))
                {
                    value = guidValue;
                    return true;
                }
                else
                {
                    message = defaultErrorMessage;
                    return false;
                }
            }
            else
            {
                message = defaultErrorMessage;
                return false;
            }
            
        }

        /// <summary>
        /// validate the specific expire format (UNIX Epoch time)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ExpireNullable(string input, out long? value, out string message)
        {
            message = string.Empty;
            value = null;

            if (input == null)
            {
                return true;
            }
            else if (long.TryParse(input, out var expireValue))
            {
                if (expireValue <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    message = "must be in future";
                    return false;
                }
                else if (expireValue > DateTimeOffset.MaxValue.ToUnixTimeSeconds())
                {
                    message = $"must not exceed {DateTimeOffset.MaxValue.ToUnixTimeSeconds()}";
                    return false;
                }
               else    
                    return true;
            }
            else
            {
                message = "must be a number if provided";
                return false;
            }
            
        }

        internal static string _path(this string currentProperty, string parentProperty, string seperator = ".")
        {
            return string.IsNullOrWhiteSpace(parentProperty) ? currentProperty: $"{parentProperty}{seperator}{currentProperty}";
        }
    }
}
