namespace TruckingSystem.Application.PublicTracking;

public interface IPublicTrackingService
{
    Task<PublicTrackingResponse?> TrackAsync(string token);
}
