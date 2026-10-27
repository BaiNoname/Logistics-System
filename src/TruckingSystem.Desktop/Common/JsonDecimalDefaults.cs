using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Số thập phân nhận từ API mang theo số chữ số lẻ của cột trong DB (numeric(18,4) -> "25.0000",
    /// numeric(18,2) -> "8000.00"), hiện thẳng lên lưới / tooltip thành "10/25.0000 m³", "8000.00 kg" — dễ đọc
    /// nhầm thành hàng nghìn. Bỏ các số 0 thừa NGAY LÚC ĐỌC JSON (giá trị không đổi: 25.0000 = 25), áp dụng cho
    /// mọi màn hình mà không phải sửa từng chỗ hiển thị. Chạy tự động khi nạp ứng dụng (ModuleInitializer).</summary>
    internal static class JsonDecimalDefaults
    {
        [ModuleInitializer]
        internal static void Init()
        {
            var previous = JsonConvert.DefaultSettings;
            JsonConvert.DefaultSettings = () =>
            {
                var settings = previous?.Invoke() ?? new JsonSerializerSettings();
                settings.FloatParseHandling = FloatParseHandling.Decimal; // không đi qua double (mất chính xác tiền)
                if (!settings.Converters.OfType<TrimDecimalConverter>().Any())
                    settings.Converters.Add(new TrimDecimalConverter());
                return settings;
            };
        }
    }

    /// <summary>Đọc decimal / decimal? và bỏ số 0 thừa sau dấu thập phân (chỉ đọc, ghi giữ nguyên).</summary>
    internal sealed class TrimDecimalConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(decimal) || objectType == typeof(decimal?);
        public override bool CanWrite => false;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return objectType == typeof(decimal?) ? null : 0m;
            var value = reader.TokenType == JsonToken.String
                ? decimal.Parse((string)reader.Value!, System.Globalization.CultureInfo.InvariantCulture)
                : Convert.ToDecimal(reader.Value, System.Globalization.CultureInfo.InvariantCulture);
            return Trim(value);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotSupportedException();

        /// <summary>25.0000m -> 25m; 1.5000m -> 1.5m (chia cho 1.000…0 làm C# bỏ phần scale thừa).</summary>
        internal static decimal Trim(decimal value) => value / 1.000000000000000000000000000000000m;
    }
}
