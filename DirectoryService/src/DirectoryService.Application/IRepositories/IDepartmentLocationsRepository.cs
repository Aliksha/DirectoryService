using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.IRepositories
{
    public interface IDepartmentLocationsRepository
    {
        Task<bool> IsConnectedAlready(DepartmentId departmentId, LocationId locationId, CancellationToken cancellationToken = default);

        Task<Result<Guid, Error>> AddConnectionAsync(DepartmentLocation departmentLocation, CancellationToken cancellationToken);

        Task<UnitResult<Errors>> DeleteConnectionAsync(DepartmentId departmentId, LocationId locationId, CancellationToken cancellationToken);
    }
}
