using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Sleppy.Functionality
{
    public class MouseMovement
    {
        private static Random random = new Random();

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public int type;
            public InputUnione U;//To rappresent various input events
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnione 
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        private const int INPUT_MOUSE = 0;
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);
        
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
        public static void MoveMouse()
        {

            // Screen dimensions (to be adjusted to be dynamic but it's ok like this)
            int screenWidth = 200;
            int screenHeight = 200;

            // Generate random absolute screen coordinates
            int screenX = random.Next(0, screenWidth);
            int screenY = random.Next(0, screenHeight);

            // Convert to absolute coordinates (0–65535 range)
            int absoluteX = (screenX * 65535) / screenWidth;
            int absoluteY = (screenY * 65535) / screenHeight;

            INPUT[] input = new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_MOUSE,
                    U = new InputUnione
                    {
                        mi = new MOUSEINPUT
                        {
                            dx = absoluteX,
                            dy = absoluteY,
                            mouseData = 0,

                            dwFlags = MOUSEEVENTF_MOVE|MOUSEEVENTF_ABSOLUTE,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                }
            };
            SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(INPUT)));
            //mouse_event(MOUSEEVENTF_MOVE, deltaX, deltaY, 0, 0);
        }
    }
}
