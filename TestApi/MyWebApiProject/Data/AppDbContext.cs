using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWebApiProject.Models;

namespace MyWebApiProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet properties for model classes
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Location> Locations { get; set; }

        public virtual DbSet<UserInfo> UserInfo { get; set; }    

                // Override SaveChanges to set auto-generated properties
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Job>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Code = Guid.NewGuid().ToString();
                }
            }
            
            return base.SaveChangesAsync(cancellationToken);
        }

        // Implement OnModelCreating method if needed for configuring entity relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        // Configure Job-Location relationship
        modelBuilder.Entity<Job>()
            .HasOne(j => j.Location)
            .WithMany()
            .HasForeignKey(j => j.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Job-Department relationship
        modelBuilder.Entity<Job>()
            .HasOne(j => j.Department)
            .WithMany()
            .HasForeignKey(j => j.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);//ensures that deleting a Location or Department referenced by a Job is restricted to avoid orphaned records.
        }  
    }
}