namespace TruckingSystem.Application.Resources;

public interface IResourceOverviewService
{
    /// <summary>Xe + tài xế của kho <paramref name="warehouseId"/> (null = mọi kho); xe / tài xế
    /// chưa gán kho cố định (dùng cho mọi kho) luôn được kể vào.</summary>
    Task<ResourceOverviewDto> GetAsync(int? warehouseId);
}
