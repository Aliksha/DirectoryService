using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DirectoryService.Domain.Departments
{
    public sealed record Identifier
    {
        public const int MIN_LENGTH = 3;
        public const int MAX_LENGTH = 150;

        private static readonly Regex ValidFormat = new(@"^[a-z0-9-]+$", RegexOptions.Compiled);

        private Identifier(string value)
        {
            Value = value;
        }
        public string Value { get; }

        public static Result<Identifier, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GeneralErrors.ValueIsRequired("identifier.empty");

            if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
                return GeneralErrors.ValueIsInvalid("identifier.wrong.lenght");

            var trimedValue = value.Trim().ToLowerInvariant();

            if (!ValidFormat.IsMatch(trimedValue))
                return GeneralErrors.ValueIsInvalid("identifier.is.invalid");

            return new Identifier(trimedValue);
        }
    }
}
