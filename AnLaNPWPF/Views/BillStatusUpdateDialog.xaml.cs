using System;
using System.Windows;
using System.Windows.Controls;
using BusinessObject;
using Repository;
using AnLaNPWPF.Helpers; // ✅ Add EventManager

namespace AnLaNPWPF.Views
{
    public partial class BillStatusUpdateDialog : Window
    {
        private MonthlyBill _originalBill;
        private RentalContractRepository _contractRepository;
        private MonthlyBillRepository _billRepository;
        
        public MonthlyBill Bill { get; private set; }
        public bool IsUpdated { get; private set; } = false;

        public BillStatusUpdateDialog(MonthlyBill bill)
        {
            InitializeComponent();
            
            _contractRepository = new RentalContractRepository();
            _billRepository = new MonthlyBillRepository();
            _originalBill = bill;
            Bill = new MonthlyBill
            {
                BillId = bill.BillId,
                ContractId = bill.ContractId,
                BillMonth = bill.BillMonth,
                BillYear = bill.BillYear,
                RoomRent = bill.RoomRent,
                ElectricityUsage = bill.ElectricityUsage,
                ElectricityRate = bill.ElectricityRate,
                ElectricityCost = bill.ElectricityCost,
                WaterUsage = bill.WaterUsage,
                WaterRate = bill.WaterRate,
                WaterCost = bill.WaterCost,
                OtherCharges = bill.OtherCharges,
                TotalAmount = bill.TotalAmount,
                BillStatus = bill.BillStatus,
                DueDate = bill.DueDate,
                PaidDate = bill.PaidDate,
                PaidAmount = bill.PaidAmount,
                CreatedAt = bill.CreatedAt,
                UpdatedAt = bill.UpdatedAt,
                Contract = bill.Contract
            };
            
            LoadBillInfo();
        }

        private void LoadBillInfo()
        {
            try
            {
                // Load contract information if not loaded
                if (Bill.Contract == null && Bill.ContractId > 0)
                {
                    try
                    {
                        Bill.Contract = _contractRepository.GetById(Bill.ContractId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not load contract {Bill.ContractId}: {ex.Message}");
                    }
                }
                
                // Display bill information with fallbacks
                txtBillId.Text = Bill.BillId.ToString();
                txtRoomNumber.Text = Bill.Contract?.RoomNumber ?? $"ContractId: {Bill.ContractId}";
                txtCustomerName.Text = Bill.Contract?.CustomerName ?? "Chưa tải thông tin KH";
                txtBillPeriod.Text = Bill.BillPeriod ?? $"{Bill.BillMonth:D2}/{Bill.BillYear}";
                txtTotalAmount.Text = Bill.FormattedTotal ?? Bill.TotalAmount.ToString("N0") + " VND";
                txtDueDate.Text = Bill.FormattedDueDate ?? Bill.DueDate.ToString("dd/MM/yyyy");
                txtCurrentStatus.Text = Bill.StatusText ?? Bill.BillStatus.ToString();
                
                // Set current status in combobox
                foreach (ComboBoxItem item in cmbNewStatus.Items)
                {
                    if (item.Tag.ToString() == Bill.BillStatus.ToString())
                    {
                        cmbNewStatus.SelectedItem = item;
                        break;
                    }
                }
                
                // Set paid date and amount if already paid
                if (Bill.PaidDate.HasValue)
                {
                    dpPaidDate.SelectedDate = Bill.PaidDate.Value;
                }
                else
                {
                    dpPaidDate.SelectedDate = DateTime.Today;
                }
                
                if (Bill.PaidAmount.HasValue)
                {
                    txtPaidAmount.Text = Bill.PaidAmount.Value.ToString("N0");
                }
                else
                {
                    txtPaidAmount.Text = Bill.TotalAmount.ToString("N0");
                }
                
                // Initial visibility based on current status
                NewStatus_Changed(cmbNewStatus, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewStatus_Changed(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = cmbNewStatus.SelectedItem as ComboBoxItem;
            var newStatus = selectedItem?.Tag?.ToString();
            
            // Show/hide paid details based on status
            if (newStatus == "Paid" || newStatus == "Partial")
            {
                pnlPaidDetails.Visibility = Visibility.Visible;
            }
            else
            {
                pnlPaidDetails.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItem = cmbNewStatus.SelectedItem as ComboBoxItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn trạng thái mới!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newStatusString = selectedItem.Tag.ToString();
                if (!Enum.TryParse<BillStatus>(newStatusString, out var newStatus))
                {
                    MessageBox.Show("Trạng thái không hợp lệ!", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validate paid details if status is Paid or Partial
                if (newStatus == BillStatus.Paid || newStatus == BillStatus.Partial)
                {
                    if (!dpPaidDate.SelectedDate.HasValue)
                    {
                        MessageBox.Show("Vui lòng chọn ngày thanh toán!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!decimal.TryParse(txtPaidAmount.Text.Replace(",", ""), out var paidAmount) || paidAmount <= 0)
                    {
                        MessageBox.Show("Vui lòng nhập số tiền thanh toán hợp lệ!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (newStatus == BillStatus.Paid && paidAmount != Bill.TotalAmount)
                    {
                        var result = MessageBox.Show(
                            $"Số tiền thanh toán ({paidAmount:N0}) khác với tổng hóa đơn ({Bill.TotalAmount:N0}).\n" +
                            "Bạn có muốn đặt trạng thái là 'Thanh toán một phần' thay vì 'Đã thanh toán'?",
                            "Xác nhận", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            newStatus = BillStatus.Partial;
                        }
                        else if (result == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                    }

                    Bill.PaidDate = dpPaidDate.SelectedDate.Value;
                    Bill.PaidAmount = paidAmount;
                }
                else
                {
                    // Clear paid info if status is not Paid or Partial
                    Bill.PaidDate = null;
                    Bill.PaidAmount = null;
                }

                Bill.BillStatus = newStatus;
                Bill.UpdatedAt = DateTime.Now;

                // ✅ Trigger events for other views to refresh
                GlobalEventBus.OnBillStatusChanged(Bill.BillId);

                IsUpdated = true;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật trạng thái: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
