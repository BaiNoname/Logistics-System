namespace TruckingSystem.Application.Locations;

public record LocationDto(int Id, string Name, string Address, double? Lat, double? Lng, int? CustomerId);
public record CreateLocationRequest(string Name, string Address, double? Lat, double? Lng, int? CustomerId);
