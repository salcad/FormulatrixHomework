using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Formulatrix.CameraFrameProcessor
{
    public class CameraFrameProcessor : IFrameCallback
    {
        private readonly IValueReporter _reporter;
        private volatile bool _isProcessing = false;

        public CameraFrameProcessor(IValueReporter reporter)
        {
            _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        public void FrameReceived(IntPtr pFrame, int width, int height)
        {
            if (pFrame == IntPtr.Zero || width <= 0 || height <= 0)
                return;

            if (_isProcessing)
                return; // Skip frame if still processing previous one

            _isProcessing = true;
            
            try
            {
                ThreadPool.QueueUserWorkItem(_ => ProcessFrameSafe(pFrame, width, height));
            }
            catch
            {
                _isProcessing = false;
                throw;
            }
        }

        private void ProcessFrameSafe(IntPtr pFrame, int width, int height)
        {
            try
            {
                int frameSize = width * height;
                byte[] frameData = new byte[frameSize];
                Marshal.Copy(pFrame, frameData, 0, frameSize);
                
                long sum = 0;
                for (int i = 0; i < frameData.Length; i++)
                {
                    sum += frameData[i];
                }
                
                double average = (double)sum / frameData.Length;
                _reporter.Report(average);
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}