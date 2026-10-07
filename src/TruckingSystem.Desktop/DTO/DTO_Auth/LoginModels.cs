namespace TruckingSystem.Desktop.DTO_Auth
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
