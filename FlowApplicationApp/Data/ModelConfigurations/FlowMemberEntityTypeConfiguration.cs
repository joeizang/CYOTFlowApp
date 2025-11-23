using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowApplicationApp.Data.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowApplicationApp.Data.ModelConfigurations;

public class FlowMemberEntityTypeConfiguration : IEntityTypeConfiguration<FlowMember>
{
    public void Configure(EntityTypeBuilder<FlowMember> builder)
    {
        builder.HasKey(fm => fm.Id);

        builder.Property(fm => fm.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(fm => fm.LastName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(fm => fm.Email)
            .IsRequired()
            .HasMaxLength(70);

        builder.Property(fm => fm.Bio)
            .HasMaxLength(1500);

        builder.Property(fm => fm.CoverSpeech)
            .HasMaxLength(2000);

        builder.HasMany(fm => fm.Roles)
            .WithMany();
    }
}