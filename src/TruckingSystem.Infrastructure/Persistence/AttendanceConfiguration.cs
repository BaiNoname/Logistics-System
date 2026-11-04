using Microsoft.EntityFrameworkCore;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Infrastructure.Persistence;

internal static class AttendanceConfiguration
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<Warehouse>(e =>
        {
            e.Property(w => w.AttendanceRadiusMeters).HasDefaultValue(150);
            e.Property(w => w.AttendanceMaxAccuracyMeters).HasDefaultValue(100);
        });
        builder.Entity<AttendanceQrCode>(e =>
        {
            e.HasOne(q => q.Warehouse).WithMany().HasForeignKey(q => q.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            e.Property(q => q.WorkDate).HasColumnType("date");
            e.Property(q => q.Token).HasMaxLength(128);
            e.Property(q => q.CreatedBy).HasMaxLength(256);
            e.HasIndex(q => q.Token).IsUnique();
            e.HasIndex(q => new { q.WarehouseId, q.WorkDate }).IsUnique();
        });
        builder.Entity<AttendanceScan>(e =>
        {
            e.HasOne(s => s.Employee).WithMany().HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.QrCode).WithMany().HasForeignKey(s => s.QrCodeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Warehouse).WithMany().HasForeignKey(s => s.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            e.Property(s => s.WorkDate).HasColumnType("date");
            e.Property(s => s.Action).HasConversion<string>().HasMaxLength(20);
            e.Property(s => s.Result).HasConversion<string>().HasMaxLength(20);
            e.Property(s => s.RejectReason).HasMaxLength(500);
            e.HasIndex(s => new { s.EmployeeId, s.WorkDate });
            e.HasIndex(s => s.WorkDate);
        });
        builder.Entity<AttendanceRecord>(e =>
        {
            e.HasOne(r => r.Employee).WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.CheckInWarehouse).WithMany().HasForeignKey(r => r.CheckInWarehouseId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.CheckOutWarehouse).WithMany().HasForeignKey(r => r.CheckOutWarehouseId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<AttendanceAdjustmentRequest>().WithMany().HasForeignKey(r => r.AdjustmentRequestId).OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.WorkDate).HasColumnType("date");
            e.Ignore(r => r.EffectiveCheckInAt);
            e.Ignore(r => r.EffectiveCheckOutAt);
            e.HasIndex(r => new { r.EmployeeId, r.WorkDate }).IsUnique();
            e.HasIndex(r => r.WorkDate);
        });
        builder.Entity<AttendanceAdjustmentRequest>(e =>
        {
            e.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.Property(a => a.WorkDate).HasColumnType("date");
            e.Property(a => a.Reason).HasConversion<string>().HasMaxLength(30);
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(a => a.Note).HasMaxLength(500);
            e.Property(a => a.ReviewedBy).HasMaxLength(256);
            e.Property(a => a.ReviewNote).HasMaxLength(500);
            e.HasIndex(a => new { a.EmployeeId, a.WorkDate }, "IX_AttendanceAdjustmentRequests_EmployeeId_WorkDate");
            e.HasIndex(a => a.Status);
            // Mỗi nhân viên chỉ có tối đa 1 yêu cầu ĐANG CHỜ cho 1 ngày.
            e.HasIndex(a => new { a.EmployeeId, a.WorkDate }, "IX_AttendanceAdjustmentRequests_OnePending").IsUnique()
                .HasFilter("\"Status\" = 'Pending'");
        });
    }
}
