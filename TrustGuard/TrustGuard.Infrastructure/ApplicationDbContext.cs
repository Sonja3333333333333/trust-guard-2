using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Тепер це запрацює!
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TrustGuard.Domain;
using TrustGuard.Domain.Entities;

namespace TrustGuard.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<NewsCheck> NewsChecks { get; set; } = null!;
        public DbSet<AnalysisReport> AnalysisReports { get; set; } = null!;
        public DbSet<ExternalSource> ExternalSources { get; set; } = null!;
        public DbSet<MediaMetadata> MediaMetadatas { get; set; } = null!;

        public DbSet<DomainTrustRecord> DomainTrustRecords { get; set; }

        public DbSet<TrustedDomain> TrustedDomains { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<NewsCheck>()
                .HasOne(n => n.AnalysisReport)
                .WithOne(a => a.NewsCheck)
                .HasForeignKey<AnalysisReport>(a => a.NewsCheckId);

            builder.Entity<NewsCheck>()
                .HasMany(n => n.ExternalSources)
                .WithOne(e => e.NewsCheck)
                .HasForeignKey(e => e.NewsCheckId);

            builder.Entity<NewsCheck>()
                .HasMany(n => n.MediaMetadatas)
                .WithOne(m => m.NewsCheck)
                .HasForeignKey(m => m.NewsCheckId);
            builder.Entity<DomainTrustRecord>()
                .HasIndex(d => d.DomainName)
                .IsUnique();
        }
    }
}