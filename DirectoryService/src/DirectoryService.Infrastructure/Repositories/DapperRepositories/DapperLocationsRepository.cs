using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Db;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedKernel;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure.Repositories.DapperRepositories
{
    public class DapperLocationsRepository : ILocationsRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<DapperLocationsRepository> _logger;

        public DapperLocationsRepository(IDbConnectionFactory connectionFactory, ILogger<DapperLocationsRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<Result<Guid, Error>> AddLocationAsync(Location location, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO locations (id, name, address, is_active, time_zone, created_at, updated_at) 
                VALUES (@Id, @Name, @AddressJson::jsonb, @IsActive, @Timezone, @CreatedAt, @UpdatedAt);";

            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

            try
            {
                // объект для json
                var addressObj = new
                {
                    house_number = location.Address.HouseNumber,
                    street = location.Address.Street,
                    city = location.Address.City,
                    country = location.Address.Country,
                };
                var addressJson = JsonSerializer.Serialize(addressObj);

                var parameters = new
                {
                    Id = location.Id.Value,
                    Name = location.Name.Value,
                    AddressJson = addressJson,
                    IsActive = location.IsActive,
                    Timezone = location.Timezone.Value,
                    CreatedAt = location.CreatedAt,
                    UpdatedAt = location.UpdatedAt,
                };

                var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);

                return Result.Success<Guid, Error>(location.Id.Value);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // kод уникального ключа в Postgres
            {
                _logger.LogError(ex, "Location already exists.");
                return GeneralErrors.DataBase();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure adding location {Location.Id}", location.Id);
                return GeneralErrors.DataBase();
            }
        }

        public async Task<UnitResult<Errors>> CheckExisting(Guid[] ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || ids.Length == 0)
            {
                var emptyError = GeneralErrors.ValueIsRequired("locations.ids.empty");
                return UnitResult.Failure(emptyError.ToErrors());
            }

            var distinctIds = ids.Distinct().ToList();

            const string sql = "SELECT id FROM locations WHERE id = ANY(@Ids);";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

                var command = new CommandDefinition(sql, new { Ids = distinctIds.ToArray() }, cancellationToken: cancellationToken);

                // выгружаем из базы список существующих Guid
                var existingIdsEnumerable = await connection.QueryAsync<Guid>(command);
                var existingIds = existingIdsEnumerable.ToList();

                // чего в бд нет
                var missingIds = distinctIds.Except(existingIds).ToList();

                if (missingIds.Count > 0)
                {
                    // список детальных ошибок для каждого отсутствующего ID
                    var errorsList = missingIds
                        .Select(id => GeneralErrors.NotFound(id, "location"))
                        .ToList();

                    return UnitResult.Failure(new Errors(errorsList));
                }

                return UnitResult.Success<Errors>();
            }
            catch (Exception ex)
            {
                // cистемная ошибка
                var systemError = Error.Failure("database.error", $"Database verification error: {ex.Message}");
                return UnitResult.Failure(new Errors([systemError]));
            }
        }

        public Task<Result<IReadOnlyCollection<Location>, Errors>> GetLocationsAsync(List<LocationId> ids, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public async Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            const string sql = "SELECT NOT EXISTS(SELECT 1 FROM locations WHERE name = @Name);";

            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

                // исключить случайные пробелы по краям
                var parameters = new { Name = name.Trim() };
                var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

                // легковесный scalar-запрос, postgres возвращает bool
                return await connection.ExecuteScalarAsync<bool>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "location unique name error  {Name}", name);
                throw;
            }
        }
    }
}
