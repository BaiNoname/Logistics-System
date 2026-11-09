using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public class TripExpense : BaseEntity
{
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public TripExpenseType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}
