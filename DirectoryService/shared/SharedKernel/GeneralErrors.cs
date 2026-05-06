using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel
{
    public static class GeneralErrors
    {
        public static Error ValueIsInvalid(string? name = null)
        {
            string label = name ?? "title";
            return Error.Validation("value.is.invalid", $"{label} is not valid", name);
        }

        public static Error NotFound(Guid? id = null, string? name = null)
        {
            string forId = id == null ? string.Empty : $"by id {id}";
            return Error.NotFound("value.not.found", $"{name ?? "the record"} was not found {forId}");
        }

        public static Error ValueIsRequired(string? name = null)
        {
            string label = name ?? string.Empty;
            return Error.Validation("length.is.invalid", $"field {label} is required", name);
        }

        public static Error AlreadyExist(string? name = null)
        {
            string label = name ?? string.Empty;
            return Error.Conflict("record.already.exist", "the record already exists");
        }

        public static Error Failure()
        {
            return Error.Failure("server.failure", "server error");
        }

        public static Error DataBase(string? code = null)
        {
            return Error.Conflict(code, "database error");
        }
    }
}
