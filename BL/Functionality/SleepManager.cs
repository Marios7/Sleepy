using System.Runtime.InteropServices;

public static class SleepManager
{
    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    /// <summary>
    /// ES_CONTINUOUS Keeps the state active until explicitly changed
    //  ES_SYSTEM_REQUIRED Prevents system sleep
    //  ES_DISPLAY_REQUIRED Prevents the screen from turning off or locking
    /// </summary>
    public static void PreventSleep()
    {
        // Notify the system to prevent sleep and keep the display on
        SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);
    }
    public static void AllowSleep()
    {
        // Reset the state, allowing the system to sleep
        SetThreadExecutionState(ES_CONTINUOUS);
    }
}
