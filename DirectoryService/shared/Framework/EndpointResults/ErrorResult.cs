using Microsoft.AspNetCore.Http;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.EndpointResults
{
    public sealed class ErrorResult : IResult
    {
        private readonly Errors _errors;

        //public ErrorResult(Errors error)
        //{
        //    _errors = error.ToError();
        //}
        public ErrorResult(Errors errors)
        {
            _errors = errors;
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            if (!_errors.Any())
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return httpContext.Response.WriteAsJsonAsync(Envelope.Error(_errors));
            }

            var distinctErrorTypes = _errors
                .Select(x => x.Type)
                .Distinct()
                .ToList();

            int statusCode = distinctErrorTypes.Count > 1
                ? StatusCodes.Status500InternalServerError
                : GetStatusCodeForErrorType(distinctErrorTypes.First());

            var envelope = Envelope.Error(_errors);

            httpContext.Response.StatusCode = statusCode;

            return httpContext.Response.WriteAsJsonAsync(envelope);
        }

        private static int GetStatusCodeForErrorType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.CONFLICT => StatusCodes.Status409Conflict,
                ErrorType.NOT_FOUND => StatusCodes.Status404NotFound,
                ErrorType.VALIDATION => StatusCodes.Status400BadRequest,
                ErrorType.FAILURE => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };
    }
}