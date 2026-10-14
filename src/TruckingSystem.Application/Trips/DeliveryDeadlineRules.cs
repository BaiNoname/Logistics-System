using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Trips;

/// <summary>HẠN GIAO — 1 quy tắc DUY NHẤT dùng chung cho Chia xe (cảnh báo trễ hạn), lưới
/// Chuyến (cột Tình trạng hạn), Thông báo "Hạn giao" và Báo cáo (tỉ lệ giao đúng hạn).
///
/// Hệ thống không tự cho xe chạy được, nên "chặt về thời gian" = biết trước MỐC XUẤT PHÁT
/// MUỘN NHẤT và nhắc khi tới mốc:
///   Xuất phát muộn nhất = Hạn giao − (số ngày các chặng CÒN LẠI của xe đó − 1).
///   Số ngày = tổng thời lượng ước tính / 8 giờ lái mỗi ngày (làm tròn lên), chặng chưa có
///   ước tính tính 1 ngày — CÙNG công thức với Đổi xe / Bắt đầu chuyến.
/// VD hạn giao 18/10, chặng Lấy + Giao cần 4 ngày -> phải xuất phát chậm nhất 15/10.
/// Mọi ngày so theo LỊCH VIỆT NAM (hạn giao lưu giờ VN; StartTime/EndTime lưu UTC).
/// Chặng Trả hàng không tính hạn giao (đơn đã hủy, trả lại khách gửi).</summary>
public static class DeliveryDeadlineRules
{
    public const int DrivingMinutesPerDay = 480;

    /// <summary>None = không áp dụng (đơn chưa có hạn / chặng trả hàng / đã hủy).</summary>
    public enum Level { None, Ok, Warning, Late, DoneOnTime, DoneLate }

    public record Info(DateTime? Deadline, DateTime? LatestStart, Level Level, string Text);

    public static DateTime UtcToVnDate(DateTime utc) => utc.AddHours(7).Date;

    /// <summary>Số ngày chạy cho 1 nhóm chặng (cùng công thức Đổi xe / Bắt đầu chuyến).</summary>
    public static int DaysFor(IEnumerable<Trip> legs)
    {
        var list = legs.ToList();
        var minutes = list.Sum(t => t.EstimatedDurationMinutes ?? 0);
        var noEstimate = list.Count(t => t.EstimatedDurationMinutes is null);
        return Math.Max(1, (int)Math.Ceiling(minutes / (double)DrivingMinutesPerDay) + noEstimate);
    }

    /// <summary>Các chặng CÒN PHẢI CHẠY của xe này cho đơn này, tính từ chặng <paramref name="trip"/>:
    /// chặng Lấy hàng thì gồm luôn chặng Giao cùng xe (chưa xong); chặng Giao chỉ chính nó.</summary>
    public static List<Trip> RemainingChain(Trip trip, IEnumerable<Trip> orderTrips)
    {
        var chain = new List<Trip> { trip };
        if (trip.LegType == TripLegType.Pickup)
            chain.AddRange(orderTrips.Where(o => o.Id != trip.Id && o.LegType == TripLegType.Delivery
                                                 && o.VehicleId == trip.VehicleId
                                                 && o.Status != TripStatus.Completed && o.Status != TripStatus.Cancelled));
        return chain;
    }

    /// <summary>Tình trạng hạn của 1 chuyến. <paramref name="deadlineVn"/> = hạn giao của đơn (giờ VN).</summary>
    public static Info Evaluate(Trip trip, IEnumerable<Trip> orderTrips, DateTime? deadlineVn, DateTime todayVn)
    {
        if (deadlineVn is null || trip.LegType == TripLegType.Return || trip.Status == TripStatus.Cancelled)
            return new Info(deadlineVn?.Date, null, Level.None, "—");
        var deadline = deadlineVn.Value.Date;

        if (trip.Status == TripStatus.Completed)
        {
            if (trip.LegType != TripLegType.Delivery || trip.EndTime is null)
                return new Info(deadline, null, Level.None, "Đã xong chặng lấy hàng");
            var doneDay = UtcToVnDate(trip.EndTime.Value);
            var lateDays = (doneDay - deadline).Days;
            return lateDays <= 0
                ? new Info(deadline, null, Level.DoneOnTime, "✅ Giao đúng hạn")
                : new Info(deadline, null, Level.DoneLate, $"⛔ Giao trễ {lateDays} ngày");
        }

        var days = DaysFor(RemainingChain(trip, orderTrips));
        var latestStart = deadline.AddDays(-(days - 1));

        if (trip.Status is TripStatus.InProgress or TripStatus.PendingConfirmation)
        {
            if (trip.Status == TripStatus.PendingConfirmation && trip.LegType == TripLegType.Delivery)
                return todayVn <= deadline
                    ? new Info(deadline, null, Level.Ok, "🟢 Đã giao, chờ duyệt — kịp hạn")
                    : new Info(deadline, null, Level.Late, "🔴 Chờ duyệt — đã quá hạn giao");
            var startDay = trip.StartTime is { } st ? UtcToVnDate(st) : todayVn;
            var expectedDone = startDay.AddDays(days - 1);
            if (todayVn > deadline)
                return new Info(deadline, null, Level.Late, $"🔴 Đang chạy — đã QUÁ hạn giao {deadline:dd/MM}");
            return expectedDone > deadline
                ? new Info(deadline, null, Level.Late, $"🔴 Đang chạy — dự kiến xong ~{expectedDone:dd/MM}, trễ hạn")
                : new Info(deadline, null, Level.Ok, $"🟢 Đang chạy — dự kiến xong ~{expectedDone:dd/MM}");
        }

        // Chưa chạy (Đã lên kế hoạch)
        if (todayVn > deadline)
            return new Info(deadline, latestStart, Level.Late, $"🔴 Đã QUÁ hạn giao {deadline:dd/MM}");
        if (todayVn > latestStart)
            return new Info(deadline, latestStart, Level.Late, $"🔴 Không còn kịp hạn (phải xuất phát trước hết {latestStart:dd/MM})");
        if (todayVn == latestStart)
            return new Info(deadline, latestStart, Level.Warning, "🟡 Phải xuất phát HÔM NAY");
        if (todayVn == latestStart.AddDays(-1))
            return new Info(deadline, latestStart, Level.Warning, $"🟡 Xuất phát muộn nhất ngày mai {latestStart:dd/MM}");
        return new Info(deadline, latestStart, Level.Ok, $"🟢 Kịp — xuất phát trước hết {latestStart:dd/MM}");
    }

    /// <summary>Đơn ĐÃ HOÀN TẤT có giao đúng hạn không: ngày xong chặng Giao CUỐI CÙNG
    /// (giờ VN) ≤ hạn giao. null = không xét được (không có hạn / không có chặng giao xong).</summary>
    public static bool? IsOrderOnTime(DateTime? deadlineVn, IEnumerable<Trip> orderTrips)
    {
        if (deadlineVn is null) return null;
        var lastEnd = orderTrips
            .Where(t => t.LegType == TripLegType.Delivery && t.Status == TripStatus.Completed && t.EndTime != null)
            .Select(t => (DateTime?)t.EndTime!.Value)
            .Max();
        if (lastEnd is null) return null;
        return UtcToVnDate(lastEnd.Value) <= deadlineVn.Value.Date;
    }

    /// <summary>Ngày giao SỚM NHẤT nếu chia xe hôm nay (dùng khi Chia xe).</summary>
    public static DateTime EarliestDelivery(DateTime todayVn, int estimatedDays) => todayVn.AddDays(Math.Max(1, estimatedDays) - 1);
}
