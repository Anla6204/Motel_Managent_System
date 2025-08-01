using System;

namespace AnLaNPWPF.Helpers
{
    public static class RefreshManager
    {
        // Events for refreshing different views
        public static event Action RefreshCustomerData;
        public static event Action RefreshRoomData;
        
        // Methods to trigger refresh
        public static void TriggerCustomerRefresh()
        {
            RefreshCustomerData?.Invoke();
        }
        
        public static void TriggerRoomRefresh()
        {
            RefreshRoomData?.Invoke();
        }
        
        public static void TriggerAllRefresh()
        {
            RefreshCustomerData?.Invoke();
            RefreshRoomData?.Invoke();
        }
    }
}
