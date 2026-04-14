using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("locations");

            builder.HasKey(l => l.Id).HasName("pk_locations");

            builder
                .Property(l => l.Id)
                .HasConversion(
                l => l.Value,
                id => LocationId.Current(id))
                .HasColumnName("id");

            builder.ComplexProperty(l => l.Name, nb =>
            {
                nb.Property(v => v.Value)
                    .IsRequired()
                    .HasMaxLength(MyConstants.Length500)
                    .HasColumnName("name");
            });

            builder
                .OwnsOne(l => l.Address, nb =>
                {
                    nb.ToJson("address");
                    nb.Property(x => x.HouseNumber)
                       .IsRequired()
                       .HasJsonPropertyName("house_number");
                    nb.Property(x => x.Street)
                        .IsRequired()
                        .HasJsonPropertyName("street");
                    nb.Property(x => x.City)
                        .IsRequired()
                        .HasJsonPropertyName("city");
                    nb.Property(x => x.Country)
                        .IsRequired()
                        .HasJsonPropertyName("country");
                });

            builder.ComplexProperty(l => l.Timezone, nb =>
            {
                nb.Property(x => x.Value)
                   .IsRequired()
                   .HasMaxLength(MyConstants.Length500)
                   .HasColumnName("time_zone");
            });

            builder
                 .Property(d => d.IsActive)
                 .IsRequired()
                 .HasColumnName("is_active");

            //builder.HasMany(l => l.LocDepartments)
            //    .WithOne()
            //    .HasForeignKey(x => x.DepartmentId);

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
