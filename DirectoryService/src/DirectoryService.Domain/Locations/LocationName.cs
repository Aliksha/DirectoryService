using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DirectoryService.Domain.Locations
{
    public sealed record LocationName
    {
        public const int MIN_LENGTH = 3;
        public const int MAX_LENGTH = 50;

        private LocationName(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<LocationName, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GeneralErrors.ValueIsRequired("location.name.empty");

            string normalized = Regex.Replace(value.Trim(), @"\s+", " ");

            if (normalized.Length < MIN_LENGTH || normalized.Length > MAX_LENGTH)
                return GeneralErrors.ValueIsInvalid("location.name.wrong.lenght");

            return new LocationName(normalized);
        }
    }
}
