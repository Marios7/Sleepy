
using System.Runtime.InteropServices;


namespace Sleppy.Functionality
{
    class IdleTimeDetector
    {
    [StructLayout(LayoutKind.Sequential)]
    struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO info);

        public static TimeSpan GetIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            if(GetLastInputInfo(ref lastInputInfo))
            {
                uint lastInputTick = lastInputInfo.dwTime;
                uint currentTick = (uint)Environment.TickCount;
                var idleTime = TimeSpan.FromMilliseconds(currentTick - lastInputTick);

                //Console.WriteLine("idleTime: {0}", idleTime.TotalSeconds.ToString());
                return idleTime;
            }
            return TimeSpan.Zero;
        }
    }
}
