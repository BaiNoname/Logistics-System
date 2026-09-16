namespace TruckingSystem.Application.Trips;

public record GpsPingRequest(double Lat, double Lng, decimal? Accuracy, decimal? SpeedKmh, DateTime Timestamp);
public record ConfirmArrivalRequest(string? Note);
public record ServiceResult(bool Success, string? Message = null);
