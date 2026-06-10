using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DirectoryService.Application.Db
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
    }
}
