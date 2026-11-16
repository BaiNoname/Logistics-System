using System.Globalization;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.FreightChat;

/// <summary>Dựng các bảng hiển thị trong khung chat và nhận biết câu hỏi muốn xem bảng (từ khóa không dấu).</summary>
public static class FreightTables
{
    public const int MaxRows = 30;
    public const string PriceListKind = "priceList", FormulaKind = "formula", GuideKind = "guide", SummaryKind = "summary";
    private const string Excludes = "Giá chưa gồm VAT, cầu đường, bốc xếp.";
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    private static readonly (string Kind, string[] Phrases)[] Intents =
    [
        (PriceListKind, ["bang gia", "bang cuoc", "bieu gia", "gia cac loai xe", "gia tung loai xe", "gia cac xe", "cac loai xe",
            "xe nao re", "xe nao gia re", "xem gia"]),
        (FormulaKind, ["cong thuc", "cach tinh", "tinh nhu the nao", "tinh the nao", "tinh ra sao", "tinh sao"]),
        (GuideKind, ["huong dan", "cach dung", "cach su dung", "su dung nhu the nao", "dung nhu the nao", "dung the nao", "lam sao de"])
    ];

    /// <summary>Loại bảng khách muốn xem trong tin nhắn này (priceList, formula, guide), theo thứ tự cố định.</summary>
    public static List<string> DetectAsks(string? message)
    {
        var text = FreightText.Fold(message);
        return Intents.Where(i => i.Phrases.Any(p => FreightText.Has(text, p))).Select(i => i.Kind).ToList();
    }

    public static ChatTable PriceList(PriceList list, IReadOnlyList<TariffRule> rules, SurchargePolicy? policy)
    {
        var fees = policy is null;
        var rows = rules.OrderBy(r => r.Id).Take(MaxRows).Select(r => fees
            ? new List<string> { r.VehicleType, Money(r.PricePerKm), Money(r.PricePerTon), Money(r.SurchargeFragile), Money(r.SurchargeHazmat) }
            : [r.VehicleType, Money(r.PricePerKm), Money(r.PricePerTon)]).ToList();
        return new(PriceListKind, $"Bảng giá {list.Name}",
            string.Format(Vi, "Hiệu lực từ {0:dd/MM/yyyy} · {1} loại xe", list.EffectiveDate, rows.Count),
            fees ? ["Loại xe", "Giá/km", "Giá/tấn", "Dễ vỡ", "Nguy hiểm"] : ["Loại xe", "Giá/km", "Giá/tấn"],
            fees ? [1, 2, 3, 4] : [1, 2], rows,
            fees ? "Đơn vị: đồng. Phụ phí dễ vỡ / nguy hiểm cố định theo loại xe, tính một lần. " + Excludes
                 : $"Đơn vị: đồng. Phụ phí trên (cước km + cước tấn): dễ vỡ {Percent(policy!.FragilePercent)}, nguy hiểm {Percent(policy.HazmatPercent)}. " + Excludes);
    }

    public static ChatTable Formula(SurchargePolicy? policy)
    {
        const string byVehicle = "Theo loại xe — xem Bảng giá";
        var feeRule = policy is null ? "Phí cố định theo loại xe, tính một lần cho cả lô" : "Phần trăm × (cước km + cước tấn), tính một lần";
        return new(FormulaKind, "Công thức tính cước", "Cước tham khảo theo bảng giá đang áp dụng",
            ["Khoản", "Cách tính", "Đơn giá áp dụng"], [],
            [
                ["Chặng lấy hàng", "Chỉ khi kho cử xe tới lấy: km từ nơi lấy hàng đến kho gần nhất", "Cộng vào tổng km"],
                ["Chặng giao hàng", "Km từ kho đến nơi giao", "Cộng vào tổng km"],
                ["Cước theo km", "Tổng km các chặng × giá/km của loại xe", byVehicle],
                ["Cước theo tấn", "Khối lượng (tấn) × giá/tấn, tính một lần cho cả lô", byVehicle],
                ["Phụ phí dễ vỡ", feeRule, policy is null ? byVehicle : Percent(policy.FragilePercent)],
                ["Phụ phí nguy hiểm", feeRule, policy is null ? byVehicle : Percent(policy.HazmatPercent)],
                ["Tổng cước", "Cước km + cước tấn + phụ phí", "Chưa gồm VAT, cầu đường, bốc xếp, ưu đãi hợp đồng"]
            ],
            "Km tính theo tuyến đường thực tế sau khi bạn xác nhận địa điểm.");
    }

    public static ChatTable Guide() => new(GuideKind, "Hướng dẫn sử dụng", "Trả lời từng câu hỏi hoặc gõ một câu đủ ý",
        ["Bước", "Bạn làm gì"], [0],
        [
            ["1", "Cho biết hàng gì và nặng bao nhiêu kg hoặc tấn"],
            ["2", "Cho biết nơi lấy hàng và nơi giao hàng"],
            ["3", "Chọn kho cử xe tới lấy hàng, hoặc tự mang hàng đến kho"],
            ["4", "Chọn loại xe (mình gợi ý xe vừa tải, bạn có thể đổi)"],
            ["5", "Bấm chọn đúng địa chỉ trong các gợi ý"],
            ["6", "Kiểm tra bảng tóm tắt, bấm Xác nhận hoặc gõ phần cần sửa"],
            ["7", "Xem báo giá; gõ yêu cầu mới hoặc bấm Cuộc trò chuyện mới để tra lô khác"]
        ],
        "Ví dụ: “Gửi 2 tấn gạo từ Thủ Đức đến Biên Hòa, kho tới lấy”.");

    public static ChatTable Summary(FreightSummary s)
    {
        var cargoType = (s.IsFragile, s.IsHazmat) switch
        {
            (true, true) => "Dễ vỡ, nguy hiểm",
            (true, false) => "Dễ vỡ",
            (false, true) => "Nguy hiểm",
            _ => "Hàng thường"
        };
        var note = "Kiểm tra lại rồi bấm Xác nhận, hoặc gõ phần cần sửa. Loại hàng do AI nhận định từ mô tả.";
        if (!s.WarehouseSuitable) note += " Kho này chưa có khu phù hợp loại hàng, nhân viên sẽ xác nhận kho khi nhận đơn.";
        return new(SummaryKind, "Thông tin đơn hàng", null, ["Mục", "Nội dung"], [],
            [
                ["Hàng hóa", s.Cargo],
                ["Khối lượng", s.Weight],
                ["Loại hàng", cargoType],
                ["Loại xe", s.Vehicle],
                ["Cách lấy hàng", s.PickupMethod],
                ["Nơi lấy hàng", s.From],
                ["Kho trung chuyển", string.IsNullOrWhiteSpace(s.WarehouseAddress) ? s.WarehouseName : $"{s.WarehouseName} — {s.WarehouseAddress}"],
                ["Nơi giao", s.To]
            ], note);
    }

    private static string Money(decimal value) => value.ToString("N0", Vi);
    private static string Percent(decimal value) => value.ToString("0.##", Vi) + "%";
}
