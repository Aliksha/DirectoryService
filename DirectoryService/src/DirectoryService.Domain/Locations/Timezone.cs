using CSharpFunctionalExtensions;
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

        public static Result<Timezone> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Failure<Timezone>("Timezone cannot be empty.");

            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(value);
            }
            catch (TimeZoneNotFoundException)
            {
                return Result.Failure<Timezone>($"'{value}' is not a valid system timezone ID.");
            }

            return Result.Success(new Timezone(value));
        }
    }
}
