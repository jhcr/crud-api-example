using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using GuidDataCRUD.Business;
using GuidDataCRUD.Business.Contracts;
using GuidDataCRUD.Business.Validators;

namespace GuidDataCRUD.Web.Controllers
{
    /// <summary>
    /// GuidData Controller
    /// </summary>
    [Route("guid")]
    [ApiController]
    [Produces("application/json", "application/problem+json")]
    public class GuidDataController : CustomControllerBase
    {
        private readonly IGuidDataService _provider;
        private readonly ILogger<GuidDataController> _logger;

        public GuidDataController(IGuidDataService provider, ILogger<GuidDataController> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        /// <summary>
        /// Get a specific guid and its data
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /guid/3BED9CEFBB9F43E99DFE3EAE6CC5847F
        ///     
        /// </remarks>
        /// <param name="guid">The guid, must be 32-bit hexadecimal in uppercase</param>
        /// <returns>The guid data being found</returns>
        /// <response code="200">Returns the guid data being found</response>
        /// <response code="404">If the guid does not exist</response>     
        /// <response code="400">If the guid is invalid</response>   
        /// <response code="500">If any unexpected error</response>   
        [HttpGet("{guid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<GuidDataResponse>> GetGuidData(string guid)
        {

            GuidValidator.EnsureValid(guid);

            var response = await _provider.GetGuidData(guid);
            if (response != null)
                return Ok(response);
            else
                return NotFound();
           
        }


        /// <summary>
        /// Create a random guid with given data
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /guid
        ///     {
        ///        "user": "user1",
        ///        "expire": "1427736345"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">The data</param>
        /// <returns>The newly created guid data</returns>
        /// <response code="201">Returns the newly created guid data</response>
        /// <response code="400">If the guid is invalid</response>  
        /// <response code="500">If any unexpected error, e.g, the auto-generated guid already exists</response>   
        [HttpPost("")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<GuidDataResponse>> CreateGuidData(GuidDataRequest request)
        {
            GuidDataRequestValidator.EnsureValid(request);

            var response = await _provider.CreateGuidData(request);
            return Created($"{UriHelper.GetEncodedUrl(Request)}/{response.Guid}", response);
        }

        /// <summary>
        /// Idempotent insert/update a specific guid
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /guid/3BED9CEFBB9F43E99DFE3EAE6CC5847F
        ///     {
        ///        "user": "user1",
        ///        "expire": "1427736345"
        ///     }
        ///     
        /// </remarks>   
        /// <param name="guid">The guid, must be 32-bit hexadecimal in uppercase</param>
        /// <param name="request">The data</param>
        /// <returns>The affected guid data</returns>
        /// <response code="200">Returns the updated guid data</response>
        /// <response code="201">Returns the newly created guid data</response>
        /// <response code="400">If the guid or request is invalid</response>  
        /// <response code="500">If any unexpected error</response>   
        [HttpPost("{guid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<GuidDataResponse>> UpsertGuidData(string guid, GuidDataRequest request)
        {
            GuidValidator.EnsureValid(guid);
            GuidDataRequestValidator.EnsureValid(request);

            var (response, isUpdated) = await _provider.UpsertGuidData(guid, request);
            if (isUpdated)
            {
                return Ok(response);
            }
            else
            {
                return Created(UriHelper.GetEncodedUrl(Request), response);
            }
           
        }

        /// <summary>
        /// Delete a specific guid
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE /guid/3BED9CEFBB9F43E99DFE3EAE6CC5847F
        ///     
        /// </remarks>   
        /// <param name="guid">The guid, must be 32-bit hexadecimal in uppercase</param>
        /// <returns></returns>
        /// <response code="204">If the guid is successfully deleted</response>
        /// <response code="404">If the guid does not exist</response>  
        /// <response code="400">If the guid is invalid</response>  
        /// <response code="500">If any unexpected error</response>   
        [HttpDelete("{guid}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult> DeleteGuidData(string guid)
        {
            GuidValidator.EnsureValid(guid);

            var success = await _provider.DeleteGuidData(guid);
            if (success)
                return NoContent();
            else
                return NotFound();
        }
    }
}
