using DirectoryService.Application.IRepositories;
using DirectoryService.Infrastructure.Repositories;
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
            services.AddDbContext<DirectoryServiceDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DirectoryServiceDb")));

            services.AddScoped<ILocationsRepository, LocationsRepository>();

            return services;
        }
    }
}
