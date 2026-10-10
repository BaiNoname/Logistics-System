namespace TruckingSystem.Desktop.DTO_ReportManagement
{
    public class ProfitReport
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal Revenue { get; set; }
        public decimal Collected { get; set; }
        public decimal FuelCost { get; set; }
        public decimal ExpenseCost { get; set; }
        public decimal IncidentCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
        public decimal? MarginPercent { get; set; }
        public int TotalOrders { get; set; }
        public int DoneOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int UninvoicedDoneOrders { get; set; }
        public decimal CancelledOrdersCost { get; set; }
        public List<CostCategoryItem> CostBreakdown { get; set; } = new();
        public List<MonthlyProfitItem> Monthly { get; set; } = new();
        public List<OrderProfitItem> Orders { get; set; } = new();

        // ---- Kiểm soát nhiên liệu ----
        public decimal FuelPlannedCost { get; set; }   // dự toán theo km VietMap
        public decimal FuelNormCost { get; set; }      // định mức theo km GPS thực tế
        public decimal FuelActualCost { get; set; }    // thực đổ (hóa đơn)
        public int TripsUsingFuelEstimate { get; set; }
        /// <summary>Dự toán / định mức chỉ trên CÙNG 1 tập chuyến (hoàn tất, có đủ cả 2 số)
        /// — dùng để tính "Lệch dự toán" cho đúng.</summary>
        public decimal FuelPlannedCostCompared { get; set; }
        public decimal FuelNormCostCompared { get; set; }
        public int FuelComparedTripCount { get; set; }
        public List<FuelControlRowItem>? FuelControl { get; set; }
    }

    /// <summary>1 chuyến đã chạy trong bảng "Kiểm soát nhiên liệu".</summary>
    public class FuelControlRowItem
    {
        public int TripId { get; set; }
        public string OrderCode { get; set; } = "";
        public string LegType { get; set; } = "";
        public string PlateNumber { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime TripDate { get; set; }
        public decimal? PlannedKm { get; set; }
        public decimal? GpsKm { get; set; }
        public decimal? KmDeviationPct { get; set; }
        public decimal? ConsumptionPer100Km { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? PlannedCost { get; set; }
        public decimal? NormCost { get; set; }
        public decimal? NormLiters { get; set; }
        public decimal ActualCost { get; set; }
        public decimal ActualLiters { get; set; }
        public decimal? CostDeviationPct { get; set; }
        public bool HasActual { get; set; }
        public bool Flagged { get; set; }
        public string? Note { get; set; }
    }

    public class CostCategoryItem
    {
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
    }

    public class MonthlyProfitItem
    {
        public string Month { get; set; } = ""; // "yyyy-MM"
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
    }

    public class OrderProfitItem
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool HasInvoice { get; set; }
        public decimal Revenue { get; set; }
        public decimal FuelCost { get; set; }
        public decimal ExpenseCost { get; set; }
        public decimal IncidentCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
    }

    public class OrderCostDetail
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Status { get; set; } = "";
        public decimal Revenue { get; set; }
        public decimal TotalCost { get; set; }
        public List<CostLineItem> Lines { get; set; } = new();
    }

    public class CostLineItem
    {
        public int TripId { get; set; }
        public string LegType { get; set; } = "";
        public string TripStatus { get; set; } = "";
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}
