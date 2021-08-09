﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fondital.Shared.Models;
using Fondital.Shared.Models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fondital.Data.Configurations
{
    public class TraceConfiguration : IEntityTypeConfiguration<Trace>
    {
        public void Configure(EntityTypeBuilder<Trace> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(m => m.Id).UseIdentityColumn();

            builder.HasOne(m => m.Utente).WithMany();

            builder.ToTable("Trace");
        }
    }
}
