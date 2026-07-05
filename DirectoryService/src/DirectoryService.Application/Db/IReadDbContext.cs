using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Db
{
    public interface IReadDbContext
    {
        IQueryable<Location> LocationsRead { get; }

        IQueryable<Department> DepartmentsRead { get; }
    }
}
