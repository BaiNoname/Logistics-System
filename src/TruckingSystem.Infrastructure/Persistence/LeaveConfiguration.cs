using Microsoft.EntityFrameworkCore;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Infrastructure.Persistence;

internal static class LeaveConfiguration
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<PublicHoliday>(e =>
        {
            e.Property(h => h.Date).HasColumnType("date");
            e.Property(h => h.Name).HasMaxLength(100);
            e.Property(h => h.CreatedBy).HasMaxLength(256);
            e.HasIndex(h => h.Date).IsUnique();
        });
        builder.Entity<LeaveRecord>(e =>
        {
            e.HasOne(l => l.Employee).WithMany().HasForeignKey(l => l.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.Property(l => l.LeaveDate).HasColumnType("date");
            e.Property(l => l.Type).HasConversion<string>().HasMaxLength(30);
            e.Property(l => l.Note).HasMaxLength(500);
            e.Property(l => l.CreatedBy).HasMaxLength(256);
            e.HasIndex(l => new { l.EmployeeId, l.LeaveDate }).IsUnique();
            e.HasIndex(l => l.LeaveDate);
            e.HasIndex(l => l.BatchId);
        });
    }
}
