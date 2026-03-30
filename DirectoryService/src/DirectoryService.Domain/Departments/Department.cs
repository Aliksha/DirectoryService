using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Departments
{
    public class Department
    {
        // ef core
        private Department()
        {

        }

        private readonly List<Department> _childDepartments = [];

        private readonly List<DepartmentLocation> _departmentsLocations = [];

        private readonly List<DepartmentPosition> _departmentsPositions = [];

        private Department(
            DepartmentId id,
            DepartmentName name,
            Identifier identifier,
            DepartmentId? parentId = null,
            Path? path = null,
            short depth = 0)
        {
            Id = id;
            Name = name;
            Identifier = identifier;
            ParentId = parentId;
            Path = path;
            Depth = depth;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public DepartmentId Id { get; private set; }
        public DepartmentName Name { get; private set; }
        public Identifier Identifier { get; private set; }
        public DepartmentId? ParentId { get; private set; }
        public Path Path { get; private set; } = null!;
        public short Depth { get; private set; }
        public bool IsActive { get; private set; }
        public IReadOnlyList<Department> ChildDepartments => _childDepartments;
        public IReadOnlyList<DepartmentLocation> Locations => _departmentsLocations;
        public IReadOnlyList<DepartmentPosition> Positions => _departmentsPositions;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // сохдать корневой дапартмент
        public static Result<Department> Create(DepartmentName name, Identifier identifier)
        {
            if (name is null)
                return Result.Failure<Department>("Name is required.");
            if (string.IsNullOrWhiteSpace(identifier.Value))
                return Result.Failure<Department>("Identifier is required.");

            var id = DepartmentId.Create();

            return Result.Success(new Department(id, name, identifier));
        }
    }
}
