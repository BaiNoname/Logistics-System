namespace TruckingSystem.Desktop.DTO_SurchargeManagement;

public record SurchargeListItem(int Id, string Name, DateTime EffectiveDate, bool Published,
    int Revision, decimal FragilePercent, decimal HazmatPercent, bool IsActive = false)
{
    public string Status => IsActive ? "Đang áp dụng" : "Ngưng áp dụng";
}
