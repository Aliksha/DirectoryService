using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DirectoryService.Domain.Locations
{
    public sealed class Location : Entity<LocationId>
    {
        // EF core
        private Location(LocationId id)
            : base(id) { }


        private List<DepartmentLocation> _departments = [];
        private Location(LocationId id, LocationName name, Address address, Timezone timezone/*, IEnumerable<DepartmentLocation> departments*/) : base(id)
        {
            Id = id;
            Name = name;
            Address = address;
            Timezone = timezone;
            IsActive = true;
            //_departments = departments.ToList();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        //public LocationId Id { get; private set; }
        public LocationName Name { get; private set; }
        public Address Address { get; private set; }
        public Timezone Timezone { get; private set; }
        public bool IsActive { get; private set; }
        public IReadOnlyList<DepartmentLocation> LocDepartments => _departments;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public static Result<Location> Create(LocationName name, Address address, Timezone timezone/*, IEnumerable<DepartmentLocation> departments*/)
        {
            var id = LocationId.Create();
            return new Location(id, name, address, timezone/*, departments*/);
        }

    }
}
