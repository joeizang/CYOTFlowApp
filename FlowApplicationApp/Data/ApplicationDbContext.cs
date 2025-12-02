using FlowApplicationApp.Data.DomainModels;
using FlowApplicationApp.Data.ModelConfigurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlowApplicationApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<FlowMember, FlowRoles, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply entity configurations
        builder.ApplyConfiguration(new CodeOfConductDocumentEntityTypeConfiguration());
    }

    public DbSet<FlowAuditioner> FlowAuditioners { get; set; }

    public DbSet<FlowMember> FlowMembers { get; set; }

    public DbSet<FlowRoles> FlowRoles { get; set; }
    
    public DbSet<CodeOfConductDocument> CodeOfConductDocuments { get; set; }
}