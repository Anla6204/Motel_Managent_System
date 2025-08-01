using System;
using System.ComponentModel;

namespace BusinessObject
{
    public class MonthlyBill : INotifyPropertyChanged
    {
        private int _billId;
        private int _contractId;
        private int _billMonth;
        private int _billYear;
        private decimal _roomRent;
        private decimal _electricityUsage;
        private decimal _electricityRate;
        private decimal _electricityCost;
        private decimal _waterUsage;
        private decimal _waterRate;
        private decimal _waterCost;
        private decimal _otherCharges;
        private decimal _totalAmount;
        private BillStatus _billStatus;
        private DateTime _dueDate;
        private DateTime? _paidDate;
        private decimal? _paidAmount;
        private DateTime _createdAt;
        private DateTime _updatedAt;
        
        // Additional properties for display
        private string _customerNameDisplay;
        private string _roomNumberDisplay;
        private string _contractNumberDisplay;

        public int BillId
        {
            get => _billId;
            set { _billId = value; OnPropertyChanged(nameof(BillId)); }
        }

        public int ContractId
        {
            get => _contractId;
            set { _contractId = value; OnPropertyChanged(nameof(ContractId)); }
        }

        public int BillMonth
        {
            get => _billMonth;
            set { _billMonth = value; OnPropertyChanged(nameof(BillMonth)); OnPropertyChanged(nameof(BillPeriod)); }
        }

        public int BillYear
        {
            get => _billYear;
            set { _billYear = value; OnPropertyChanged(nameof(BillYear)); OnPropertyChanged(nameof(BillPeriod)); }
        }

        public decimal RoomRent
        {
            get => _roomRent;
            set { _roomRent = value; OnPropertyChanged(nameof(RoomRent)); RecalculateTotal(); }
        }

        public decimal ElectricityUsage
        {
            get => _electricityUsage;
            set { _electricityUsage = value; OnPropertyChanged(nameof(ElectricityUsage)); RecalculateElectricity(); }
        }

        public decimal ElectricityRate
        {
            get => _electricityRate;
            set { _electricityRate = value; OnPropertyChanged(nameof(ElectricityRate)); RecalculateElectricity(); }
        }

        public decimal ElectricityCost
        {
            get => _electricityCost;
            set { _electricityCost = value; OnPropertyChanged(nameof(ElectricityCost)); RecalculateTotal(); }
        }

        public decimal WaterUsage
        {
            get => _waterUsage;
            set { _waterUsage = value; OnPropertyChanged(nameof(WaterUsage)); RecalculateWater(); }
        }

        public decimal WaterRate
        {
            get => _waterRate;
            set { _waterRate = value; OnPropertyChanged(nameof(WaterRate)); RecalculateWater(); }
        }

        public decimal WaterCost
        {
            get => _waterCost;
            set { _waterCost = value; OnPropertyChanged(nameof(WaterCost)); RecalculateTotal(); }
        }

        public decimal OtherCharges
        {
            get => _otherCharges;
            set { _otherCharges = value; OnPropertyChanged(nameof(OtherCharges)); RecalculateTotal(); }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(nameof(TotalAmount)); OnPropertyChanged(nameof(FormattedTotal)); }
        }

