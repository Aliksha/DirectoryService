using CSharpFunctionalExtensions;
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

        public static Result<Identifier> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Failure<Identifier>("Identifier cannot be empty");
            if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
                return Result.Failure<Identifier>("Identifier is too short or too long");

            var trimedValue = value.Trim().ToLowerInvariant();
            if (!ValidFormat.IsMatch(trimedValue))
                return Result.Failure<Identifier>("Identifier");

            return Result.Success(new Identifier(trimedValue));
        }
    }
}
