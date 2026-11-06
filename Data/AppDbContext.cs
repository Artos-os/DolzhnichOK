using DolzhnikOk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DolzhnikOk.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
    public DbSet<StudentProfile> Students => Set<StudentProfile>();
    public DbSet<TeacherProfile> Teachers => Set<TeacherProfile>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ResitSession> ResitSessions => Set<ResitSession>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().HasIndex(x => x.NormalizedEmail)
            .IsUnique().HasDatabaseName("EmailIndex");
        builder.Entity<Department>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<StudentGroup>().HasIndex(x => new { x.DepartmentId, x.Code }).IsUnique();
        builder.Entity<Classroom>().HasIndex(x => new { x.DepartmentId, x.Number }).IsUnique();
        builder.Entity<Subject>().HasIndex(x => new { x.DepartmentId, x.Name }).IsUnique();

        builder.Entity<StudentProfile>()
            .HasOne(x => x.User).WithOne(x => x.StudentProfile)
            .HasForeignKey<StudentProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<TeacherProfile>()
            .HasOne(x => x.User).WithOne(x => x.TeacherProfile)
            .HasForeignKey<TeacherProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Department>().HasMany(x => x.Teachers).WithOne(x => x.Department).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Department>().HasMany(x => x.StudentGroups).WithOne(x => x.Department).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Department>().HasMany(x => x.Subjects).WithOne(x => x.Department).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Department>().HasMany(x => x.Classrooms).WithOne(x => x.Department).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentGroup>().HasMany(x => x.Students).WithOne(x => x.Group).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<TeacherProfile>().HasMany(x => x.ResitSessions).WithOne(x => x.Teacher).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Classroom>().HasMany(x => x.ResitSessions).WithOne(x => x.Classroom).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Subject>().HasMany(x => x.ResitSessions).WithOne(x => x.Subject).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>().HasIndex(x => new { x.ResitSessionId, x.StudentId })
            .IsUnique().HasFilter("\"Status\" = 'Active'");
        builder.Entity<Booking>().HasOne(x => x.Student).WithMany(x => x.Bookings)
            .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Booking>().HasOne(x => x.ResitSession).WithMany(x => x.Bookings)
            .HasForeignKey(x => x.ResitSessionId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Booking>().Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Entity<ResitSession>().Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Entity<ResitSession>().ToTable(t => t.HasCheckConstraint("CK_ResitSessions_Time", "\"EndsAt\" > \"StartsAt\""));
        builder.Entity<ResitSession>().ToTable(t => t.HasCheckConstraint("CK_ResitSessions_Capacity", "\"Capacity\" > 0"));
        builder.Entity<ResitSession>().ToTable(t => t.HasCheckConstraint(
            "CK_ResitSessions_BookingDeadline",
            "\"BookingDeadline\" IS NULL OR \"BookingDeadline\" <= \"StartsAt\""));

        builder.Entity<Comment>().HasOne(x => x.Author).WithMany(x => x.Comments)
            .HasForeignKey(x => x.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Comment>().HasOne(x => x.ResitSession).WithMany(x => x.Comments)
            .HasForeignKey(x => x.ResitSessionId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notification>().HasOne(x => x.User).WithMany(x => x.Notifications)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Notification>().HasOne(x => x.ResitSession).WithMany(x => x.Notifications)
            .HasForeignKey(x => x.ResitSessionId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Notification>().HasOne(x => x.Booking).WithMany()
            .HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.SetNull);
    }
}
