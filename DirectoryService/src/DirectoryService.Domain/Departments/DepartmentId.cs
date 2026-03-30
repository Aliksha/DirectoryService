using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Departments
{
    public sealed record DepartmentId
    {
        public DepartmentId(Guid value)
        {
            Value = value;
        }

        private Guid Value { get; }

        public static DepartmentId Create() => new(Guid.NewGuid());
        public static DepartmentId Current(Guid id) => new DepartmentId(id);
    }
}
