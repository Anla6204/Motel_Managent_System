using System;

namespace AnLaNPWPF.Helpers
{
    public static class DataChangedEventManager
    {
        // Events for data changes
        public static event Action CustomerDataChanged;
        public static event Action RoomDataChanged;

        // Methods to raise events
        public static void OnCustomerDataChanged()
        {
            CustomerDataChanged?.Invoke();
        }

        public static void OnRoomDataChanged()
        {
            RoomDataChanged?.Invoke();
        }

        // Convenience method to refresh all data
        public static void OnAllDataChanged()
        {
            CustomerDataChanged?.Invoke();
            RoomDataChanged?.Invoke();
        }
    }
}
