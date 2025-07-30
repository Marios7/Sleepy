using Sleppy.Functionality;

namespace Sleppy
{
    public class Sleepy
    {
        private readonly ILogger<Sleepy> _logger;
        private readonly TimeSpan waitInterval = TimeSpan.FromSeconds(1);
        private int isPreventingSleep = 0; // Changed to int for Interlocked compatibility  
        private CancellationTokenSource cancellationTokenSource;
        private Task? _backgroundTask; // Made nullable to resolve CS8618  

        public Sleepy(ILogger<Sleepy> logger)
        {
            _logger = logger;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start(int idleThresholdInSeconds, bool moveMouse)
        {
            cancellationTokenSource = new CancellationTokenSource(); // Reset the CTS 
            if (_backgroundTask != null && !_backgroundTask.IsCompleted)
            {
                return; // Task is already running  
            }
            if (moveMouse)
                _backgroundTask = Task.Run(() => ExcuteAsyncWithMouseMovement(cancellationTokenSource.Token, idleThresholdInSeconds));
            else
                _backgroundTask = Task.Run(() => ExcuteAsyncWithoutMouseMovement(cancellationTokenSource.Token, idleThresholdInSeconds));
        }

        public void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            SleepManager.AllowSleep();
            Interlocked.Exchange(ref isPreventingSleep, 0); // Reset flag safely  
            _backgroundTask = null;

        }

        private async Task ExcuteAsyncWithoutMouseMovement(CancellationToken stoppingToken, int idleThresholdInSeconds)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan idleTime = IdleTimeDetector.GetIdleTime();

                if (idleTime.TotalSeconds > idleThresholdInSeconds)
                {

                    SleepManager.PreventSleep();
                    await Task.Delay(waitInterval, stoppingToken);
                    if (stoppingToken.IsCancellationRequested) break;
                }
            }
        }

        private async Task ExcuteAsyncWithMouseMovement(CancellationToken stoppingToken, int idleThresholdInSeconds)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan idleTime = IdleTimeDetector.GetIdleTime();
                if (idleTime.TotalSeconds > idleThresholdInSeconds)
                {
                    MouseMovement.MoveMouse();
                    await Task.Delay(waitInterval, stoppingToken);
                }
            }
        }
    }
}
