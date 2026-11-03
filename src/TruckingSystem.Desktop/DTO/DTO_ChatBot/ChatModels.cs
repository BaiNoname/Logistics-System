namespace TruckingSystem.Desktop.DTO_ChatBot;
public record ChatOption(string Id, string Label) { public override string ToString() => Label; }
public record ChatResponse(string Message, string Context, List<ChatOption> Options, Quote? Quote);
public record Quote(string PriceList, DateTime EffectiveDate, DateTime QuotedAtUtc, string VehicleType, List<QuoteLeg> Legs, decimal Total, string Notice);
public record QuoteLeg(string From, string To, decimal DistanceKm, decimal WeightKg, LegPrice Price);
public record LegPrice(decimal DistanceCharge, decimal WeightCharge, decimal FragileCharge, decimal HazmatCharge, decimal Total);
