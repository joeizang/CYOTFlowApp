using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowApplicationApp.Data.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowApplicationApp.Data.ModelConfigurations;

public class FlowAuditionerEntityTypeConfiguration : IEntityTypeConfiguration<FlowAuditioner>
{
    public void Configure(EntityTypeBuilder<FlowAuditioner> builder)
    {
        builder.HasKey(fa => fa);

        builder.Property(fm => fm.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(fm => fm.LastName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(fm => fm.Email)
            .IsRequired()
            .HasMaxLength(70);
        builder.Property(fm => fm.ProfileImageUrl)
            .HasMaxLength(600);
        builder.Property(fa => fa.HowTheyStartedHearingGod)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(fm => fm.Bio)
            .HasMaxLength(1500)
            .IsRequired();

        builder.Property(fm => fm.CoverSpeech)
            .HasMaxLength(2000)
            .IsRequired();
    }
}
