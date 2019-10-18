using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;

namespace GuidDataCRUD.Web.Middlewares
{
    /// <summary>
    /// Global error handling on HttpContext
    /// </summary>
    public static class GlobalErrorHandler
    {
        public static void UseGlobalErrorHandler(this IApplicationBuilder app, ILogger logger)
        {
            app.UseExceptionHandler(configure =>
            {
                configure.Run(async context =>
                {
                    var problemDetails = default(ProblemDetails);

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if (contextFeature?.Error != null)
                    {
                        if (contextFeature.Error is BadHttpRequestException badHttpRequestException)
                        {
                            problemDetails = new ProblemDetails
                            {
                                Title = "Invalid request",
                                Status = badHttpRequestException.StatusCode,
                                Detail = badHttpRequestException.Message
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

                        logger.LogError(contextFeature.Error, problemDetails.Detail);

                        problemDetails?.Extensions.Add("traceId", context.TraceIdentifier);

                        context.Response.ContentType = "application/problem+json";
                        context.Response.StatusCode = problemDetails.Status?? (int)HttpStatusCode.InternalServerError;
                        await context.Response.WriteAsync(problemDetails.ToString());
                    }

                });
            });
        }
    }
}
