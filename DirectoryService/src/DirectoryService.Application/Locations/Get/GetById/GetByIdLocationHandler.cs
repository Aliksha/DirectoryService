using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.Get.GetById
{
    public class GetByIdLocationHandler : IQueryHandler<LocationByIdResponseDto, GetByIdLocationQuery>
    {
        public readonly IReadDbContext _context;

        public GetByIdLocationHandler(IReadDbContext context)
        {
            _context = context;
        }

        public async Task<Result<LocationByIdResponseDto, Errors>> Handle(GetByIdLocationQuery query, CancellationToken cancellationToken = default)
        {
            var locationId = LocationId.Current(query.Dto.LocationId);

            var location = await _context.LocationsRead
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == locationId, cancellationToken);

            if (location == null)
            {
                return GeneralErrors.NotFound(query.Dto.LocationId, "location.not.found").ToErrors();
            }

            var locationDto = new LocationDto
            {
                Id = location.Id.Value,
                Name = location.Name.Value,
                Country = location.Address.Country,
                City = location.Address.City,
                Street = location.Address.Street,
                HouseNumber = location.Address.HouseNumber,
                Timezone = location.Timezone.Value,
                IsActive = location.IsActive,
                Created = location.CreatedAt,
                Updated = location.UpdatedAt,
            };

            var responseDto = new LocationByIdResponseDto(locationDto);
            return Result.Success<LocationByIdResponseDto, Errors>(responseDto);
        }
    }
}
