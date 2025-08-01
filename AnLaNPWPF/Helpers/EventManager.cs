using System;

namespace AnLaNPWPF.Helpers
{
    /// <summary>
    /// Global Event Bus để quản lý communication giữa các view
    /// </summary>
    public static class GlobalEventBus
    {
        // Events cho Room Management
        public static event Action RoomDataChanged;
        public static event Action<int> RoomStatusChanged; // roomId
        
        // Events cho Contract Management  
        public static event Action ContractDataChanged;
        public static event Action<int> ContractStatusChanged; // contractId
        
        // Events cho Bill Management
        public static event Action BillDataChanged;
        public static event Action<int> BillStatusChanged; // billId
        
        // Events cho Customer Management
        public static event Action CustomerDataChanged;
        public static event Action<int> CustomerUpdated; // customerId

        #region Room Events
        public static void OnRoomDataChanged()
        {
            RoomDataChanged?.Invoke();
        }
        
        public static void OnRoomStatusChanged(int roomId)
        {
            RoomStatusChanged?.Invoke(roomId);
            OnRoomDataChanged(); // Also trigger general room data change
        }
        #endregion

        #region Contract Events
        public static void OnContractDataChanged()
        {
            ContractDataChanged?.Invoke();
        }
        
        public static void OnContractStatusChanged(int contractId)
        {
            ContractStatusChanged?.Invoke(contractId);
            OnContractDataChanged();
        }
        #endregion

        #region Bill Events
        public static void OnBillDataChanged()
        {
            BillDataChanged?.Invoke();
        }
        
        public static void OnBillStatusChanged(int billId)
        {
            BillStatusChanged?.Invoke(billId);
            OnBillDataChanged();
        }
        #endregion

        #region Customer Events
        public static void OnCustomerDataChanged()
        {
            CustomerDataChanged?.Invoke();
        }
        
        public static void OnCustomerUpdated(int customerId)
        {
            CustomerUpdated?.Invoke(customerId);
            OnCustomerDataChanged();
        }
        #endregion
    }
}
