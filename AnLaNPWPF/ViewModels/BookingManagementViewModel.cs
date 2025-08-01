using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using BusinessObject;
using Repository;

namespace AnLaNPWPF.ViewModels
{
    public class BookingManagementViewModel : INotifyPropertyChanged
    {
        private readonly BookingRepository _bookingRepository;
        private ObservableCollection<BookingModel> _bookings;
        private BookingModel _selectedBooking;
        private string _searchText;
        private DateTime? _searchStartDate;
        private DateTime? _searchEndDate;
        private int _selectedStatusFilter;

        public ObservableCollection<BookingModel> Bookings
        {
            get => _bookings;
            set { _bookings = value; OnPropertyChanged(); }
        }

        public BookingModel SelectedBooking
        {
            get => _selectedBooking;
            set 
            { 
                _selectedBooking = value; 
                OnPropertyChanged();
                ((AnLaNPWPF.ViewModels.RelayCommand)ConfirmBookingCommand).RaiseCanExecuteChanged();
                ((AnLaNPWPF.ViewModels.RelayCommand)CancelBookingCommand).RaiseCanExecuteChanged();
                ((AnLaNPWPF.ViewModels.RelayCommand)ViewDetailsCommand).RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public DateTime? SearchStartDate
        {
            get => _searchStartDate;
            set { _searchStartDate = value; OnPropertyChanged(); }
        }

        public DateTime? SearchEndDate
        {
            get => _searchEndDate;
            set { _searchEndDate = value; OnPropertyChanged(); }
        }

        public int SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { _selectedStatusFilter = value; OnPropertyChanged(); }
        }

        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ConfirmBookingCommand { get; }
        public ICommand CancelBookingCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public BookingManagementViewModel()
        {
            _bookingRepository = new BookingRepository();
            _bookings = new ObservableCollection<BookingModel>();
            
            SearchCommand = new AnLaNPWPF.ViewModels.RelayCommand(ExecuteSearch);
            RefreshCommand = new AnLaNPWPF.ViewModels.RelayCommand(ExecuteRefresh);
            ConfirmBookingCommand = new AnLaNPWPF.ViewModels.RelayCommand(ExecuteConfirmBooking, CanExecuteConfirmBooking);
            CancelBookingCommand = new AnLaNPWPF.ViewModels.RelayCommand(ExecuteCancelBooking, CanExecuteCancelBooking);
            ViewDetailsCommand = new AnLaNPWPF.ViewModels.RelayCommand(ExecuteViewDetails, CanExecuteViewDetails);
            
            LoadBookings();
        }

        private void LoadBookings()
        {
            try
            {
                var bookings = _bookingRepository.GetAll();
                Bookings = new ObservableCollection<BookingModel>(bookings);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSearch(object parameter)
        {
            try
            {
                var allBookings = _bookingRepository.GetAll();
                var filteredBookings = allBookings.AsQueryable();

                // Filter by search text (customer name, room number, booking ID)
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredBookings = filteredBookings.Where(b => 
                        b.CustomerName.Contains(SearchText) ||
                        b.RoomNumber.Contains(SearchText) ||
                        b.BookingId.ToString().Contains(SearchText));
                }

                // Filter by date range
                if (SearchStartDate.HasValue)
                {
                    filteredBookings = filteredBookings.Where(b => b.CheckInDate >= SearchStartDate.Value);
                }

                if (SearchEndDate.HasValue)
                {
                    filteredBookings = filteredBookings.Where(b => b.CheckOutDate <= SearchEndDate.Value);
                }

                // Filter by status
                if (SelectedStatusFilter > 0)
                {
                    filteredBookings = filteredBookings.Where(b => b.BookingStatusId == SelectedStatusFilter);
                }

                Bookings = new ObservableCollection<BookingModel>(filteredBookings.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching bookings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteRefresh(object parameter)
        {
            SearchText = string.Empty;
            SearchStartDate = null;
            SearchEndDate = null;
            SelectedStatusFilter = 0;
            LoadBookings();
        }

        private bool CanExecuteConfirmBooking(object parameter)
        {
            return SelectedBooking != null && SelectedBooking.BookingStatusId == 1; // Only pending bookings
        }

        private void ExecuteConfirmBooking(object parameter)
        {
            if (SelectedBooking == null) return;

            var result = MessageBox.Show(
                $"Confirm booking #{SelectedBooking.BookingId} for {SelectedBooking.CustomerName}?",
                "Confirm Booking",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _bookingRepository.ConfirmBooking(SelectedBooking.BookingId);
                    SelectedBooking.BookingStatusId = 2;
                    SelectedBooking.BookingStatus = "Confirmed";
                    OnPropertyChanged(nameof(SelectedBooking));
                    
                    MessageBox.Show("Booking confirmed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error confirming booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecuteCancelBooking(object parameter)
        {
            return SelectedBooking != null && (SelectedBooking.BookingStatusId == 1 || SelectedBooking.BookingStatusId == 2); // Pending or Confirmed
        }

        private void ExecuteCancelBooking(object parameter)
        {
            if (SelectedBooking == null) return;

            var result = MessageBox.Show(
                $"Cancel booking #{SelectedBooking.BookingId} for {SelectedBooking.CustomerName}?\nThis action cannot be undone.",
                "Cancel Booking",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _bookingRepository.CancelBooking(SelectedBooking.BookingId);
                    SelectedBooking.BookingStatusId = 4;
                    SelectedBooking.BookingStatus = "Cancelled";
                    OnPropertyChanged(nameof(SelectedBooking));
                    
                    MessageBox.Show("Booking cancelled successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error cancelling booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecuteViewDetails(object parameter)
        {
            return SelectedBooking != null;
        }

        private void ExecuteViewDetails(object parameter)
        {
            if (SelectedBooking == null) return;

            var detailsWindow = new Views.BookingDetailsDialog(SelectedBooking);
            detailsWindow.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
