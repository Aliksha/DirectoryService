using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Departments
{
    public sealed record DepartmentName
    {
        public const int MIN_LENGTH = 3;
        public const int MAX_LENGTH = 150;

        public DepartmentName(string value)
        {
            Value = value;
        }
        private string Value { get; }

        public static Result<DepartmentName> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Failure<DepartmentName>("Department name cannot be enpty");
            if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
                return Result.Failure<DepartmentName>("Department name is too short or too long");

            return Result.Success(new DepartmentName(value));
        }
    }
}
