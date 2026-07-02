using Dapper;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Infrastructure.Repositories.DapperRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // EF Core
            var connectionString = configuration.GetConnectionString("DirectoryServiceDb");

            services.AddDbContext<DirectoryServiceDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<DirectoryServiceDbContext>());
            services.AddScoped<IDepartmentLocationsRepository, DepartmentLocationsRepository>();
            services.AddScoped<IDepartmentPositionsRepository, DepartmentPositionsRepository>();
            services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
            services.AddScoped<IPositionsRepository, PositionsRepository>();
            services.AddScoped<ITransactionManager, TransactionManager>();

            // фабрика для Dapper
            services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

            // ВЫБОР РЕАЛИЗАЦИИ РЕПОЗИТОРИЯ ЛОКАЦИЙ
            bool useDapper = false;

            if (useDapper)
            {
                // Dapper
                services.AddScoped<ILocationsRepository, DapperLocationsRepository>();
            }
            else
            {
                // EF Core
                services.AddScoped<ILocationsRepository, LocationsRepository>();
            }

            return services;
        }
    }
}
