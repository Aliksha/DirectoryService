using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Locations
{
    public sealed record Timezone
    {
        private Timezone(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Result<Timezone, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GeneralErrors.ValueIsRequired("timezone.empty");

            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(value);
            }
            catch (TimeZoneNotFoundException)
            {
                return GeneralErrors.ValueIsInvalid("timezone.not.found");
            }

            return new Timezone(value);
        }
    }
}
