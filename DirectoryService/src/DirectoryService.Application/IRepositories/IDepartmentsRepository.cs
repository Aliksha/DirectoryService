using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DirectoryService.Application.IRepositories
{
    public interface IDepartmentsRepository
    {
        Task<Department?> GetBy(
            Expression<Func<Department, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Expression<Func<Department, object>>[] includes);

        Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default);

        Task<UnitResult<Errors>> CheckExisting(Guid[] ids, CancellationToken cancellationToken = default);
    }
}
