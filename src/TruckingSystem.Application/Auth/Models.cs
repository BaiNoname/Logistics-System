namespace TruckingSystem.Application.Auth;

public record LoginRequest(string Username, string Password);
public record LoginResult(bool Success, string? Token, string? Message);
