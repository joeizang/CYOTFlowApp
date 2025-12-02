using FlowApplicationApp.Data.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowApplicationApp.Data.ModelConfigurations;

public class CodeOfConductDocumentEntityTypeConfiguration : IEntityTypeConfiguration<CodeOfConductDocument>
{
    public void Configure(EntityTypeBuilder<CodeOfConductDocument> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.OriginalFilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.HtmlContent)
            .IsRequired();

        builder.Property(c => c.UploadedAt)
            .IsRequired();

        builder.Property(c => c.Version)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.FileSize)
            .IsRequired();

        // Foreign key relationship with FlowMember
        builder.HasOne(c => c.UploadedByMember)
            .WithMany()
            .HasForeignKey(c => c.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Index on IsActive for quick retrieval of active document
        builder.HasIndex(c => c.IsActive);

        // Index on Version for sorting
        builder.HasIndex(c => c.Version);

        builder.ToTable("CodeOfConductDocuments");
    }
}
