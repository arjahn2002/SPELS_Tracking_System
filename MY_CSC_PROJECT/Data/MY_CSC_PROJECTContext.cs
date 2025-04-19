using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MY_CSC_PROJECT.Models;

namespace MY_CSC_PROJECT.Data
{
    public class MY_CSC_PROJECTContext : DbContext
    {
        public MY_CSC_PROJECTContext (DbContextOptions<MY_CSC_PROJECTContext> options)
            : base(options)
        {
        }

        public DbSet<MY_CSC_PROJECT.Models.Document> Document { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.OtherFOs> OtherFOs { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.EvaluationStage> EvaluationStage { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.ProofingStage> ProofingStage { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.PostingStage> PostingStage { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.ApprovalStage> ApprovalStage { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.Province> Province { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.Position> Position { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.Role> Role { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.RolePermission> RolePermission { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.User> User { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.ReleasingStage> ReleasingStage { get; set; } = default!;
        public DbSet<MY_CSC_PROJECT.Models.SpecialEligibility> SpecialEligibility { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleID);
        }
        public DbSet<MY_CSC_PROJECT.Models.ODStage> ODStage { get; set; } = default!;
    }
}