        public BillStatus BillStatus
        {
            get => _billStatus;
            set { _billStatus = value; OnPropertyChanged(nameof(BillStatus)); OnPropertyChanged(nameof(StatusText)); OnPropertyChanged(nameof(CurrentStatus)); }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(nameof(DueDate)); OnPropertyChanged(nameof(IsOverdue)); OnPropertyChanged(nameof(DaysOverdue)); }
        }

        public DateTime? PaidDate
        {
            get => _paidDate;
            set { _paidDate = value; OnPropertyChanged(nameof(PaidDate)); }
        }

        public decimal? PaidAmount
        {
            get => _paidAmount;
            set { _paidAmount = value; OnPropertyChanged(nameof(PaidAmount)); }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(nameof(CreatedAt)); }
        }

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set { _updatedAt = value; OnPropertyChanged(nameof(UpdatedAt)); }
        }

        // Navigation properties
        public RentalContract Contract { get; set; }

        // Computed properties
        public string StatusText
        {
            get
            {
                if (BillStatus == BillStatus.Pending)
                    return "🟡 Chờ thanh toán";
                else if (BillStatus == BillStatus.Paid)
                    return "🟢 Đã thanh toán";
                else if (BillStatus == BillStatus.Overdue)
                    return "🔴 Quá hạn";
                else if (BillStatus == BillStatus.Partial)
                    return "🟠 Thanh toán một phần";
                else
                    return "❓ Không xác định";
            }
        }

        public string CurrentStatus
        {
            get
            {
                if (BillStatus == BillStatus.Pending && IsOverdue)
                    return "🔴 Quá hạn";
                return StatusText;
            }
        }

        public bool IsOverdue => BillStatus == BillStatus.Pending && DateTime.Now > DueDate;

        public int DaysOverdue => IsOverdue ? (int)(DateTime.Now - DueDate).TotalDays : 0;

        public string BillPeriod => $"{BillMonth:D2}/{BillYear}";

        public string FormattedTotal => TotalAmount.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        public string FormattedRoomRent => RoomRent.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        public string FormattedElectricityCost => ElectricityCost.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        public string FormattedWaterCost => WaterCost.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        public string FormattedOtherCharges => OtherCharges.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        public string FormattedDueDate => DueDate.ToString("dd/MM/yyyy");
        public string FormattedTotalAmount => TotalAmount.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        public string FormattedRoomCharge => RoomRent.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        public string FormattedElectricityCharge => ElectricityCost.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        public string FormattedWaterCharge => WaterCost.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        public string PaymentStatusText => StatusText;

        public string CustomerName => !string.IsNullOrEmpty(_customerNameDisplay) ? _customerNameDisplay : (Contract?.Customer?.CustomerFullName ?? "Chưa có");
        public string RoomNumber => !string.IsNullOrEmpty(_roomNumberDisplay) ? _roomNumberDisplay : (Contract?.Room?.RoomNumber ?? "Chưa có");
        public string ContractNumber => !string.IsNullOrEmpty(_contractNumberDisplay) ? _contractNumberDisplay : (Contract?.ContractNumber ?? "Chưa có");

        // Constructor
        public MonthlyBill()
        {
            BillMonth = DateTime.Now.Month;
            BillYear = DateTime.Now.Year;
            ElectricityRate = 3500; // Default rate
            WaterRate = 20000; // Default rate
            BillStatus = BillStatus.Pending;
            DueDate = new DateTime(BillYear, BillMonth, 5); // Due on 5th of each month
            if (DueDate < DateTime.Now)
                DueDate = DueDate.AddMonths(1);
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        // Calculation methods
        private void RecalculateElectricity()
        {
            ElectricityCost = ElectricityUsage * ElectricityRate;
        }

        private void RecalculateWater()
        {
            WaterCost = WaterUsage * WaterRate;
        }

        private void RecalculateTotal()
        {
            TotalAmount = RoomRent + ElectricityCost + WaterCost + OtherCharges;
        }

        public void CalculateTotal()
        {
            RecalculateElectricity();
            RecalculateWater();
            RecalculateTotal();
        }

        // Payment methods
        public void MarkAsPaid(decimal amount, DateTime? paymentDate = null)
        {
            PaidAmount = amount;
            PaidDate = paymentDate ?? DateTime.Now;
            
            if (amount >= TotalAmount)
                BillStatus = BillStatus.Paid;
            else if (amount > 0)
                BillStatus = BillStatus.Partial;
            
            UpdatedAt = DateTime.Now;
        }

        public void MarkAsOverdue()
        {
            if (BillStatus == BillStatus.Pending && DateTime.Now > DueDate)
            {
                BillStatus = BillStatus.Overdue;
                UpdatedAt = DateTime.Now;
            }
        }

        // Auto-update status
        public void UpdateStatus()
        {
            if (BillStatus == BillStatus.Pending && DateTime.Now > DueDate)
            {
                BillStatus = BillStatus.Overdue;
                UpdatedAt = DateTime.Now;
            }
        }

        // Methods to set display values
        public void SetDisplayValues(string customerName, string roomNumber, string contractNumber)
        {
            _customerNameDisplay = customerName;
            _roomNumberDisplay = roomNumber;
            _contractNumberDisplay = contractNumber;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum BillStatus
    {
        Pending = 0,    // Chờ thanh toán
        Paid = 1,       // Đã thanh toán
        Overdue = 2,    // Quá hạn
        Partial = 3     // Thanh toán một phần
    }
}
