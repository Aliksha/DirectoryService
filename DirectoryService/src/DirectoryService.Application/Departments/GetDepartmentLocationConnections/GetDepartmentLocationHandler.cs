using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Contracts.DepartmentLocation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.GetDepartmentLocationConnections
{
    // to test soft delete

    public class GetDepartmentLocationHandler : IQueryHandler<DepartmentLocationResponseDto, GetDepartmentLocationQuery>
    {
        private readonly IReadDbContext _context;

        public GetDepartmentLocationHandler(IReadDbContext context)
        {
            _context = context;
        }

        public async Task<Result<DepartmentLocationResponseDto, Errors>> Handle(GetDepartmentLocationQuery query, CancellationToken cancellationToken = default)
        {
            // разворачиваем департаменты и их id локаций в плоский список
            var departmentLocations = await _context.DepartmentsRead.AsQueryable()
                .SelectMany(d => d.Locations.Select(ld => new
                {
                    DepartmentName = d.Name,
                    LocationId = ld.LocationId,
                }))
                .ToListAsync(cancellationToken);

            // достаем все живые локации (глобал фильтр soft delete сработает автоматически)
            var locations = await _context.LocationsRead.AsQueryable()
                .ToListAsync(cancellationToken);

            // cоединяем их в памяти и вызываем конструктор рекорда
            var connections = departmentLocations
                .Join(
                    locations,
                    deptLoc => deptLoc.LocationId,
                    loc => loc.Id,
                    (deptLoc, loc) => new DepartmentLocationConnectionDto(
                        deptLoc.DepartmentName.Value,
                        loc.Name.Value,
                        new AddressDto(
                            loc.Address.HouseNumber,
                            loc.Address.Street,
                            loc.Address.City,
                            loc.Address.Country)))
                .ToList();

            var response = new DepartmentLocationResponseDto(connections);

            return response;
        }
    }
}
