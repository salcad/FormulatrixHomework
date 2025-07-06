using System;
using System.Diagnostics;

namespace Formulatrix.CameraFrameProcessor
{
    public class ConsoleValueReporter : IValueReporter
    {
        private int _frameCount = 0;
        private readonly object _lock = new object();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private DateTime _lastFrameTime = DateTime.Now;

        public void Report(double value)
        {
            lock (_lock)
            {
                _frameCount++;
                var now = DateTime.Now;
                var timeSinceLastFrame = (now - _lastFrameTime).TotalMilliseconds;
                var instantFps = timeSinceLastFrame > 0 ? 1000.0 / timeSinceLastFrame : 0;
                
                Console.WriteLine($"[{_stopwatch.Elapsed.TotalSeconds:F1}s] Frame #{_frameCount:D4}: Avg = {value:F2} | Instant FPS: {instantFps:F1}");
                
                _lastFrameTime = now;
            }
        }
    }
}