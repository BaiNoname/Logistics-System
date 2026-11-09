using System.ComponentModel;
using System.Globalization;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Ô nhập số dùng thay NumericUpDown cho các form phần vận hành.
    /// - Ô SỐ LƯỢNG / KG / M³ / MÉT: hiện đúng số đã nhập, bỏ số 0 thừa sau dấu thập phân
    ///   (nhập 500 hiện "500", nhập 1,5 hiện "1,5") — NumericUpDown thường hiện "500,00" / "500.000"
    ///   theo DecimalPlaces, dễ đọc nhầm thành năm trăm nghìn. DecimalPlaces vẫn là SỐ CHỮ SỐ LẺ TỐI ĐA.
    /// - Ô TIỀN (IsMoney = true): GÕ TẮT NGHÌN — gõ 500 (dưới 1.000) tự thành 500.000 đ; gõ đủ 500000
    ///   vẫn là 500.000 đ. Có phân cách nghìn, bước tăng 1.000, gợi ý "= 500.000 đ" ngay khi đang gõ.</summary>
    public class NumberBox : NumericUpDown
    {
        private readonly ToolTip _tip = new() { InitialDelay = 300 };
        private bool _isMoney;

        /// <summary>Ô tiền (đồng): gõ tắt nghìn, phân cách nghìn, không số lẻ.</summary>
        [DefaultValue(false), Category("Behavior"), Description("Ô tiền: gõ 500 = 500.000 đ.")]
        public bool IsMoney
        {
            get => _isMoney;
            set
            {
                _isMoney = value;
                if (!value) return;
                DecimalPlaces = 0;
                ThousandsSeparator = true;
                Increment = 1000;
                _tip.SetToolTip(this, "Gõ tắt nghìn: 500 = 500.000 đ; 22,5 = 22.500 đ. Gõ đủ số vẫn được.");
            }
        }

        /// <summary>Số đã gõ tắt -> số đồng thật (dưới 1.000 thì nhân 1.000). null = không phải gõ tắt.</summary>
        private decimal? ShortcutValue()
        {
            if (!decimal.TryParse(Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var typed)) return null;
            return typed > 0 && typed < 1000 ? Math.Round(typed * 1000m, 0) : null;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (!_isMoney || !UserEdit || !Focused) return;
            if (ShortcutValue() is decimal real)
                _tip.Show($"= {real.ToString("#,##0", CultureInfo.CurrentCulture)} đ  (gõ tắt nghìn)", this, 0, Height + 2, 2000);
            else
                _tip.Hide(this);
        }

        protected override void UpdateEditText()
        {
            if (UserEdit && _isMoney && ShortcutValue() is decimal real)
                Text = real.ToString(CultureInfo.CurrentCulture); // ParseEditText bên dưới đọc số đã đổi
            if (UserEdit) ParseEditText();
            ChangingText = true;
            Text = Format(Value);
        }

        private string Format(decimal value)
        {
            var places = Math.Max(0, DecimalPlaces);
            var pattern = (ThousandsSeparator ? "#,##0" : "0") + (places > 0 ? "." + new string('#', places) : "");
            return Math.Round(value, places).ToString(pattern, CultureInfo.CurrentCulture);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tip.Dispose();
            base.Dispose(disposing);
        }
    }
}
