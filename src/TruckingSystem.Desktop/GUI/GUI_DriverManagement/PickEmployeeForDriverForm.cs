using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_EmployeeManagement;

namespace TruckingSystem.Desktop.GUI_DriverManagement
{
    /// <summary>Dialog nhỏ: chọn 1 nhân viên CHƯA phải tài xế, trước khi mở
    /// CreateDriverProfileForm cho đúng người. Chỉ chọn — không tự gọi API
    /// gì ở đây, màn gọi nó (DriversControl) lo phần tiếp theo.</summary>
    public partial class PickEmployeeForDriverForm : BaseForm
    {
        public int? SelectedEmployeeId { get; private set; }

        public PickEmployeeForDriverForm()
        {
            InitializeComponent();
        }

        public PickEmployeeForDriverForm(List<EmployeeListItem> candidates) : this()
        {
            // "Mã — Họ tên (trạng thái)" — phân biệt được 2 người trùng tên; người Đang nghỉ phép
            // vẫn chọn được nhưng thấy rõ. Nhân viên đã nghỉ việc không có trong danh sách.
            cboEmployee.DataSource = candidates
                .Where(c => c.Status != "Terminated")
                .OrderBy(c => c.FullName)
                .Select(c => new ComboOption(c.Id.ToString(), $"{c.Code} — {c.FullName}{StatusSuffix(c.Status)}"))
                .ToList();
            cboEmployee.DisplayMember = nameof(ComboOption.Text);
            cboEmployee.ValueMember = nameof(ComboOption.Value);
        }

        private static string StatusSuffix(string status) => status switch
        {
            "Active" => "",
            "OnLeave" => " (đang nghỉ phép)",
            _ => $" ({status})",
        };

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            if (cboEmployee.SelectedItem is not ComboOption option || !int.TryParse(option.Value, out var employeeId))
            {
                lblStatus.Text = "Vui lòng chọn 1 nhân viên.";
                return;
            }
            SelectedEmployeeId = employeeId;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}