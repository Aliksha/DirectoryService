using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure.BackgroundServices
{
    public class SoftDeleteGarbageCollector : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SoftDeleteGarbageCollector> _logger;
        private readonly SoftDeleteOptions _options; // для хранения конфига

        public SoftDeleteGarbageCollector(
            IServiceProvider serviceProvider,
            ILogger<SoftDeleteGarbageCollector> logger,
            IOptions<SoftDeleteOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value; // чистый объект с настройками
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("soft delete garbage collector started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var thresholdDate = DateTime.UtcNow.AddDays(-_options.ExpirationDays);

                    _logger.LogInformation("starting background soft-delete cleanup task...");

                    // scope и достаем контекст здесь, чтоб передать его в вызовы ниже
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

                    await CleanUpTable<Department>(context, thresholdDate, _options.BatchSize, stoppingToken);
                    await CleanUpTable<Location>(context, thresholdDate, _options.BatchSize, stoppingToken);
                    await CleanUpTable<Position>(context, thresholdDate, _options.BatchSize, stoppingToken);

                    _logger.LogInformation("cleanup cycle finished. sleeping for {Hours} hours...", _options.IntervalHours);

                    await Task.Delay(TimeSpan.FromHours(_options.IntervalHours), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // app тушится, выходим без паники
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Garbage collector execution failed unexpectedly!");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // if error - спим 5 мин и пробуем снова
                }
            }
        }

        // универсальный дженерик метод. интерфейсы в домене не нужны!
        private async Task CleanUpTable<TEntity>(
            DirectoryServiceDbContext context,
            DateTime thresholdDate,
            int batchSize,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            int deletedCount;
            int totalDeleted = 0;
            var tableName = typeof(TEntity).Name;

            _logger.LogInformation("starting cleanup for {Table} rows deleted before {ThresholdDate}", tableName, thresholdDate);

            try
            {
                do
                {
                    // EF.Property, чтобы динамически читать свойства из shadow-state или обычных полей базы
                    var query = context.Set<TEntity>()
                        .IgnoreQueryFilters() // обязательно отключить глобальные фильтры через IgnoreQueryFilters()
                        .Where(x => EF.Property<bool>(x, "SoftDeleted") &&
                                    EF.Property<DateTime?>(x, "DeletedAt") < thresholdDate)
                        .Take(batchSize);

                    // выполняет чистый SQL за одну короткую транзакцию, удаляет пачку сразу в БД без загрузки сущностей в оперативку
                    deletedCount = await query.ExecuteDeleteAsync(cancellationToken);
                    totalDeleted += deletedCount;

                    if (deletedCount > 0)
                    {
                        // дать базе подышать
                        await Task.Delay(100, cancellationToken);
                    }
                } while (deletedCount > 0);
            }
            catch (Exception ex)
            {
                // для конкретной упавшей таблицы и не ломаем общий цикл
                _logger.LogError(ex, "failed to clean up expired rows for table {Table}", tableName);
            }
        }
    }
}
