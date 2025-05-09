using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SPELS_TRACKING_SYSTEM.Models;

namespace SPELS_TRACKING_SYSTEM.Data
{
    public class SPELS_TRACKING_SYSTEMContext : DbContext
    {
        public SPELS_TRACKING_SYSTEMContext(DbContextOptions<SPELS_TRACKING_SYSTEMContext> options)
            : base(options)
        {
        }

        public DbSet<SPELS_TRACKING_SYSTEM.Models.Document> Document { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.OtherFOs> OtherFOs { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.EvaluationStage> EvaluationStage { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.ProofingStage> ProofingStage { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.PostingStage> PostingStage { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.ApprovalStage> ApprovalStage { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.Province> Province { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.Position> Position { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.Role> Role { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.RolePermission> RolePermission { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.User> User { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.ReleasingStage> ReleasingStage { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.SpecialEligibility> SpecialEligibility { get; set; } = default!;
        public DbSet<SPELS_TRACKING_SYSTEM.Models.DocumentHistory> DocumentHistory { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleID);
        }
        public DbSet<SPELS_TRACKING_SYSTEM.Models.ODStage> ODStage { get; set; } = default!;
    }
}
