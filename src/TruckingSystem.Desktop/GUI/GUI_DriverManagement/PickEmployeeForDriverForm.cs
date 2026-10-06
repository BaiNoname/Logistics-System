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
            cboEmployee.DataSource = candidates;
            cboEmployee.DisplayMember = "FullName";
            cboEmployee.ValueMember = "Id";
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            if (cboEmployee.SelectedValue is not int employeeId)
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