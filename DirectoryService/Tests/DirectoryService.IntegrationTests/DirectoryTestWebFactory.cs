using DirectoryService.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Testcontainers.PostgreSql;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DirectoryService.IntegrationTests
{
    public class DirectoryTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres")
            .WithDatabase("directory_service_TESTS_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        private Respawner _respawner = null!;
        private DbConnection _dbConnection = null!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // чтобы логгер не конфликтовал с основным хостом
            builder.UseSetting("Serilog:PreserveStaticLogger", "true");

            builder.ConfigureTestServices(services =>
            {
                var garbageCollectorService = services.FirstOrDefault(d =>
                    d.ImplementationType?.Name == "SoftDeleteGarbageCollector" ||
                    d.ServiceType.Name == "SoftDeleteGarbageCollector");

                if (garbageCollectorService != null)
                {
                    services.Remove(garbageCollectorService);
                }

                // удаляем старый контекст и старые опции, которые успел накатать Program.cs
                services.RemoveAll<DbContextOptions<DirectoryServiceDbContext>>();
                services.RemoveAll<DirectoryServiceDbContext>();

                // регистрируем контекст заново, жестко передавая строку от тест-контейнера
                services.AddDbContext<DirectoryServiceDbContext>(options =>
                {
                    options.UseNpgsql(_dbContainer.GetConnectionString());
                });
            });
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            await using var scope = Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
            await _dbConnection.OpenAsync();

            await InitializerRespawner();
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();

            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            //using var connection = new Npgsql.NpgsqlConnection(_dbContainer.GetConnectionString());
            //await connection.OpenAsync();

            await _respawner.ResetAsync(_dbConnection);
        }

        private async Task InitializerRespawner()
        {
            //using var connection = new Npgsql.NpgsqlConnection(_dbContainer.GetConnectionString());
            //await connection.OpenAsync();

            _respawner = await Respawner.CreateAsync(
                _dbConnection,
                new RespawnerOptions
                {
                    DbAdapter = DbAdapter.Postgres,
                    SchemasToInclude = ["public"],
                });
        }
    }
}
