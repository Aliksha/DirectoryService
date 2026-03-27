using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.DepartmentPositions
{
    public class DepartmentPosition
    {
        private DepartmentPosition() { }

        private DepartmentPosition(DepartmentPositionId id, DepartmentId departmentId, PositionId positionId)
        {
            Id = id;
            DepartmentId = departmentId;
            PositionId = positionId;
        }

        public DepartmentPositionId Id { get; private set; }

        public DepartmentId DepartmentId { get; private set; }

        public PositionId PositionId { get; private set; }

        public static DepartmentPosition Create(DepartmentPositionId id, DepartmentId departmentId, PositionId positionId)
        {
            return new DepartmentPosition(id, departmentId, positionId);
        }

    }
}
