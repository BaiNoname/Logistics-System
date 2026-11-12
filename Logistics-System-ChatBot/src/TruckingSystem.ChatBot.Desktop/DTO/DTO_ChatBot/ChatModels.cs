namespace TruckingSystem.Desktop.DTO_ChatBot;
public record ChatOption(string Id, string Label) { public override string ToString() => Label; }
public record ChatResponse(string Message, string Context, List<ChatOption> Options, Quote? Quote, List<ChatTable>? Tables);
/// <summary>Bảng server dựng sẵn (bảng giá, công thức, hướng dẫn, tóm tắt); NumericColumns là cột canh phải.</summary>
public record ChatTable(string Kind, string Title, string? Subtitle, List<string> Columns, List<int> NumericColumns,
    List<List<string>> Rows, string? Note);
/// <summary>Báo giá một lô: PickupMethod = "company" (kho cử xe tới lấy) | "self" (tự mang đến kho).</summary>
public record Quote(string PriceList, DateTime EffectiveDate, DateTime QuotedAtUtc, string VehicleType, string PickupMethod,
    string Warehouse, List<QuoteLeg> Legs, List<QuoteLine> Lines, decimal Total, string Notice);
public record QuoteLeg(string Name, string From, string To, decimal DistanceKm);
public record QuoteLine(string Name, string Detail, decimal Amount);
