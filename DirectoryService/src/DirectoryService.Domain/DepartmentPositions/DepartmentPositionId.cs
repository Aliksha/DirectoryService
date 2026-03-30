using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.DepartmentPositions
{
    public sealed record DepartmentPositionId
    {
        private DepartmentPositionId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static DepartmentPositionId Create() => new(Guid.NewGuid());
        public static DepartmentPositionId Current(Guid id) => new(id);
    }
}
