using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DirectoryService.Infrastructure.Repositories
{
    public class DepartmentsRepository : IDepartmentsRepository
    {
        private readonly DirectoryServiceDbContext _context;
        private readonly ILogger<DepartmentsRepository> _logger;

        public DepartmentsRepository(DirectoryServiceDbContext context, ILogger<DepartmentsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Departments.AddAsync(department, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Department {Department.Id} created", department.Id);

                return department.Id.Value;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure adding department {DepartmentId}", department.Id);
                return GeneralErrors.DataBase();
            }
        }

        public async Task<UnitResult<Errors>> CheckExisting(Guid[] ids, CancellationToken cancellationToken = default)
        {
            var distincts = ids.Distinct().ToArray();

            var departmentIds = distincts.Select(DepartmentId.Current).ToArray();

            var existings = await _context.Departments
                 .Where(x => departmentIds.Contains(x.Id))
                 .Select(x => x.Id.Value)
                 .ToListAsync(cancellationToken);

            var missings = distincts.Except(existings).ToArray();

            var errors = missings
               .Select(id => GeneralErrors.NotFound(id, "department"))
               .ToList();

            if (errors.Count > 0)
                return UnitResult.Failure(new Errors(errors));

            return UnitResult.Success<Errors>();
        }

        public async Task<Department?> GetBy(
            Expression<Func<Department, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Expression<Func<Department, object>>[] includes)
        {
            IQueryable<Department> query = _context.Departments;

            // если в метод передали связи типа d => d.Locations - накатываем их через Include
            if (includes is { Length: > 0 })
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }
    }
}
