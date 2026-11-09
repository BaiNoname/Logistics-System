using TruckingSystem.Domain.Common;
using System.Text.RegularExpressions;

namespace TruckingSystem.Domain.Entities;

public class CustomerContact : BaseEntity
{
    public static bool IsValidPhoneNumber(string? phone)
        => !string.IsNullOrWhiteSpace(phone)
            && Regex.IsMatch(phone, @"^(?:0[35789][0-9]{8}|\+[1-9][0-9]{0,2} [0-9]{4,14})$");

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Position { get; set; }
}
