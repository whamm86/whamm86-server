/*
 * DotAAS Part 2 | HTTP/REST | Repository Service Specification
 *
 * The entire Repository Service Specification as part of Details of the Asset Administration Shell Part 2
 *
 * OpenAPI spec version: V3.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using IO.Swagger.Attributes;

using Microsoft.AspNetCore.Authorization;
using IO.Swagger.Models;

namespace IO.Swagger.Controllers
{ 
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class DescriptionAPIApiController : ControllerBase
    { 
        /// <summary>
        /// Returns the self-describing information of a network resource (Description)
        /// </summary>
        /// <response code="200">Requested Description</response>
        /// <response code="403">Forbidden</response>
        [HttpGet]
        [Route("/description")]
        [ValidateModelState]
        [SwaggerOperation("GetDescription")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Description>), description: "Requested Description")]
        [SwaggerResponse(statusCode: 403, type: typeof(Result), description: "Forbidden")]
        public virtual IActionResult GetDescription()
        { 
            //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
            // return StatusCode(200, default(List<Description>));

            //TODO: Uncomment the next line to return response 403 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
            // return StatusCode(403, default(Result));
            string exampleJson = null;
            exampleJson = "[ \"{\n  \"profiles\": [\n    \"RepositoryServiceSpecification/V3.0-MinimalProfile\",\n    \"RegistryServiceSpecification/V3.0\"\n  ]\n}\", \"{\n  \"profiles\": [\n    \"RepositoryServiceSpecification/V3.0-MinimalProfile\",\n    \"RegistryServiceSpecification/V3.0\"\n  ]\n}\" ]";
            
                        var example = exampleJson != null
                        ? JsonConvert.DeserializeObject<List<Description>>(exampleJson)
                        : default(List<Description>);            //TODO: Change the data returned
            return new ObjectResult(example);
        }
    }
}
