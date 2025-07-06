using System;

namespace Formulatrix.CameraFrameProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Camera Frame Processor Demo");
            Console.WriteLine("===========================");
            
            // Mock reporter for demonstration
            IValueReporter reporter = new ConsoleValueReporter();
            
            // Create processor
            IFrameCallback processor = new CameraFrameProcessor(reporter);
            
            // Create frame simulator
            FrameSimulator simulator = new FrameSimulator(processor);
            
            Console.WriteLine("Starting frame simulation...");
            simulator.Start();
            
            Console.WriteLine("\nPress any key to stop and exit...");
            Console.ReadKey();
            
            simulator.Stop();
            simulator.Dispose();
            
            Console.WriteLine("\nDemo completed.");
        }
    }
}