using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowApplicationApp.Data.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowApplicationApp.Data.ModelConfigurations;

public class FlowRolesEntityTypeConfiguration : IEntityTypeConfiguration<FlowRoles>
{
    public void Configure(EntityTypeBuilder<FlowRoles> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.RoleName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(f => f.RoleDescription)
            .HasMaxLength(200);
    }
}