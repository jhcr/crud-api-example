using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GuidDataCRUD.Web.Controllers
{
    /// <summary>
    /// Controller base
    /// </summary>
    public abstract class CustomControllerBase: ControllerBase
    {
        protected ActionResult Result(HttpStatusCode statusCode, object value)
        {
            return new ObjectResult(value){ StatusCode = (int)statusCode};
        }

        protected ActionResult Result(HttpStatusCode statusCode)
        {
            return Result(statusCode, null);
        }
    }
}
