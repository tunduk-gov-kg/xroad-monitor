﻿using System.Threading;
using System.Threading.Tasks;
using Catalog.DataAccessLayer.Domain.Entity;
using Catalog.DataAccessLayer.Domain.Entity.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Catalog.DataAccessLayer {
    public class AppDbContext : IdentityDbContext {
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions)
            : base(dbContextOptions) {
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<SecurityServer> SecurityServers { get; set; }
        public DbSet<SubSystem> SubSystems { get; set; }
        public DbSet<MemberService> MemberServices { get; set; }
        public DbSet<SubSystemService> SubSystemServices { get; set; }
        public DbSet<MemberInfo> MemberInfoRecords { get; set; }
        public DbSet<MemberRole> MemberRoles { get; set; }
        public DbSet<MemberStatus> MemberStatuses { get; set; }
        public DbSet<MemberType> MemberTypes { get; set; }
        public DbSet<MemberInfoRoleReference> MemberInfoRoleReferences { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.ApplyConfiguration(new MemberConfiguration());
            modelBuilder.ApplyConfiguration(new MemberInfoConfiguration());
            modelBuilder.ApplyConfiguration(new MemberInfoRoleReferenceConfiguration());
            modelBuilder.ApplyConfiguration(new MemberRoleConfiguration());
            modelBuilder.ApplyConfiguration(new MemberServiceConfiguration());
            modelBuilder.ApplyConfiguration(new MemberStatusConfiguration());
            modelBuilder.ApplyConfiguration(new MemberTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SecurityServerConfiguration());
            modelBuilder.ApplyConfiguration(new SubSystemConfiguration());
            modelBuilder.ApplyConfiguration(new SubSystemServiceConfiguration());
        }

        public override int SaveChanges() {
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess) {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken()) {
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken()) {
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}