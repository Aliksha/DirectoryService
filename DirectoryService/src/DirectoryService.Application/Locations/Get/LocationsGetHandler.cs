using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Application.Locations.Create.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace DirectoryService.Application.Locations.Get
{
    public class LocationsGetHandler : IQueryHandler<LocationResponseDto, LocationsGetQuery>
    {
        private readonly IReadDbContext _context;
        private readonly IValidator<LocationsGetQuery> _validator;

        public LocationsGetHandler(IReadDbContext context, IValidator<LocationsGetQuery> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result<LocationResponseDto, Errors>> Handle(LocationsGetQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                var domainErrors = validationResult.ToErrorList();
                return domainErrors;
            }

            var locationQuery = _context.LocationsRead.AsQueryable();

            // Фильтрация по поисковой строке
            if (!string.IsNullOrWhiteSpace(query.Dto.Search))
            {
                var search = query.Dto.Search.Trim();

                var wildcardPattern = $"%{query.Dto.Search.Trim()}%";
                locationQuery = locationQuery.Where(l =>
                    NpgsqlDbFunctionsExtensions.ILike(EF.Functions, l.Name.Value, wildcardPattern));

               // locationQuery = locationQuery.Where(l => l.Name.Value.Contains(search));

            }

            // Фильтрация по статусу
            if (query.Dto.IsActive.HasValue)
            {
                locationQuery = locationQuery.Where(l => l.IsActive == query.Dto.IsActive.Value);
            }

            // Фильтрация по департаментам
            if(query.Dto.DepartmentIds != null && query.Dto.DepartmentIds.Length > 0)
            {
                var depIds = query.Dto.DepartmentIds;
                locationQuery = locationQuery.Where(l => l.LocDepartments.Any(d => depIds.Contains(d.DepartmentId.Value)));
            }

            long totalCount = await EntityFrameworkQueryableExtensions.LongCountAsync(locationQuery, cancellationToken);

            // Dynamic Sorting: Includes a default fallback key to prevent pagination instability
            locationQuery = query.Dto.SortBy switch
            {
                "Name" => locationQuery.OrderBy(l => l.Name.Value),
                "Created" => locationQuery.OrderBy(l => l.CreatedAt),
                _=> locationQuery.OrderBy(l => l.CreatedAt) // дату вместо Id.Value для стабильности
            };

            int pageSize = query.Dto.PageSize ?? 20;
            int page = query.Dto.Page.HasValue && query.Dto.Page.Value > 0 ? query.Dto.Page.Value : 1;
            int skipCount = (page - 1) * pageSize;

            // Выгружаем саму сущность из БД (EF Core соберет JSON из базы в объекты C#)
            var dbLocations = await locationQuery
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync(cancellationToken); // Здесь запрос уходит в Postgres

            var locations = dbLocations.Select(l => new LocationDto
            {
                Id = l.Id.Value,
                Name = l.Name.Value,
                Country = l.Address.Country,
                City = l.Address.City,
                Street = l.Address.Street,
                HouseNumber = l.Address.HouseNumber,
                Timezone = l.Timezone.Value,
                IsActive = l.IsActive,
                Created = l.CreatedAt,
                Updated = l.UpdatedAt,
            }).ToList();

            return new LocationResponseDto(locations, totalCount);
        }
    }
}
