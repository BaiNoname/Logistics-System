using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public static class InvoicePricing
{
    public static decimal? DistanceKm(IEnumerable<Waypoint> waypoints)
    {
        var points = waypoints.OrderBy(p => p.Timestamp).ThenBy(p => p.Id).ToList();
        if (points.Count < 2) return null;
        double km = 0;
        foreach (var point in points)
            if (!double.IsFinite(point.Lat) || !double.IsFinite(point.Lng)
                || Math.Abs(point.Lat) > 90 || Math.Abs(point.Lng) > 180)
                throw new InvalidOperationException("Dữ liệu GPS không hợp lệ, cần kiểm tra trước khi lập hóa đơn.");
        for (var i = 1; i < points.Count; i++)
        {
            var first = points[i - 1];
            var second = points[i];
            var dLat = (second.Lat - first.Lat) * Math.PI / 180;
            var dLng = (second.Lng - first.Lng) * Math.PI / 180;
            var a = Math.Pow(Math.Sin(dLat / 2), 2)
                + Math.Cos(first.Lat * Math.PI / 180) * Math.Cos(second.Lat * Math.PI / 180)
                * Math.Pow(Math.Sin(dLng / 2), 2);
            km += 6371 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
        }
        return decimal.Round((decimal)km, 6);
    }

    public static List<InvoiceLine> CalculateLines(TransportOrder order, TariffRule tariff, decimal? distanceKm,
        string source, SurchargePolicy? policy = null, decimal otherCosts = 0, decimal storageFee = 0)
    {
        if (order.Status != TransportOrderStatus.Done)
            throw new InvalidOperationException("Chỉ được lập hóa đơn cho đơn hàng đã hoàn thành.");
        TariffRule.Validate(tariff.VehicleType, tariff.PricePerKm, tariff.PricePerTon,
            tariff.SurchargeFragile, tariff.SurchargeHazmat);
        if (tariff.PricePerKm > 0 && distanceKm is null)
            throw new InvalidOperationException("Chưa đủ GPS cho tất cả chuyến đã chạy (mỗi chuyến cần ít nhất 2 điểm). Không thể tính cước theo km.");
        if (order.CargoLines.Count == 0 || order.CargoLines.Any(c => c.WeightKg <= 0))
            throw new InvalidOperationException("Đơn hàng chưa có khối lượng hàng hợp lệ để tính cước.");

        var lines = new List<InvoiceLine>();
        try
        {
            Add("Cước theo km", distanceKm ?? 0, tariff.PricePerKm, source);
            // CargoLine.WeightKg is the total weight, shared by pickup and delivery legs.
            Add("Cước theo tấn", order.CargoLines.Sum(c => c.WeightKg) / 1000m, tariff.PricePerTon, source);
            var basis = lines.Sum(l => l.Amount);
            if (order.CargoLines.Any(c => c.IsFragile))
                AddFee("Phụ phí hàng dễ vỡ", policy?.FragilePercent, tariff.SurchargeFragile);
            if (order.CargoLines.Any(c => c.IsHazmat))
                AddFee("Phụ phí hàng nguy hiểm", policy?.HazmatPercent, tariff.SurchargeHazmat);
            Add("Chi phí khác của các chuyến xe", 1, otherCosts);
            Add("Phí lưu kho", 1, storageFee);

            void AddFee(string name, decimal? rate, decimal legacy)
            {
                if (rate is decimal percent)
                    Add($"{name} ({percent:0.##}% × {basis:N2} đ)", 1, SurchargePolicy.Charge(basis, percent), source);
                else Add(name, 1, legacy, source);
            }
            _ = Invoice.Create(order, lines.Sum(l => l.Amount));
        }
        catch (OverflowException)
        {
            throw new ArgumentException("Tổng tiền vượt giới hạn hóa đơn. Vui lòng kiểm tra đơn giá và số liệu chuyến.");
        }
        return lines;

        void Add(string description, decimal quantity, decimal price, string? pricingSource = null)
        {
            if (price == 0 || quantity == 0) return;
            lines.Add(new InvoiceLine
            {
                Description = $"{description} — đơn {order.Code}" + (pricingSource is null ? "" : $" — {pricingSource}"),
                Quantity = quantity, UnitPrice = price,
                Amount = decimal.Round(quantity * price, 2, MidpointRounding.AwayFromZero)
            });
        }
    }
}
