using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
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
            SoftDeleted = false;
            DeletedAt = null;
        }

        public PositionId Id { get; private set; }
        public PositionName Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;
        public IReadOnlyList<DepartmentPosition> Departments => _departments;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public bool SoftDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        public static Result<Position> Create(PositionName name, string? description, IEnumerable<DepartmentPosition> departments, PositionId? id = null)
        {
            return new Position(id ?? PositionId.Create(), name, description, departments);
        }

        public void Rename(PositionName newName)
        {
            if (newName is null)
                throw new ArgumentNullException(nameof(newName));

            Name = newName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDescription(string newDescription)
        {
            if (newDescription is null)
                throw new ArgumentNullException(nameof(newDescription));
            Description = newDescription;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDepartments(IEnumerable<DepartmentPosition> connectionsWithDepartments)
        {
            _departments.Clear();
            _departments.AddRange(connectionsWithDepartments);
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            SoftDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}
