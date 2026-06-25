using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DirectoryService.Infrastructure.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("departments");

            builder.HasKey(d => d.Id).HasName("pk_departments");

            builder.Property(d => d.Id)
                .HasConversion(
                d => d.Value, // записывает
                id => DepartmentId.Current(id)) // читает
                .HasColumnName("id");

            //builder
            //    .Property(d => d.Name)
            //    .IsRequired()
            //    .HasMaxLength(MyConstants.Length500)
            //    .HasColumnName("name");

            //builder
            //    .OwnsOne(d => d.Name, nb =>
            //    {
            //        nb
            //        .Property(x => x.Value)
            //        .IsRequired()
            //        .HasMaxLength(MyConstants.Length500)
            //        .HasColumnName("name");
            //    });

            builder
              .OwnsOne(d => d.Name, nb =>
              {
                  nb
                  .Property(x => x.Value)
                  .IsRequired()
                  .HasMaxLength(MyConstants.Length500)
                  .HasColumnName("name");

                  nb.HasIndex(x => x.Value)
                 .HasDatabaseName("uq_departments_name")
                 .IsUnique();
              });

            builder
                .OwnsOne(d => d.Identifier, nb =>
                {
                    nb
                    .Property(x => x.Value)
                    .IsRequired()
                    .HasMaxLength(MyConstants.Length500)
                    .HasColumnName("identifier");
                });

            builder
                .Property(d => d.ParentId)
                .HasColumnName("parent_id");

            builder
                .OwnsOne(d => d.Path, nb =>
                {
                    nb.Property(x => x.Value)
                    .IsRequired()
                    .HasColumnType("ltree")
                    .HasMaxLength(MyConstants.Length500)
                    .HasColumnName("path");
                });

            builder
                .Property(d => d.Depth)
                .IsRequired()
                .HasColumnName("depth");

            builder
                .Property(d => d.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            builder
                .HasMany(d => d.ChildDepartments)
                .WithOne()
                .IsRequired(false)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(d => d.Locations)
                .WithOne()
                .HasForeignKey(d => d.DepartmentId);

            builder
               .HasMany(d => d.Positions)
               .WithOne()
               .HasForeignKey(d => d.DepartmentId);

            builder
                .Property(d => d.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder
                .Property(d => d.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");
        }
    }
}
