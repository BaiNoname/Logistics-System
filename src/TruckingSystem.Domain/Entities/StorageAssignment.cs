using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>"Địa chỉ nhà" của từng lô hàng trong kho — ghi nhận lô hàng nào (từ GoodsReceipt nào) đang nằm ở StorageBin nào.</summary>
public class StorageAssignment : BaseEntity
{
    public int GoodsReceiptId { get; set; }
    public GoodsReceipt GoodsReceipt { get; set; } = null!;

    public int StorageBinId { get; set; }
    public StorageBin StorageBin { get; set; } = null!;

    public decimal QtyStored { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
