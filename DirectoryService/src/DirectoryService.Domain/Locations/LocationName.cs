using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Locations
{
    public sealed record LocationName
    {
        public const int MIN_LENGTH = 3;
        public const int MAX_LENGTH = 120;

        private LocationName(string value)
        {
            Value = value;
        }
        public string Value { get; }

        public static Result<LocationName> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<LocationName>("Name cannot be empty.");
            if (name.Length < MIN_LENGTH || name.Length > MAX_LENGTH)
                return Result.Failure<LocationName>("Name is tooo short ot too long");

            return Result.Success(new LocationName(name));
        }
    }
}
