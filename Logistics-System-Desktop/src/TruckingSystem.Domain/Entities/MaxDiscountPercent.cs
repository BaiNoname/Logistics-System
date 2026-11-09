namespace TruckingSystem.Domain.Entities;

public class MaxDiscountPercent
{
    public int Id { get; set; } = 1;
    public int Value { get; set; } = 20;
    public DateTime UpdatedAt { get; set; }

    public static void Validate(int value)
    {
        if (value is < 0 or > 100)
            throw new ArgumentException("Phần trăm ưu đãi tối đa phải từ 0 đến 100.");
    }
}
