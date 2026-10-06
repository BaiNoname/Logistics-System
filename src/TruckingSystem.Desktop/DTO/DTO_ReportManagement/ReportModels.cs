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
