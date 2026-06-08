using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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

        public static Result<PositionName, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GeneralErrors.ValueIsRequired("position.name.empty");

            string normalized = Regex.Replace(value.Trim(), @"\s+", " ");

            if (normalized.Length < MIN_LENGTH || normalized.Length > MAX_LENGTH)
                return GeneralErrors.ValueIsInvalid("position.name.wrong.lenght");

            return new PositionName(normalized);
        }

    }
}
