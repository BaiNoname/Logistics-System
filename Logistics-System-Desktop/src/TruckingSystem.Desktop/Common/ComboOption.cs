namespace TruckingSystem.Desktop.Common
{
    /// <summary>1 lựa chọn trong ComboBox: HIỂN THỊ tiếng Việt (Text) nhưng GỬI lên
    /// server đúng giá trị enum (Value) — tránh hiện tên kỹ thuật như "Toll", "Accident".</summary>
    public sealed record ComboOption(string Value, string Text)
    {
        public override string ToString() => Text;
    }
}
