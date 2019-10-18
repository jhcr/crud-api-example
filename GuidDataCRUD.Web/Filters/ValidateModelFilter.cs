using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GuidDataCRUD.Business.Exceptions;

namespace GuidDataCRUD.Web.Filters
{
    public class ValidateModelFilter: ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelFilter> _logger;

        public ValidateModelFilter(ILogger<ValidateModelFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handle model state errors
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                _logger.LogError($"Validation error, {JsonConvert.SerializeObject(actionContext.ModelState)}");

                actionContext.Result = new BadRequestObjectResult(actionContext.ModelState);
            }
        }

        /// <summary>
        /// Handle model state errors and validation exceptions on ActionContext
        /// </summary>
        /// <param name="executedContext"></param>
        public override void OnActionExecuted(ActionExecutedContext executedContext)
        {
            if (!executedContext.ModelState.IsValid)
            {
                _logger.LogError($"Validation error, {JsonConvert.SerializeObject(executedContext.ModelState)}");

                executedContext.Result = new BadRequestObjectResult(executedContext.ModelState);
            }
            else if (executedContext.Exception != null)
            {
                executedContext.ExceptionHandled = true;

                var problemDetails = default(ProblemDetails);

                if (executedContext.Exception is ValidationException validationException)
                {
                    var errors = new Dictionary<string,string[]>() {
                        [validationException.Property] = new string[] { validationException.ValidationMessage }
                    };

                    problemDetails = new ValidationProblemDetails(errors)
                    {
                        Title = "Validation error",
                        Status = validationException.StatusCode,
                        Detail = validationException.Message
                    };  
                }
                else
                {
                    problemDetails = new ProblemDetails
                    {
                        Title = "Unexpected error",
                        Status = (int)HttpStatusCode.InternalServerError,
                        Detail = "An unexpected error is encountered"
                    };
                }

                _logger.LogError(executedContext.Exception, problemDetails.Detail);

                if (problemDetails != null)
                {
                    problemDetails.Extensions.Add("traceId", executedContext.HttpContext.TraceIdentifier);

                    executedContext.Result = new ObjectResult(problemDetails) { StatusCode = problemDetails?.Status ?? (int)HttpStatusCode.InternalServerError };
                }
            }

            
        }
    }
}
