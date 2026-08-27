using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure.Configurations
{
    public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
    {
        public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
        {
            builder.ToTable("department_locations");

            builder.HasKey(x => x.Id).HasName("pk_department_locations");

            builder.Property(dl => dl.Id)
                .IsRequired()
                .HasConversion(
                x => x.Value,
                id => DepartmentLocationId.Current(id))
                .HasColumnName("id");

            builder.Property(dl => dl.DepartmentId)
                .IsRequired()
                .HasConversion(
                x => x.Value,
                id => DepartmentId.Current(id))
               .HasColumnName("department_id");

            builder.Property(dl => dl.LocationId)
                .IsRequired()
                .HasConversion(
                x => x.Value,
                id => LocationId.Current(id))
                .HasColumnName("location_id");

            builder
                .HasOne<Department>()
                .WithMany(dl => dl.Locations)
                .HasForeignKey(dl => dl.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_department_locations_department_id");

            builder
                .HasOne<Location>()
                .WithMany(dl => dl.LocDepartments)
                .HasForeignKey(dl => dl.LocationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_department_locations_location_id");

            builder.HasIndex(dl => dl.DepartmentId)
                .HasDatabaseName("IX_department_locations_department_id");

            builder.HasIndex(dl => dl.LocationId)
                .HasDatabaseName("IX_department_locations_location_id");
        }
    }
}
