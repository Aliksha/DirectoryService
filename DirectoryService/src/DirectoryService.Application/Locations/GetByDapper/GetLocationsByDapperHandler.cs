using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Db;
using DirectoryService.Contracts.Locations.ForDapperCase;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DirectoryService.Application.Locations.GetByDapper
{
    public class GetLocationsByDapperHandler : IQueryHandler<LocationsPagedResponseDto, GetLocationsQuery>
    {
        private readonly IDbConnectionFactory _connection;
        private readonly IValidator<GetLocationsQuery> _validator;
        private readonly ILogger<GetLocationsByDapperHandler> _logger;

        public GetLocationsByDapperHandler(
            IDbConnectionFactory connection,
            IValidator<GetLocationsQuery> validator,
            ILogger<GetLocationsByDapperHandler> logger)
        {
            _connection = connection;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<LocationsPagedResponseDto, Errors>> Handle(GetLocationsQuery query, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToErrorList();
            }

            var dto = query.Dto;

            // маппим безопасные строки
            string sortBySql = dto.SortBy?.ToLower().Trim()
                switch
            {
                "createdat" => "fl.created_at",
                "departmentcount" => "department_count",
                _ => "fl.name"
            };

            string sortDirSql = dto.SortDir?.ToLower().Trim() == "desc" ? "DESC" : "ASC";

            // сюда cобираем параметры и условия фильтрации
            var sqlParameters = new DynamicParameters();
            var sqlWhereFilters = new List<string>();
            var sqlHavingFilters = new List<string>();

            // текстовый поиск
            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                sqlWhereFilters.Add("l.name ILIKE @SearchPattern");
                sqlParameters.Add("SearchPattern", $"%{dto.Search.Trim()}");
            }

            // по минимальному числу департаментов
            if (dto.MinDepartmentCount.HasValue)
            {
                sqlHavingFilters.Add("COUNT(dl.id) >= @MinCount");
                sqlParameters.Add("MinCount", dto.MinDepartmentCount.Value);
            }

            // cобираем строки фильтров для SQL
            string whereClause = sqlWhereFilters.Count > 0
                ? $"WHERE {string.Join(" AND ", sqlWhereFilters)}"
                : string.Empty;

            string havingClause = sqlHavingFilters.Count > 0
                ? $"HAVING {string.Join(" AND ", sqlHavingFilters)}"
                : string.Empty;

            // красивый СТЕ запрос
            string rawSqlScript = $"""
                WITH FilteredLocations AS (
                    SELECT 
                        l.id AS Id,
                        l.name AS Name,
                        l.created_at AS CreatedAt,
                        COALESCE(l.address->>'houseNumber', '') AS HouseNumber,
                        COALESCE(l.address->>'street', '') AS Street,
                        COALESCE(l.address->>'city', '') AS City,
                        COALESCE(l.address->>'country', '') AS Country,
                        COUNT(dl.id)::int AS department_count
                    FROM public.locations l
                    LEFT JOIN public.department_locations dl ON l.id = dl.location_id
                    {whereClause}
                    GROUP BY l.id, l.name, l.created_at, l.address
                    {havingClause}
                ),
                TotalCountCTE AS (
                    SELECT COUNT(*) AS total_count FROM FilteredLocations
                )
                SELECT
                    fl.*,
                    tc.total_count AS TotalCount
                FROM FilteredLocations fl
                CROSS JOIN TotalCountCTE tc
                ORDER BY {sortBySql} {sortDirSql}
                LIMIT @PageSize OFFSET @SkipCount;
                """;

            int pageSize = dto.PageSize ?? 20;
            int page = dto.Page ?? 1;
            int skipCount = (page - 1) * pageSize;

            sqlParameters.Add("PageSize", pageSize);
            sqlParameters.Add("SkipCount", skipCount);

            try
            {
                using var dbConnection = await _connection.CreateConnectionAsync(cancellationToken);

                // выхываем метод Даппер у объекта соединения
                var rawResult = await dbConnection.QueryAsync<dynamic>(rawSqlScript, sqlParameters);
                var rowList = rawResult.ToList();

                // если база пустая, totalCount равен 0
                long totalCount = rowList.Count > 0 ? Convert.ToInt64(rowList[0].totalcount) : 0;

                // маппим динамические строки даппер в дто
                var items = rowList.Select(row => new LocationListItemDto(
                    Id: (Guid)row.id,
                    Name: (string)row.name,
                    HouseNumber: (string)row.housenumber,
                    Street: (string)row.street,
                    City: (string)row.city,
                    Country: (string)row.country,
                    CreatedAt: (DateTime)row.createdat,
                    DepartmentCount: (int)row.department_count
                )).ToList();

                var response = new LocationsPagedResponseDto(items, totalCount);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка выполнения Dapper запроса списка локаций");
                return GeneralErrors.DataBase().ToErrors();
            }
        }
    }
}
