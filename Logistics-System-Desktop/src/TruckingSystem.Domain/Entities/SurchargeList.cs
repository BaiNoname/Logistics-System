namespace TruckingSystem.Domain.Entities;

public enum SurchargeKind { Fragile = 1, Hazmat = 2 }

public sealed class SurchargeLine
{
    public int SurchargeListId { get; set; }
    public SurchargeKind Kind { get; set; }
    public decimal Percent { get; set; }
}

public sealed class SurchargeList
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateOnly EffectiveDate { get; set; }
    public bool Published { get; set; }
    public int Revision { get; set; }
    public List<SurchargeLine> Lines { get; set; } = [];

    public SurchargePolicy Rates() => new()
    {
        EffectiveDate = EffectiveDate,
        FragilePercent = Lines.Single(l => l.Kind == SurchargeKind.Fragile).Percent,
        HazmatPercent = Lines.Single(l => l.Kind == SurchargeKind.Hazmat).Percent
    };

    public void SetDraft(string name, DateOnly date, decimal fragile, decimal hazmat)
    {
        if (Published) throw new InvalidOperationException("Bảng đã công bố, không thể sửa.");
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 150 || date == default)
            throw new ArgumentException("Nhập tên bảng tối đa 150 ký tự và ngày hiệu lực.");
        foreach (var value in new[] { fragile, hazmat })
            if (value < 0 || value > 100 || decimal.Round(value, 2) != value)
                throw new ArgumentException("Tỷ lệ phải từ 0 đến 100%, tối đa hai chữ số thập phân.");
        Name = name.Trim();
        EffectiveDate = date;
        if (Lines.Count == 0)
            Lines = [new() { Kind = SurchargeKind.Fragile }, new() { Kind = SurchargeKind.Hazmat }];
        Lines.Single(l => l.Kind == SurchargeKind.Fragile).Percent = fragile;
        Lines.Single(l => l.Kind == SurchargeKind.Hazmat).Percent = hazmat;
        Revision++;
    }

    public void Publish(DateOnly today)
    {
        if (Published) throw new InvalidOperationException("Bảng đã công bố.");
        Rates().Validate(today);
        Published = true;
        Revision++;
    }
}
