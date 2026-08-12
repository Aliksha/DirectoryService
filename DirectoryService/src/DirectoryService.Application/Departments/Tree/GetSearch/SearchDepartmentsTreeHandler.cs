using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Db;
using DirectoryService.Contracts.Departments.Tree;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Tree.GetSearch
{
    public class SearchDepartmentsTreeHandler : IQueryHandler<DepartmentSearchResponseDto, SearchDepartmentsTreeQuery>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IValidator<SearchDepartmentsTreeQuery> _validator;
        private readonly ILogger<SearchDepartmentsTreeHandler> _logger;

        public SearchDepartmentsTreeHandler(
            IDbConnectionFactory connectionFactory,
            IValidator<SearchDepartmentsTreeQuery> validator,
            ILogger<SearchDepartmentsTreeHandler> logger)
        {
            _connectionFactory = connectionFactory;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<DepartmentSearchResponseDto, Errors>> Handle(SearchDepartmentsTreeQuery query, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToErrorList();
            }

            string rawSqlScript = """
            WITH MatchedDepartments AS (
                SELECT id, path 
                FROM public.departments 
                WHERE is_active = true AND name ILIKE @SearchPattern
            )
            SELECT DISTINCT
                d.id AS Id,
                d.name AS Name,
                d.identifier AS Slug,
                d.path AS Path,
                d.depth AS Depth,
                (SELECT COUNT(*)::int FROM public.departments child WHERE child.parent_id = d.id AND child.is_active = true) AS ChildrenCount
            FROM public.departments d
            JOIN MatchedDepartments md ON d.path @> md.path -- ltree: подтягиваем всех предков для раскрытия веток
            WHERE d.is_active = true
            ORDER BY d.depth ASC, d.name ASC;
            """;

            var sqlParameters = new DynamicParameters();
            sqlParameters.Add("SearchPattern", $"%{query.Q.Trim()}%");

            try
            {
                using var dbConnection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
                var rawResult = await dbConnection.QueryAsync<dynamic>(rawSqlScript, sqlParameters);
                var rowList = rawResult.ToList();

                var items = rowList.Select(row => new DepartmentTreeItemDto(
                    Id: (Guid)row.id,
                    Name: (string)row.name,
                    Slug: (string)row.slug,
                    Path: (string)row.path,
                    Depth: (int)row.depth,
                    HasChildren: (int)row.childrencount > 0,
                    ChildrenCount: (int)row.childrencount
                )).ToList();

                var response = new DepartmentSearchResponseDto(items);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "tree search error");
                return GeneralErrors.DataBase().ToErrors();
            }
        }
    }
}
