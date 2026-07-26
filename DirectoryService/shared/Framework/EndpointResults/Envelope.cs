using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Framework.EndpointResults
{
    public record Envelope<T>
    {
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Result { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Errors? ErrorsList { get; init; }

        public bool IsError => ErrorsList?.Any() == true;

        public DateTime Timestamp { get; init; }

        public Envelope(T? result, Errors? errorsList)
        {
            Result = result;
            ErrorsList = errorsList;
            Timestamp = DateTime.UtcNow;
        }

        public static Envelope<T> Ok(T? result = default) => new(result, null);
        public static Envelope<T> Error(Errors errorsList) => new(default, errorsList);
    }

    public static class Envelope
    {
        public static Envelope<object> Ok(object? result = null) => Envelope<object>.Ok(result);
        public static Envelope<object> Error(Errors errorsList) => Envelope<object>.Error(errorsList);
    }
}
