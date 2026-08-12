using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Db;
using DirectoryService.Contracts.Departments.Tree;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Departments.Tree.GetAncestors
{
    public class GetDepartmentAncestorsHandler : IQueryHandler<DepartmentAncestorsResponseDto, GetDepartmentAncestorsQuery>
    {
        private readonly IReadDbContext _context; // только чтобы проверить существование и взять Path
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IValidator<GetDepartmentAncestorsQuery> _validator;
        private readonly ILogger<GetDepartmentAncestorsHandler> _logger;

        public GetDepartmentAncestorsHandler(
            IReadDbContext context,
            IDbConnectionFactory connectionFactory,
            IValidator<GetDepartmentAncestorsQuery> validator,
            ILogger<GetDepartmentAncestorsHandler> logger)
        {
            _context = context;
            _connectionFactory = connectionFactory;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<DepartmentAncestorsResponseDto, Errors>> Handle(GetDepartmentAncestorsQuery query, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToErrorList();
            }

            var childId = DepartmentId.Current(query.ChildId);

            // найти целевой узел, чтобы узнать его Path и проверить существование
            var targetDepartment = await _context.DepartmentsRead
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == childId, cancellationToken);

            if (targetDepartment == null)
            {
                return GeneralErrors.NotFound(query.ChildId, "department.not.found").ToErrors();
            }

            // оператор ltree @> - является предком для
            string rawSqlScript = """
            SELECT 
                d.id AS Id,
                d.name AS Name,
                d.identifier AS Slug,
                d.path AS Path,
                d.depth AS Depth,
                -- Считаем детей, чтобы UI знал, рисовать ли стрелочку
                (SELECT COUNT(*)::int FROM public.departments child WHERE child.parent_id = d.id AND child.is_active = true) AS ChildrenCount
            FROM public.departments d
            WHERE d.is_active = true
              AND d.id <> @TargetId              -- исключить сам узел из ответа
              AND d.path @> @TargetPath::ltree   -- вытащить всех предков одной командой
            ORDER BY d.depth ASC;                -- сортировка от корня до прямого родителя
            """;

            // параметры безопасности, исключая sql инъекции
            var sqlParameters = new DynamicParameters();
            sqlParameters.Add("TargetId", childId.Value);
            sqlParameters.Add("TargetPath", targetDepartment.Path.Value);

            try
            {
                using var dbConnection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
                var rawResult = await dbConnection.QueryAsync<dynamic>(rawSqlScript, sqlParameters);
                var rowList = rawResult.ToList();

                var ancestors = rowList.Select(row => new DepartmentTreeItemDto(
                    Id: (Guid)row.id,
                    Name: (string)row.name,
                    Slug: (string)row.slug,
                    Path: (string)row.path,
                    Depth: (int)row.depth,
                    HasChildren: (int)row.childrencount > 0,
                    ChildrenCount: (int)row.childrencount
                )).ToList();

                var response = new DepartmentAncestorsResponseDto(ancestors);
                return Result.Success<DepartmentAncestorsResponseDto, Errors>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error dapper request for department's ancestors");
                return GeneralErrors.DataBase().ToErrors();
            }
        }
    }
}
