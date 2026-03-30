using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.DepartmentLocations
{
    public class DepartmentLocation
    {
        private DepartmentLocation() { }

        private DepartmentLocation(DepartmentLocationId id, DepartmentId departmentId, LocationId locationId)
        {
            Id = id;
            DepartmentId = departmentId;
            LocationId = locationId;
        }

        public DepartmentLocationId Id { get; private set; }

        public DepartmentId DepartmentId { get; private set; }

        public LocationId LocationId { get; private set; }

        public static DepartmentLocation Create(DepartmentLocationId id, DepartmentId departmentId, LocationId locationId)
        {
            return new DepartmentLocation(id, departmentId, locationId);
        }

    }
}
