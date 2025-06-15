using DailyCheckIn.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace DailyCheckIn.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Advance> Advances { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<TimeOff> TimeOffs { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AppUser>()
                .HasOne(a => a.ModifiedBy)
                .WithMany()
                .HasForeignKey(a => a.ModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Advance
            builder.Entity<Advance>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Advance>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Advance>()
                .HasOne(a => a.ModifiedBy)
                .WithMany()
                .HasForeignKey(a => a.ModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Attendance
            builder.Entity<Attendance>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attendance>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attendance>()
                .HasOne(a => a.ModifiedBy)
                .WithMany()
                .HasForeignKey(a => a.ModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attendance>()
                .HasMany(a => a.TimeOffsForTheDay)
                .WithOne(t => t.AttendanceDay)
                .HasForeignKey(t => t.AttendanceDayId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Attendance>()
                .HasIndex(a => new { a.Date, a.UserId })
                .IsUnique();

            // Payroll
            builder.Entity<Payroll>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payroll>()
                .HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payroll>()
                .HasOne(p => p.ModifiedBy)
                .WithMany()
                .HasForeignKey(p => p.ModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
