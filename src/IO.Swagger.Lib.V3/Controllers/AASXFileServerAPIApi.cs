/********************************************************************************
* Copyright (c) {2019 - 2024} Contributors to the Eclipse Foundation
*
* See the NOTICE file(s) distributed with this work for additional
* information regarding copyright ownership.
*
* This program and the accompanying materials are made available under the
* terms of the Apache License Version 2.0 which is available at
* https://www.apache.org/licenses/LICENSE-2.0
*
* SPDX-License-Identifier: Apache-2.0
********************************************************************************/

/*
 * DotAAS Part 2 | HTTP/REST | AASX File Server Service Specification
 *
 * The File Server Service Specification as part of the [Specification of the Asset Administration Shell: Part 2](http://industrialdigitaltwin.org/en/content-hub).   Publisher: Industrial Digital Twin Association (IDTA) 2023
 *
 * OpenAPI spec version: V3.0.1_SSP-001
 * Contact: info@idtwin.org
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using AasSecurity.Exceptions;
using AasxServerStandardBib.Interfaces;
using AasxServerStandardBib.Logging;
using IO.Swagger.Attributes;
using IO.Swagger.Lib.V3.Interfaces;
using IO.Swagger.Lib.V3.Models;
using IO.Swagger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mime;

namespace IO.Swagger.Controllers;

using System.Threading.Tasks;

/// <summary>
/// 
/// </summary>
[Authorize(AuthenticationSchemes = "AasSecurityAuth")]
[ApiController]
public class AASXFileServerAPIApiController : ControllerBase
{
    private readonly IAppLogger<AASXFileServerAPIApiController> _logger;
    private readonly IBase64UrlDecoderService _decoderService;
    private readonly IAasxFileServerInterfaceService _fileService;
    private readonly IPaginationService _paginationService;
    private readonly IAuthorizationService _authorizationService;

    public AASXFileServerAPIApiController(IAppLogger<AASXFileServerAPIApiController> logger, IBase64UrlDecoderService decoderService,
                                          IAasxFileServerInterfaceService fileService, IPaginationService paginationService, IAuthorizationService authorizationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ;
        _decoderService = decoderService ?? throw new ArgumentNullException(nameof(decoderService));
        ;
        _fileService          = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _paginationService    = paginationService ?? throw new ArgumentNullException(nameof(paginationService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    /// <summary>
    /// Deletes a specific AASX package from the server
    /// </summary>
    /// <param name="packageId">The package Id (UTF8-BASE64-URL-encoded)</param>
    /// <response code="204">Deleted successfully</response>
    /// <response code="400">Bad Request, e.g. the request parameters of the format of the request body is wrong.</response>
    /// <response code="401">Unauthorized, e.g. the server refused the authorization attempt.</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal Server Error</response>
    /// <response code="0">Default error handling for unmentioned status codes</response>
    [HttpDelete]
    [Route("/packages/{packageId}")]
    [ValidateModelState]
    [SwaggerOperation("DeleteAASXByPackageId")]
    [SwaggerResponse(statusCode: 400, type: typeof(Result), description: "Bad Request, e.g. the request parameters of the format of the request body is wrong.")]
    [SwaggerResponse(statusCode: 401, type: typeof(Result), description: "Unauthorized, e.g. the server refused the authorization attempt.")]
    [SwaggerResponse(statusCode: 403, type: typeof(Result), description: "Forbidden")]
    [SwaggerResponse(statusCode: 404, type: typeof(Result), description: "Not Found")]
    [SwaggerResponse(statusCode: 500, type: typeof(Result), description: "Internal Server Error")]
    [SwaggerResponse(statusCode: 0, type: typeof(Result), description: "Default error handling for unmentioned status codes")]
    public virtual IActionResult DeleteAASXByPackageId([FromRoute] [Required] string packageId)
    {
        var decodedPackageId = _decoderService.Decode("packageId", packageId);

        _logger.LogInformation($"Received request to delete the AASX Package with package id {decodedPackageId}");
        var aas        = _fileService.GetAssetAdministrationShellByPackageId(decodedPackageId);
        var authResult = _authorizationService.AuthorizeAsync(User, aas, "SecurityPolicy").Result;
        if (!authResult.Succeeded)
        {
            var failedReasons               = authResult.Failure.FailureReasons;
            var authorizationFailureReasons = failedReasons.ToList();
            if (authorizationFailureReasons.Count != 0)
            {
                throw new NotAllowed(authorizationFailureReasons.First().Message); // TODO (jtikekar, 2023-09-04): write AuthResultMiddlewareHandler
            }

            if (HttpContext.Response.StatusCode == 307)
            {
                var url = HttpContext.Response.Headers["redirectInfo"].First();
                return new RedirectResult(url ?? string.Empty);
            }
        }

        _fileService.DeleteAASXByPackageId(decodedPackageId);

        return NoContent();
    }

    /// <summary>
    /// Returns a specific AASX package from the server
    /// </summary>
    /// <param name="packageId">The package Id (UTF8-BASE64-URL-encoded)</param>
    /// <response code="200">Requested AASX package</response>
    /// <response code="400">Bad Request, e.g. the request parameters of the format of the request body is wrong.</response>
    /// <response code="401">Unauthorized, e.g. the server refused the authorization attempt.</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal Server Error</response>
    /// <response code="0">Default error handling for unmentioned status codes</response>
    [HttpGet]
    [Route("/packages/{packageId}")]
    [ValidateModelState]
    [SwaggerOperation("GetAASXByPackageId")]
    [SwaggerResponse(statusCode: 200, type: typeof(byte[]), description: "Requested AASX package")]
    [SwaggerResponse(statusCode: 400, type: typeof(Result), description: "Bad Request, e.g. the request parameters of the format of the request body is wrong.")]
    [SwaggerResponse(statusCode: 401, type: typeof(Result), description: "Unauthorized, e.g. the server refused the authorization attempt.")]
    [SwaggerResponse(statusCode: 403, type: typeof(Result), description: "Forbidden")]
    [SwaggerResponse(statusCode: 404, type: typeof(Result), description: "Not Found")]
    [SwaggerResponse(statusCode: 500, type: typeof(Result), description: "Internal Server Error")]
    [SwaggerResponse(statusCode: 0, type: typeof(Result), description: "Default error handling for unmentioned status codes")]
    public virtual async Task<IActionResult> GetAASXByPackageId([FromRoute] [Required] string packageId)
    {
        var decodedPackageId = _decoderService.Decode("packageId", packageId);

        _logger.LogInformation($"Received request to get the AASX Package with package id {decodedPackageId}");

        if (decodedPackageId == null)
        {
            throw new NotAllowed($"Cannot proceed as {nameof(decodedPackageId)} is null");
        }

        var fileName = _fileService.GetAASXByPackageId(decodedPackageId, out var content, out var fileSize, out var aas);

        var authResult = _authorizationService.AuthorizeAsync(User, aas, "SecurityPolicy").Result;
        if (!authResult.Succeeded)
        {
            var failedReasons               = authResult.Failure.FailureReasons;
            var authorizationFailureReasons = failedReasons.ToList();
            if (authorizationFailureReasons.Count != 0)
            {
                throw new NotAllowed(authorizationFailureReasons.First().Message); // TODO (jtikekar, 2023-09-04): write AuthResultMiddlewareHandler
            }

            if (HttpContext.Response.StatusCode == 307)
            {
                var url = HttpContext.Response.Headers["redirectInfo"].First();
                return new RedirectResult(url ?? string.Empty);
            }
        }

        //content-disposition so that the aasx file can be downloaded from the web browser.
        ContentDisposition contentDisposition = new() {FileName = fileName};

        HttpContext.Response.Headers.Append("Content-Disposition", contentDisposition.ToString());
        HttpContext.Response.Headers.Append("X-FileName", fileName);


        HttpContext.Response.ContentLength = fileSize;
        await HttpContext.Response.Body.WriteAsync(content);
        return new EmptyResult();
    }

    /// <summary>
    /// Returns a list of available AASX packages at the server
    /// </summary>
    /// <param name="aasId">The Asset Administration Shell’s unique id (UTF8-BASE64-URL-encoded)</param>
    /// <param name="limit">The maximum number of elements in the response array</param>
    /// <param name="cursor">A server-generated identifier retrieved from pagingMetadata that specifies from which position the result listing should continue</param>
    /// <response code="200">Requested package list</response>
    /// <response code="400">Bad Request, e.g. the request parameters of the format of the request body is wrong.</response>
    /// <response code="401">Unauthorized, e.g. the server refused the authorization attempt.</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Internal Server Error</response>
    /// <response code="0">Default error handling for unmentioned status codes</response>
    [HttpGet]
    [Route("/packages")]
    [ValidateModelState]
    [SwaggerOperation("GetAllAASXPackageIds")]
    [SwaggerResponse(statusCode: 200, type: typeof(PackageDescriptionPagedResult), description: "Requested package list")]
    [SwaggerResponse(statusCode: 400, type: typeof(Result), description: "Bad Request, e.g. the request parameters of the format of the request body is wrong.")]
    [SwaggerResponse(statusCode: 401, type: typeof(Result), description: "Unauthorized, e.g. the server refused the authorization attempt.")]
    [SwaggerResponse(statusCode: 403, type: typeof(Result), description: "Forbidden")]
    [SwaggerResponse(statusCode: 500, type: typeof(Result), description: "Internal Server Error")]
    [SwaggerResponse(statusCode: 0, type: typeof(Result), description: "Default error handling for unmentioned status codes")]
    public virtual IActionResult GetAllAASXPackageIds([FromQuery] string? aasId, [FromQuery] int? limit, [FromQuery] string? cursor)
    {
        _logger.LogInformation($"Received request to get all the AASX packages.");
        var decodedAasId = _decoderService.Decode("aasId", aasId);

        if (decodedAasId == null)
        {
            throw new NotAllowed($"Cannot proceed as {nameof(decodedAasId)} is null");
        }

        var packages = _fileService.GetAllAASXPackageIds(decodedAasId);

        var authResult = _authorizationService.AuthorizeAsync(User, packages, "SecurityPolicy").Result;
        if (!authResult.Succeeded)
        {
            var failedReasons               = authResult.Failure.FailureReasons;
            var authorizationFailureReasons = failedReasons.ToList();
            if (authorizationFailureReasons.Count != 0)
            {
                throw new NotAllowed(authorizationFailureReasons.First().Message); // TODO (jtikekar, 2023-09-04): write AuthResultMiddlewareHandler
            }
        }

        var paginatedPackages = _paginationService.GetPaginatedPackageDescriptionList(packages, new PaginationParameters(cursor, limit));
        return new ObjectResult(paginatedPackages);
    }

    /// <summary>
    /// Creates an AASX package at the server
    /// </summary>
    /// <param name="aasIds">Included AAS Ids</param>
    /// <param name="file">AASX Package</param>
    /// <returns></returns>
    [HttpPost]
    [Route("/packages")]
    [ValidateModelState]
    [SwaggerOperation("PostAASXPackage")]
    public virtual IActionResult PostAASXPackage([FromQuery] string? aasIds, IFormFile? file)
    {
        _logger.LogInformation($"Received request to create a new AASX Package.");

        if (file == null)
        {
            return Ok("No file was provided.");
        }

        var authResult = _authorizationService.AuthorizeAsync(User, "", "SecurityPolicy").Result;
        if (!authResult.Succeeded)
        {
            var failedReasons               = authResult.Failure.FailureReasons;
            var authorizationFailureReasons = failedReasons.ToList();
            if (authorizationFailureReasons.Count != 0)
            {
                throw new NotAllowed(authorizationFailureReasons.First().Message); // TODO (jtikekar, 2023-09-04): write AuthResultMiddlewareHandler
            }

            if (HttpContext.Response.StatusCode == 307)
            {
                var url = HttpContext.Response.Headers["redirectInfo"].First();
                return new RedirectResult(url ?? string.Empty);
            }
        }

        // TODO (jtikekar, 2023-09-04): aasIds
        var stream = new MemoryStream();
        file.CopyTo(stream);
        var packageId = _fileService.PostAASXPackage(stream.ToArray(), file.FileName);
        return CreatedAtAction(nameof(PostAASXPackage), packageId);
    }

    /// <summary>
    /// Updates the AASX package at the server
    /// </summary>
    /// <param name="packageId">Package ID from the package list (BASE64-URL-encoded)</param>
    /// <param name="file">AASX Package</param>
    /// <param name="aasIds">Included AAS Identifiers</param>
    /// <returns></returns>
    [HttpPut]
    [Route("/packages/{packageId}")]
    [ValidateModelState]
    [SwaggerOperation("PutAASXPackageById")]
    public virtual IActionResult PutAASXPackageById([FromRoute] [Required] string packageId, IFormFile? file, [FromQuery] string? aasIds)
    {
        if (file == null)
        {
            return Ok("No file was provided.");
        }

        // TODO (jtikekar, 2023-09-04): aasIds
        var decodedPackageId = _decoderService.Decode("packageId", packageId);

        if (decodedPackageId == null)
        {
            throw new NotAllowed($"Cannot proceed as {nameof(decodedPackageId)} is null");
        }

        _logger.LogInformation($"Received request to update the AASX Package with package id {decodedPackageId}.");

        var aas        = _fileService.GetAssetAdministrationShellByPackageId(decodedPackageId);
        var authResult = _authorizationService.AuthorizeAsync(User, aas, "SecurityPolicy").Result;
        if (!authResult.Succeeded)
        {
            var failedReasons               = authResult.Failure.FailureReasons;
            var authorizationFailureReasons = failedReasons.ToList();
            if (authorizationFailureReasons.Count != 0)
            {
                throw new NotAllowed(authorizationFailureReasons.First().Message); // TODO (jtikekar, 2023-09-04): write AuthResultMiddlewareHandler
            }

            if (HttpContext.Response.StatusCode == 307)
            {
                var url = HttpContext.Response.Headers["redirectInfo"].First();
                return new RedirectResult(url ?? string.Empty);
            }
        }

        var stream = new MemoryStream();
        file.CopyTo(stream);
        var fileName = file.FileName;
        _fileService.UpdateAASXPackageById(decodedPackageId, stream.ToArray(), fileName);

        return NoContent();
    }
}