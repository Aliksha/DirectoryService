using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Departments
{
    public record DepartmentDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = null!;

        public string Identifier { get; init; } = null!;

        // потом адреса добавь !

        public bool IsActive { get; init; }

        public DateTime Created { get; init; }

        public DateTime Updated { get; init; }
    }
}
