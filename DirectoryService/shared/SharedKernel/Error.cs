using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharedKernel
{
    public record Error
    {
        public string Code { get; }

        public string Message { get; }

        public ErrorType Type { get; }

        public string? InvalidField { get; }

        private Error(string code, string message, ErrorType type, string? invalidField = null)
        {
            Code = code;
            Message = message;
            Type = type;
            InvalidField = invalidField;
        }

        public static Error NotFound(string? code, string message) =>
            new(code ?? "value.not.found", message, ErrorType.NOT_FOUND);

        public static Error Validation(string? code, string message, string? invalidField = null) =>
            new(code ?? "value.is.invalid", message, ErrorType.VALIDATION, invalidField);

        public static Error Failure(string? code, string message) =>
            new(code ?? "failure", message, ErrorType.FAILURE);

        public static Error Conflict(string? code, string message) =>
            new(code ?? "value.is.conflict", message, ErrorType.CONFLICT);

        public Errors ToErrors() => new([this]);
    }

    [JsonConverter(typeof(JsonStringEnumConverter))] // Принудительно превращает в строку при сериализации
    public enum ErrorType
    {
        VALIDATION,
        NOT_FOUND,
        FAILURE,
        CONFLICT,
    }
}
