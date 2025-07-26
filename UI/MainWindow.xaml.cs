using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Win32;
using Sleppy;
using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Windows.Graphics;


namespace UI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private bool isRunning = false;
        private TaskbarIcon taskbarIcon;
        private const string AppName = "Sleepy";
        private const string StartUpRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const int DefaultSecondsThreshold = 1;
   
        private Sleepy sleepyInstance;
        private readonly string AppPath = System.Reflection.Assembly.GetExecutingAssembly().Location; // Path to the application executable
        private readonly string IconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sleepy.ico");
        private readonly string SleepGifPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sleepy_cat.gif");
        private readonly string AwakeGifPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Awake_cat.gif");

        private static AppWindow MAIN_WINDOW = null;
        public MainWindow()
        {
            MAIN_WINDOW = GetAppWindow(); // Get the AppWindow instance
            InitializeComponent();
            InitializeTaskbarIcon();
            InitializeSleepy();
            SetWindowIcon();
            SetWindowDimensions();
            CheckWindowsStartUp();
            InitializeAnimation();
            AssignEvents();
        }

        private void InitializeAnimation()
        {
            GifView.Source = new Uri(SleepGifPath);
        }

        private void SetWindowIcon()
        {
            MAIN_WINDOW.SetIcon(IconPath);
        }

        #region Initialization
        private void InitializeSleepy()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            ILogger<Sleepy> logger = loggerFactory.CreateLogger<Sleepy>();
            sleepyInstance = new Sleepy(logger);
        }

        private AppWindow GetAppWindow()
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this); // Get the window handle
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            return AppWindow.GetFromWindowId(windowId); // Retrieve the AppWindow instance
        }

        private void InitializeTaskbarIcon()
        {
            if (taskbarIcon == null)
            {
                taskbarIcon = new TaskbarIcon
                {
                    IconSource = new BitmapImage(new Uri(IconPath)),
                    ToolTipText = AppName,
                    Visibility = System.Windows.Visibility.Visible,
                };
                taskbarIcon.TrayMouseDoubleClick += TaskbarIcon_TrayMouseDoubleClick;

                var contextMenu = new ContextMenu();
                contextMenu.Items.Add(new MenuItem { Header = "Restore", Command = new RelayCommand(Restore_ContextMenuClick) });
                 contextMenu.Items.Add(new MenuItem { Header = "Start", Command = new RelayCommand(Start_ContextMenuClick) });
                contextMenu.Items.Add(new MenuItem { Header = "Stop", Command = new RelayCommand(Stop_ContextMenuClick) });
                contextMenu.Items.Add(new MenuItem { Header = "Exit", Command = new RelayCommand(Exit_ContextMenuClick) });
                contextMenu.Items.Add(new MenuItem { Header = "Force Exit", Command = new RelayCommand(ForceExit_ContextMenuClick) });
                taskbarIcon.ContextMenu = contextMenu;
            }
        }

        private void AssignEvents()
        {
            MAIN_WINDOW.Closing += ClosingEvent;
        }
        private void SetWindowDimensions()
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this); //'this' is the Window instance
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            RectInt32 workingArea = displayArea.WorkArea;

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            //Get Monitor information
            const int MONITOR_DEFAULTTONEAREST = 2;
            [DllImport("user32.dll")]
            static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);
            [DllImport("user32.dll")]
            static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            MONITORINFO monitorInfo = new MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            GetMonitorInfo(monitor, ref monitorInfo);

            int workWidth = monitorInfo.rcWork.Right - monitorInfo.rcWork.Left;
            int workHeight = monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top;

            const double baseWidth = 400;
            const double baseHeight = 600;
            const double ratio = baseWidth / baseHeight;

            double scaleFactor = 0.55;
            double maxWidth = workWidth * scaleFactor;
            double maxHeight = workHeight * scaleFactor;

            // Scale to keep the aspect ratio
            double scaledWidth = maxWidth;
            double scaledHeight = scaledWidth / ratio;

            if (scaledHeight > maxHeight)
            {
                scaledHeight = maxHeight;
                scaledWidth = scaledHeight * ratio;
            }

            // Resize to 80% of the working area
            appWindow.Resize(new SizeInt32(((int)scaledWidth), (int)(scaledHeight)));

            // Set resizable = false
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        #region AppWindowEvents
        private void ClosingEvent(AppWindow sender, AppWindowClosingEventArgs args)
        {
            args.Cancel = true;
            sender.Hide();
            taskbarIcon?.ShowBalloonTip("Sleepy", "Sleepy is still running in the background", BalloonIcon.Info);
        }
        #endregion

        #region Core
        private void ShowCore()
        {
            MAIN_WINDOW.Show();

        }
        private void StopCore()
        {
            isRunning = false;
            if (ActivateAnimation_CheckBox.IsChecked.HasValue && ActivateAnimation_CheckBox.IsChecked.Value)
                ActivateAnimation_CheckBox_Checked(ActivateAnimation_CheckBox, new RoutedEventArgs());
            sleepyInstance.Stop();
        }

        private void StartCore()
        {
            isRunning = true;
            if (ActivateAnimation_CheckBox.IsChecked.HasValue && ActivateAnimation_CheckBox.IsChecked.Value)
                ActivateAnimation_CheckBox_Checked(ActivateAnimation_CheckBox, new RoutedEventArgs());

            bool? MoveMouse = MoveMouse_CheckBox.IsChecked.HasValue ? MoveMouse_CheckBox.IsChecked : false;

            sleepyInstance.Start(DefaultSecondsThreshold, MoveMouse.Value);

        }
        #endregion

        #region TrayIconContextMenu
        private void ForceExit_ContextMenuClick()
        {
            taskbarIcon.Dispose(); // Dispose of the tray icon
            Environment.Exit(0); // Close the application without waiting for the application to finish any process
        }
        private void Restore_ContextMenuClick()
        {
            ShowCore();
        }

        private void Exit_ContextMenuClick()
        {
            taskbarIcon.Dispose(); // Dispose of the tray icon
            Application.Current.Exit(); // Close the application
        }
        private void Stop_ContextMenuClick()
        {
            StopCore();

        }
        private void Start_ContextMenuClick()
        {
            StartCore();
        }
        private void TaskbarIcon_TrayMouseDoubleClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowCore();
        }
        #endregion

        #region UIEvents

        //private void IdleTimeTextBox_PreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        //{
        //    if (e.Key == VirtualKey.Back || e.Key == VirtualKey.Delete || e.Key == VirtualKey.Tab)
        //        return;

        //    // Check if the pressed key is a numeric key (0-9)
        //    if (!Char.IsDigit((char)e.Key))
        //    {
        //        e.Handled = true; // Prevent non-numeric input
        //    }
        //}

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopCore();
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartCore();
        }
        #endregion

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            bool startOnWindowsStartUp = StartUp_CheckBox.IsChecked.HasValue ? StartUp_CheckBox.IsChecked.Value : false;
            SaveRegistryKey(startOnWindowsStartUp);
        }
        private void CheckWindowsStartUp()
        {
            // Open the registry key
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(StartUpRegistryKey, true))
            {
                if (key != null)
                {
                    // Check if the registry entry exists for the app
                    object existingValue = key.GetValue(AppName);
                    if (existingValue != null)
                    {
                        // Registry key exists, mark the checkbox as checked
                        StartUp_CheckBox.IsChecked = true;
                    }
                    else
                    {
                        // Registry key does not exist, uncheck the checkbox
                        StartUp_CheckBox.IsChecked = false;
                    }
                }
            }
        }
        private void SaveRegistryKey(bool enable)
        {
            // Registry key for startup applications
            // Open the registry key
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(StartUpRegistryKey, true))
            {
                if (key != null)
                {
                    if (enable)
                    {
                        string exePath = Environment.ProcessPath;
                        // Set the registry key to make the app start on boot
                        key.SetValue(AppName, $"{exePath}");
                    }
                    else
                    {
                        // Remove the registry key if the user unchecks the box
                        key.DeleteValue(AppName, false);
                    }
                }
            }
        }
        private void ActivateAnimation_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (GifView == null)
            {
                return;
            }
            if (isRunning)
            {
                GifView.Source = new Uri(AwakeGifPath);
            }
            else
            {
                GifView.Source = new Uri(SleepGifPath);

            }
            GifView.Visibility = Visibility.Visible;
        }
        private void ActivateAnimation_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            GifView.Visibility = Visibility.Collapsed;
        }

        //private void ThemeSlider_Toggled(object sender, RoutedEventArgs e)
        //{
        //    if(Content is FrameworkElement element)
        //    {
        //     element.RequestedTheme = ThemeSlider.IsOn ? ElementTheme.Dark : ElementTheme.Light;

        //    }
        //}

        private void StartUp_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool startOnWindowsStartUp = StartUp_CheckBox.IsChecked.HasValue ? StartUp_CheckBox.IsChecked.Value : false;
            SaveRegistryKey(startOnWindowsStartUp);
        }

        private void StartUp_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            bool startOnWindowsStartUp = StartUp_CheckBox.IsChecked.HasValue ? StartUp_CheckBox.IsChecked.Value : false;
            SaveRegistryKey(startOnWindowsStartUp);
        }
    }
}
