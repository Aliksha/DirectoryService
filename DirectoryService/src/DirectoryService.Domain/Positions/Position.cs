using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Positions
{
    public class Position
    {
        private Position()
        {

        }

        private readonly List<DepartmentPosition> _departments;
        private Position(PositionId id, PositionName name, string? description, IEnumerable<DepartmentPosition> departments)
        {
            Id = id;
            Name = name;
            Description = description;
            IsActive = true;
            _departments = departments.ToList();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public PositionId Id { get; private set; }
        public PositionName Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;
        public IReadOnlyList<DepartmentPosition> Departments => _departments;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public static Result<Position> Create(PositionName name, string description, IEnumerable<DepartmentPosition> departments)
        {
            var id = PositionId.Create();
            return Result.Success(new Position(id, name, description, departments));
        }
    }
}
