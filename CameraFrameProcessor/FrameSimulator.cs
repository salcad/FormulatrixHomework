using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Formulatrix.CameraFrameProcessor
{
    public class FrameSimulator
    {
        private readonly IFrameCallback _callback;
        private readonly Timer _timer;
        private readonly Timer _fpsTimer;
        private readonly Random _random = new Random();
        private bool _isRunning = false;
        private int _framesSent = 0;
        private int _framesInLastSecond = 0;
        private readonly object _lock = new object();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public FrameSimulator(IFrameCallback callback)
        {
            _callback = callback;
            _timer = new Timer(SimulateFrame, null, Timeout.Infinite, Timeout.Infinite);
            _fpsTimer = new Timer(DisplayFpsStats, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            _isRunning = true;
            _stopwatch.Start();
            _timer.Change(0, 33); // 30 FPS (33ms interval)
            _fpsTimer.Change(1000, 1000); // Display FPS every second
            Console.WriteLine("Frame simulation started at 30 FPS...");
            Console.WriteLine("Time | Frames Sent | Current FPS | Avg FPS");
            Console.WriteLine("-----|-------------|-------------|--------");
        }

        public void Stop()
        {
            _isRunning = false;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _fpsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _stopwatch.Stop();
            Console.WriteLine("Frame simulation stopped.");
            
            // Final statistics
            if (_stopwatch.Elapsed.TotalSeconds > 0)
            {
                double avgFps = _framesSent / _stopwatch.Elapsed.TotalSeconds;
                Console.WriteLine($"Final Stats: {_framesSent} frames in {_stopwatch.Elapsed.TotalSeconds:F1}s (Avg: {avgFps:F1} FPS)");
            }
        }

        private void SimulateFrame(object state)
        {
            if (!_isRunning) return;

            // Create a simulated frame (100x100 pixels)
            int width = 100;
            int height = 100;
            int frameSize = width * height;
            
            byte[] frameData = new byte[frameSize];
            
            // Fill with random pixel values (simulating camera noise/movement)
            _random.NextBytes(frameData);
            
            // Allocate unmanaged memory and copy data
            IntPtr framePtr = Marshal.AllocHGlobal(frameSize);
            try
            {
                Marshal.Copy(frameData, 0, framePtr, frameSize);
                _callback.FrameReceived(framePtr, width, height);
                
                lock (_lock)
                {
                    _framesSent++;
                    _framesInLastSecond++;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(framePtr);
            }
        }

        private void DisplayFpsStats(object state)
        {
            if (!_isRunning) return;
            
            lock (_lock)
            {
                double elapsed = _stopwatch.Elapsed.TotalSeconds;
                double avgFps = elapsed > 0 ? _framesSent / elapsed : 0;
                
                Console.WriteLine($"{elapsed,4:F1}s | {_framesSent,11} | {_framesInLastSecond,11:F1} | {avgFps,7:F1}");
                _framesInLastSecond = 0;
            }
        }

        public void Dispose()
        {
            Stop();
            _timer?.Dispose();
            _fpsTimer?.Dispose();
        }
    }
}