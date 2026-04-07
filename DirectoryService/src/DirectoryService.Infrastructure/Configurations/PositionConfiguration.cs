using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure.Configurations
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.ToTable("positions");

            builder
                .HasKey(p => p.Id)
                .HasName("pk_positions");

            builder
                .Property(p => p.Id)
                .HasConversion(
                 p => p.Value,
                id => PositionId.Current(id))
                .HasColumnName("id");

            builder.ComplexProperty(p => p.Name, nb =>
            {
                nb.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(MyConstants.Length500)
                .HasColumnName("name");
            });

            builder.Property(p => p.Description)
                .IsRequired(false)
                .HasMaxLength(MyConstants.Length500)
                .HasColumnName("description");

            builder
                .Property(d => d.IsActive)
               .IsRequired()
                .HasColumnName("is_active");


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
