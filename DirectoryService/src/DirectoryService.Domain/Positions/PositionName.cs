using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Positions
{
    public sealed record PositionName
    {
        public const int MIN_LENGTH = 3;
        public const int MAX_LENGTH = 100;

        private PositionName(string value)
        {
            Value = value;
        }
        public string Value { get; }

        public static Result<PositionName> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<PositionName>("Name cannot be empty.");
            }

            if (name.Length < MIN_LENGTH || name.Length > MAX_LENGTH)
            {
                return Result.Failure<PositionName>($"Name must be between {MIN_LENGTH} and {MAX_LENGTH} characters.");
            }

            return Result.Success(new PositionName(name));
        }

    }
}
