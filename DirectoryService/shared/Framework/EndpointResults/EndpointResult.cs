using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Framework.EndpointResults
{
    public sealed class EndpointResult<TValue> : IResult, IEndpointMetadataProvider
    {
        private readonly IResult _result;

        public EndpointResult(Result<TValue, Error> result)
        {
            _result = result.IsSuccess
                ? new SuccessResult<TValue>(result.Value)
                : new ErrorResult(result.Error);
        }

        public EndpointResult(Result<TValue, Errors> result)
        {
            _result = result.IsSuccess
                ? new SuccessResult<TValue>(result.Value)
                : new ErrorResult(result.Error);
        }

        public static implicit operator EndpointResult<TValue>(Result<TValue, Error> result) => new(result);
        public static implicit operator EndpointResult<TValue>(Result<TValue, Errors> result) => new(result);

        static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(method);
            ArgumentNullException.ThrowIfNull(builder);

            builder.Metadata.Add(new ProducesResponseTypeAttribute(typeof(Envelope<TValue>), StatusCodes.Status200OK));
            builder.Metadata.Add(new ProducesResponseTypeAttribute(typeof(Envelope<object>), StatusCodes.Status400BadRequest));
            builder.Metadata.Add(new ProducesResponseTypeAttribute(typeof(Envelope<object>), StatusCodes.Status401Unauthorized));
            builder.Metadata.Add(new ProducesResponseTypeAttribute(typeof(Envelope<object>), StatusCodes.Status404NotFound));
            builder.Metadata.Add(new ProducesResponseTypeAttribute(typeof(Envelope<object>), StatusCodes.Status500InternalServerError));
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
           return _result.ExecuteAsync(httpContext);
        }
    }
}
