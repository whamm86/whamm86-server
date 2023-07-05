/*
 * DotAAS Part 2 | HTTP/REST | Concept Description Repository Service Specification
 *
 * The ConceptDescription Repository Service Specification as part of Details of the Asset Administration Shell Part 2. Publisher: Industrial Digital Twin Association (IDTA) March 2023
 *
 * OpenAPI spec version: V3.0_SSP-001
 * Contact: info@idtwin.org
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using AasxServerStandardBib.Interfaces;
using AasxServerStandardBib.Logging;
using IO.Swagger.Attributes;
using IO.Swagger.Lib.V3.Interfaces;
using IO.Swagger.Lib.V3.Services;
using IO.Swagger.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IO.Swagger.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class ConceptDescriptionRepositoryAPIApiController : ControllerBase
    {
        private readonly IAppLogger<ConceptDescriptionRepositoryAPIApiController> _logger;
        private readonly IBase64UrlDecoderService _decoderService;
        private readonly IConceptDescriptionService _cdService;
        private readonly IJsonQueryDeserializer _jsonQueryDeserializer;
        private readonly IPaginationService _paginationService;

        public ConceptDescriptionRepositoryAPIApiController(IAppLogger<ConceptDescriptionRepositoryAPIApiController> logger, IBase64UrlDecoderService decoderService, IConceptDescriptionService cdService, IJsonQueryDeserializer jsonQueryDeserializer, IPaginationService paginationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _decoderService = decoderService ?? throw new ArgumentNullException(nameof(decoderService));
            _cdService = cdService ?? throw new ArgumentNullException(nameof(cdService));
            _jsonQueryDeserializer = jsonQueryDeserializer ?? throw new ArgumentNullException(nameof(jsonQueryDeserializer));
            _paginationService = paginationService ?? throw new ArgumentNullException(nameof(paginationService));
        }
        /// <summary>
        /// Deletes a Concept Description
        /// </summary>
        /// <param name="cdIdentifier">The Concept Description’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <response code="204">Concept Description deleted successfully</response>
        /// <response code="400">Bad Request, e.g. the request parameters of the format of the request body is wrong.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="0">Default error handling for unmentioned status codes</response>
        [HttpDelete]
        [Route("/concept-descriptions/{cdIdentifier}")]
        [ValidateModelState]
        [SwaggerOperation("DeleteConceptDescriptionById")]
        [SwaggerResponse(statusCode: 400, type: typeof(Result), description: "Bad Request, e.g. the request parameters of the format of the request body is wrong.")]
        [SwaggerResponse(statusCode: 403, type: typeof(Result), description: "Forbidden")]
        [SwaggerResponse(statusCode: 404, type: typeof(Result), description: "Not Found")]
        [SwaggerResponse(statusCode: 500, type: typeof(Result), description: "Internal Server Error")]
        [SwaggerResponse(statusCode: 0, type: typeof(Result), description: "Default error handling for unmentioned status codes")]
        public virtual IActionResult DeleteConceptDescriptionById([FromRoute][Required] string cdIdentifier)
        {
            var decodedCdIdentifier = _decoderService.Decode("cdIdentifier", cdIdentifier);

            _logger.LogInformation($"Received request to delete concept description with id {decodedCdIdentifier}");

            _cdService.DeleteConceptDescriptionById(decodedCdIdentifier);

            return NoContent();
        }

        /// <summary>
        /// Returns all Concept Descriptions
        /// </summary>
        /// <param name="idShort">The Concept Description’s IdShort</param>
        /// <param name="isCaseOf">IsCaseOf reference (UTF8-BASE64-URL-encoded)</param>
        /// <param name="dataSpecificationRef">DataSpecification reference (UTF8-BASE64-URL-encoded)</param>
        /// <param name="limit">The maximum number of elements in the response array</param>
        /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
        /// <response code="200">Requested Concept Descriptions</response>
        /// <response code="400">Bad Request, e.g. the request parameters of the format of the request body is wrong.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="0">Default error handling for unmentioned status codes</response>
        [HttpGet]
        [Route("/concept-descriptions")]
        [ValidateModelState]
        [SwaggerOperation("GetAllConceptDescriptions")]
        //[SwaggerResponse(statusCode: 200, type: typeof(GetConceptDescriptionsResult), description: "Requested Concept Descriptions")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<ConceptDescription>), description: "Requested Concept Descriptions")]
        [SwaggerResponse(statusCode: 400, type: typeof(Result), description: "Bad Request, e.g. the request parameters of the format of the request body is wrong.")]
        [SwaggerResponse(statusCode: 403, type: typeof(Result), description: "Forbidden")]
        [SwaggerResponse(statusCode: 500, type: typeof(Result), description: "Internal Server Error")]
        [SwaggerResponse(statusCode: 0, type: typeof(Result), description: "Default error handling for unmentioned status codes")]
        public virtual IActionResult GetAllConceptDescriptions([FromQuery] string idShort, [FromQuery] string isCaseOf, [FromQuery] string dataSpecificationRef, [FromQuery] int? limit, [FromQuery] string cursor)
        {
            _logger.LogInformation($"Received request to get all the concept descriptions.");

            var reqIsCaseOf = _jsonQueryDeserializer.DeserializeReference("isCaseOf", isCaseOf);
            var reqDataSpecificationRef = _jsonQueryDeserializer.DeserializeReference("dataSpecificationRef", dataSpecificationRef);

            var cdList = _cdService.GetAllConceptDescriptions(idShort, reqIsCaseOf, reqDataSpecificationRef);

            var output = _paginationService.GetPaginatedList(cdList, new PaginationParameters(cursor, limit));
            return new ObjectResult(output);
        }

        /// <summary>
        /// Returns a specific Concept Description
        /// </summary>
        /// <param name="cdIdentifier">The Concept Description’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <response code="200">Requested Concept Description</response>
        /// <response code="400">Bad Request, e.g. the request parameters of the format of the request body is wrong.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="0">Default error handling for unmentioned status codes</response>
        [HttpGet]
        [Route("/concept-descriptions/{cdIdentifier}")]
        [ValidateModelState]
        [SwaggerOperation("GetConceptDescriptionById")]
        [SwaggerResponse(statusCode: 200, type: typeof(ConceptDescription), description: "Requested Concept Description")]
        [SwaggerResponse(statusCode: 400, type: typeof(Result), description: "Bad Request, e.g. the request parameters of the format of the request body is wrong.")]
        [SwaggerResponse(statusCode: 403, type: typeof(Result), description: "Forbidden")]
        [SwaggerResponse(statusCode: 404, type: typeof(Result), description: "Not Found")]
        [SwaggerResponse(statusCode: 500, type: typeof(Result), description: "Internal Server Error")]
        [SwaggerResponse(statusCode: 0, type: typeof(Result), description: "Default error handling for unmentioned status codes")]
        public virtual IActionResult GetConceptDescriptionById([FromRoute][Required] string cdIdentifier)
        {
            var decodedCdIdentifier = _decoderService.Decode("cdIdentifier", cdIdentifier);

            _logger.LogInformation($"Received request to get concept description with id {decodedCdIdentifier}");

            var output = _cdService.GetConceptDescriptionById(decodedCdIdentifier);
            return new ObjectResult(output);
        }

        /// <summary>
        /// Creates a new Concept Description
        /// </summary>
        /// <param name="body">Concept Description object</param>
        /// <response code="201">Concept Description created successfully</response>
        /// <response code="400">Bad Request, e.g. the request parameters of the format of the request body is wrong.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="409">Conflict, a resource which shall be created exists already. Might be thrown if a Submodel or SubmodelElement with the same ShortId is contained in a POST request.</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="0">Default error handling for unmentioned status codes</response>
        [HttpPost]
        [Route("/concept-descriptions")]
        [ValidateModelState]
        [SwaggerOperation("PostConceptDescription")]
        [SwaggerResponse(statusCode: 201, type: typeof(ConceptDescription), description: "Concept Description created successfully")]
        [SwaggerResponse(statusCode: 400, type: typeof(Result), description: "Bad Request, e.g. the request parameters of the format of the request body is wrong.")]
        [SwaggerResponse(statusCode: 403, type: typeof(Result), description: "Forbidden")]
        [SwaggerResponse(statusCode: 409, type: typeof(Result), description: "Conflict, a resource which shall be created exists already. Might be thrown if a Submodel or SubmodelElement with the same ShortId is contained in a POST request.")]
        [SwaggerResponse(statusCode: 500, type: typeof(Result), description: "Internal Server Error")]
        [SwaggerResponse(statusCode: 0, type: typeof(Result), description: "Default error handling for unmentioned status codes")]
        public virtual IActionResult PostConceptDescription([FromBody] ConceptDescription body)
        {
            var output = _cdService.CreateConceptDescription(body);

            return CreatedAtAction(nameof(PostConceptDescription), output);
        }

        /// <summary>
        /// Updates an existing Concept Description
        /// </summary>
        /// <param name="body">Concept Description object</param>
        /// <param name="cdIdentifier">The Concept Description’s unique id (UTF8-BASE64-URL-encoded)</param>
        /// <response code="204">Concept Description updated successfully</response>
        /// <response code="400">Bad Request, e.g. the request parameters of the format of the request body is wrong.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <response code="0">Default error handling for unmentioned status codes</response>
        [HttpPut]
        [Route("/concept-descriptions/{cdIdentifier}")]
        [ValidateModelState]
        [SwaggerOperation("PutConceptDescriptionById")]
        [SwaggerResponse(statusCode: 400, type: typeof(Result), description: "Bad Request, e.g. the request parameters of the format of the request body is wrong.")]
        [SwaggerResponse(statusCode: 403, type: typeof(Result), description: "Forbidden")]
        [SwaggerResponse(statusCode: 404, type: typeof(Result), description: "Not Found")]
        [SwaggerResponse(statusCode: 500, type: typeof(Result), description: "Internal Server Error")]
        [SwaggerResponse(statusCode: 0, type: typeof(Result), description: "Default error handling for unmentioned status codes")]
        public virtual IActionResult PutConceptDescriptionById([FromBody] ConceptDescription body, [FromRoute][Required] string cdIdentifier)
        {
            var decodedCdId = _decoderService.Decode("cdIdentifier", cdIdentifier);

            _cdService.UpdateConceptDescriptionById(body, decodedCdId);

            return NoContent();
        }
    }
}
