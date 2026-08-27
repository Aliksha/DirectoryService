using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure.Configurations
{
    public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
    {
        public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
        {
            builder.ToTable("department_positions");

            builder.HasKey(x => x.Id).HasName("pk_department_positions");

            builder.Property(dp => dp.Id)
                .IsRequired()
                .HasConversion(
                dp => dp.Value,
                id => DepartmentPositionId.Current(id))
                .HasColumnName("id");

            builder.Property(dp => dp.DepartmentId)
                .IsRequired()
                .HasConversion(
                dp => dp.Value,
                id => DepartmentId.Current(id))
               .HasColumnName("department_id");

            builder.Property(dp => dp.PositionId)
                .IsRequired()
                .HasConversion(
                x => x.Value,
                id => PositionId.Current(id))
                .HasColumnName("position_id");

            builder
                .HasOne<Department>()
                .WithMany(d => d.Positions)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_department_position_department_id");

            builder
                .HasOne<Position>()
                .WithMany(d => d.Departments)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_department_position_position_id");

            builder.HasIndex(dp => dp.DepartmentId)
               .HasDatabaseName("IX_department_positions_department_id")
               .HasFilter("\"SoftDeleted\" = false");

            builder.HasIndex(dp => dp.PositionId)
                .HasDatabaseName("IX_department_positions_position_id");
        }
    }
}
