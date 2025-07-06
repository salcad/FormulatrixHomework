using System;

namespace Formulatrix.CameraFrameProcessor
{
    public interface IFrameCallback
    {
        void FrameReceived(IntPtr pFrame, int width, int height);
    }
}