using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Framework.EndpointResults
{
    public sealed class SuccessResult<TValue> : IResult
    {
        private readonly TValue _value;

        public SuccessResult(TValue value)
        {
            _value = value;
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            var envelope = Envelope.Ok(_value);

            httpContext.Response.StatusCode = StatusCodes.Status200OK;

            return httpContext.Response.WriteAsJsonAsync(envelope);
        }

        //public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
        //{
        //    // свагеру что возвращается Envelope с типом TValue
        //    builder.Metadata.Add(new ProducesResponseTypeAttribute(typeof(Envelope<TValue>), StatusCodes.Status200OK));
        //}
    }
}