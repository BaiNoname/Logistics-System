using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty; // FK -> AspNetUsers.Id
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}
