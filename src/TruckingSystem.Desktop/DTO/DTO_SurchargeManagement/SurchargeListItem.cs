namespace TruckingSystem.Desktop.DTO_SurchargeManagement;

public record SurchargeListItem(int Id, string Name, DateTime EffectiveDate, bool Published,
    int Revision, decimal FragilePercent, decimal HazmatPercent)
{
    public string Status => Published ? "Đã xác nhận" : "Bản nháp";
}
