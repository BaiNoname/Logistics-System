namespace TruckingSystem.Desktop.DTO_CustomerManagement
{
    public record CustomerContactInput(string Name, string Phone, string? Position);

    public class CustomerListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Phone { get; set; }
        public string CustomerType { get; set; } = "";
        public string? TaxCode { get; set; }
        public string? Address { get; set; }
        public decimal? CreditLimit { get; set; }
        public string? GoogleCalendarId { get; set; }
    }

    public class CustomerContactListItem
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Phone { get; set; }
        public string? Position { get; set; }
    }

    public class CustomerActionResultItem
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class CustomerBusinessCardInfoItem
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? TaxCode { get; set; }
        public string? CustomerType { get; set; }
        public string? Address { get; set; }
        public string? FullName { get; set; }
        public string? Company { get; set; }
        public string? Title { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? RawResponse { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
