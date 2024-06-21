/*
 * DotAAS Part 2 | HTTP/REST | Asset Administration Shell Repository Service Specification
 *
 * The Full Profile of the Asset Administration Shell Repository Service Specification as part of Specification of the Asset Administration Shell: Part 2. Publisher: Industrial Digital Twin Association (IDTA) April 2023
 *
 * OpenAPI spec version: V3.0_SSP-001
 * Contact: info@idtwin.org
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using AasxServerStandardBib.Logging;
using IO.Swagger.Attributes;
using IO.Swagger.Lib.V3.Interfaces;
using IO.Swagger.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Xml;

namespace IO.Swagger.Controllers;

using System.ComponentModel.DataAnnotations;
using System.Linq;

/// <summary>
/// 
/// </summary>
[ApiController]
public class SerializationAPIApiController : ControllerBase
{
    private readonly IAppLogger<SerializationAPIApiController> _logger;
    private readonly IBase64UrlDecoderService _decoderService;
    private readonly IGenerateSerializationService _serializationService;

    public SerializationAPIApiController(IAppLogger<SerializationAPIApiController> logger, IBase64UrlDecoderService decoderService,
                                         IGenerateSerializationService serializationService)
    {
        _logger               = logger ?? throw new ArgumentNullException(nameof(logger));
        _decoderService       = decoderService ?? throw new ArgumentNullException(nameof(decoderService));
        _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
    }

    /// <summary>
    /// Returns an appropriate serialization based on the specified format (see SerializationFormat)
    /// </summary>
    /// <param name="aasIds">The Asset Administration Shells&#x27; unique ids (UTF8-BASE64-URL-encoded)</param>
    /// <param name="submodelIds">The Submodels&#x27; unique ids (UTF8-BASE64-URL-encoded)</param>
    /// <param name="includeConceptDescriptions">Include Concept Descriptions?</param>
    /// <response code="200">Requested serialization based on SerializationFormat</response>
    /// <response code="400">Bad Request, e.g. the request parameters of the format of the request body is wrong.</response>
    /// <response code="401">Unauthorized, e.g. the server refused the authorization attempt.</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Internal Server Error</response>
    /// <response code="0">Default error handling for unmentioned status codes</response>
    [HttpGet]
    [Route("/serialization")]
    [ValidateModelState]
    [SwaggerOperation("GenerateSerializationByIds")]
    [SwaggerResponse(statusCode: 200, type: typeof(byte[]), description: "Requested serialization based on SerializationFormat")]
    [SwaggerResponse(statusCode: 400, type: typeof(Result), description: "Bad Request, e.g. the request parameters of the format of the request body is wrong.")]
    [SwaggerResponse(statusCode: 401, type: typeof(Result), description: "Unauthorized, e.g. the server refused the authorization attempt.")]
    [SwaggerResponse(statusCode: 403, type: typeof(Result), description: "Forbidden")]
    [SwaggerResponse(statusCode: 500, type: typeof(Result), description: "Internal Server Error")]
    [SwaggerResponse(statusCode: 0, type: typeof(Result), description: "Default error handling for unmentioned status codes")]
    public virtual IActionResult GenerateSerializationByIds([FromQuery] List<string?>? aasIds, [FromQuery] List<string?>? submodelIds,
                                                            [FromQuery] bool includeConceptDescriptions = false)
    {
        var decodedAasIds = aasIds.Select(aasId => _decoderService.Decode("aasIdentifier", aasId)).ToList();

        var decodedSubmodelIds = submodelIds.Select(submodelId => _decoderService.Decode("submodelIdentifier", submodelId)).ToList();

        var environment = _serializationService.GenerateSerializationByIds(decodedAasIds, decodedSubmodelIds, (bool)includeConceptDescriptions);

        HttpContext.Request.Headers.TryGetValue("Content-Type", out var contentType);
        if (!contentType.Equals("application/xml"))
        {
            return new ObjectResult(environment);
        }

        var outputBuilder = new System.Text.StringBuilder();
        var writer        = XmlWriter.Create(outputBuilder, new XmlWriterSettings() {Indent = true, OmitXmlDeclaration = true});
        Xmlization.Serialize.To(environment, writer);
        writer.Flush();
        writer.Close();
        return new ObjectResult(outputBuilder.ToString());
    }
}