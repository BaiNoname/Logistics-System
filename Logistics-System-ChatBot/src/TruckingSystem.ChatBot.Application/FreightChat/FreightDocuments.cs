using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.FreightChat;

public static class FreightDocuments
{
    public static TariffDocument Create(TariffRule rule, PriceList list, bool includeFixedFees = true)
    {
        var text = string.Create(CultureInfo.InvariantCulture,
            $"Bảng giá {list.Name}; hiệu lực {list.EffectiveDate:yyyy-MM-dd}; trạng thái {list.Status}; loại xe {rule.VehicleType}; đồng/km {rule.PricePerKm}; đồng/tấn {rule.PricePerTon}");
        if (includeFixedFees)
            text += string.Create(CultureInfo.InvariantCulture, $"; dễ vỡ {rule.SurchargeFragile}; nguy hiểm {rule.SurchargeHazmat}");
        text += ".";
        return new(rule.Id, list.Id, text, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))));
    }
}
