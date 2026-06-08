using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DirectoryService.Domain.Departments
{
    public sealed record DepartmentName
    {
        public const int MIN_LENGTH = 3;
        public const int MAX_LENGTH = 150;

        private DepartmentName(string value)
        {
            Value = value;
        }
        public string Value { get; }

        public static Result<DepartmentName, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return GeneralErrors.ValueIsRequired("departmnet.name.empty");

            string normalized = Regex.Replace(value.Trim(), @"\s+", " ");

            if (normalized.Length < MIN_LENGTH || normalized.Length > MAX_LENGTH)
                return GeneralErrors.ValueIsInvalid("department.name.wrong.lenght");

            return new DepartmentName(normalized);
        }
    }
}
