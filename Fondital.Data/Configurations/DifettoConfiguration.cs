﻿using Fondital.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fondital.Data.Configurations
{
    public class DifettoConfiguration : IEntityTypeConfiguration<Difetto>
    {
        public void Configure(EntityTypeBuilder<Difetto> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(m => m.Id).UseIdentityColumn();

            builder.Property(m => m.NomeItaliano).IsRequired();
            builder.HasIndex(m => m.NomeItaliano).IsUnique();
            builder.Property(m => m.NomeRusso).IsRequired();
            builder.HasIndex(m => m.NomeRusso).IsUnique();

            builder.ToTable("Difetti");
        }
    }
}
