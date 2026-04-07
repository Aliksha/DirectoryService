using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure
{
    public class DirectoryServiceDbContext : DbContext
    {
        private readonly string _connectionString;

        public DirectoryServiceDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        // EF сore
        public DirectoryServiceDbContext(DbContextOptions<DirectoryServiceDbContext> options)
            : base(options)
        {
        }
    
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder
            //    .UseLoggerFactory(MyLoggerFactory)
            //    .UseNpgsql(_connectionString);

            optionsBuilder.UseLoggerFactory(MyLoggerFactory); 

            // настроить БД только если еще не настроена извне
            if (!optionsBuilder.IsConfigured && _connectionString != null)
            {
                optionsBuilder.UseNpgsql(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // вызвать базовый метод в начале
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly); // соберет все конфигурации
        }

        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Position> Positions => Set<Position>();
        public DbSet<DepartmentLocation> DepartmentLocations => Set<DepartmentLocation>();
        public DbSet<DepartmentPosition> DepartmentPositions => Set<DepartmentPosition>();


        public static readonly ILoggerFactory MyLoggerFactory
            = LoggerFactory.Create(builder => { builder.AddConsole(); });
    }
}
