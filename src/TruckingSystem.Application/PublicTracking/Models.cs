namespace TruckingSystem.Application.PublicTracking;

public record PublicTrackingResponse(
    string OrderCode, string Status, decimal ProgressPercent,
    string PickupPoint, string DropoffPoint,
    double? LastLat, double? LastLng, DateTime? LastUpdatedAt,
    string CurrentStage
);