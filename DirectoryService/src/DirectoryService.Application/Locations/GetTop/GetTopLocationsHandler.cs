using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Contracts.Locations;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.GetTop
{
    public class GetTopLocationsHandler : IQueryHandler<TopLocationsResponseDto, GetTopLocationsQuery>
    {
        private readonly IReadDbContext _context;

        public GetTopLocationsHandler(IReadDbContext context)
        {
            _context = context;
        }

        public async Task<Result<TopLocationsResponseDto, Errors>> Handle(GetTopLocationsQuery query, CancellationToken cancellationToken = default)
        {
            var queryCount = query.Dto.Count;

            var topLocationsQuery = _context.LocationsRead
                .AsNoTracking()
                .Where(l => l.LocDepartments.Any())
                .OrderByDescending(l => l.LocDepartments.Count)
                .ThenBy(l => l.Name.Value)
                .Take(queryCount)
                .Select(l => new TopLocationDto
                {
                    Id = l.Id.Value,
                    Name = l.Name.Value,
                    HouseNumber = l.Address != null ? l.Address.HouseNumber : string.Empty,
                    Street = l.Address != null ? l.Address.Street : string.Empty,
                    City = l.Address != null ? l.Address.City : string.Empty,
                    Country = l.Address != null ? l.Address.Country : string.Empty,
                    Timezone = l.Timezone.Value,
                    DepartmentCount = l.LocDepartments.Count,
                });

            var locationsDtoList = await topLocationsQuery.ToListAsync(cancellationToken);

            var response = new TopLocationsResponseDto(locationsDtoList);

            return Result.Success<TopLocationsResponseDto, Errors>(response);
        }
    }
}
