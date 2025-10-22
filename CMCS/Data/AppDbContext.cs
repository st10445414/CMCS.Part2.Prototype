using CMCS.Models;
using Microsoft.EntityFrameworkCore;
using ClaimEntity = CMCS.Models.Claim;
using ClaimLineItemEntity = CMCS.Models.ClaimLineItem;
using SupportingDocumentEntity = CMCS.Models.SupportingDocument;
using ApprovalEntity = CMCS.Models.Approval;

namespace CMCS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Lecturer> Lecturers => Set<Lecturer>();
    public DbSet<User> Users => Set<User>();

    public DbSet<ClaimEntity> Claims => Set<ClaimEntity>();
    public DbSet<ClaimLineItemEntity> ClaimLineItems => Set<ClaimLineItemEntity>();
    public DbSet<SupportingDocumentEntity> SupportingDocuments => Set<SupportingDocumentEntity>();
    public DbSet<ApprovalEntity> Approvals => Set<ApprovalEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Lecturer>().HasKey(x => x.LecturerId);
        b.Entity<User>().HasKey(x => x.UserId);

        b.Entity<ClaimEntity>()
            .HasOne(c => c.Lecturer)
            .WithMany(l => l.Claims)
            .HasForeignKey(c => c.LecturerId);

        b.Entity<ClaimLineItemEntity>()
            .HasOne(i => i.Claim)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.ClaimId);

        b.Entity<SupportingDocumentEntity>()
            .HasOne(d => d.Claim)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.ClaimId);

        b.Entity<ApprovalEntity>()
            .HasOne(a => a.Claim)
            .WithMany(c => c.Approvals)
            .HasForeignKey(a => a.ClaimId);

        b.Entity<ApprovalEntity>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.ApprovedByUserId);

        b.Entity<ClaimEntity>()
            .HasIndex(c => new { c.LecturerId, c.Year, c.Month })
            .IsUnique();
    }
}

