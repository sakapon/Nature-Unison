using NatureUnison;
using NatureUnison.Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //IsConnectedTest();
            //SingleHandFrameTest();
            //DistanceTest();
            //PinchReportedTest();
            DragTest();

            HandsContext.Current.AddTracker(new LeapHandsTracker());

            Console.WriteLine("Press [Enter] key to exit.");
            Console.ReadLine();
        }

        static void IsConnectedTest()
        {
            LeapContext.Current.AddPropertyChangedHandler("IsConnected", () =>
            {
                Console.WriteLine("IsConnected: {0}", LeapContext.Current.IsConnected);
            });
        }

        static void SingleHandFrameTest()
        {
            HandsContext.Current.SingleHandFrame += f =>
            {
                if (f.HasValue)
                {
                    Console.WriteLine("{0} {1}", f.Value.Fingers.Length, f.Value.PalmPosition);
                }
            };
        }

        static void DistanceTest()
        {
            var gesture = new FingerGesture();
            gesture.TwoFingersDistance += (f, d) =>
            {
                if (d.HasValue)
                {
                    Console.WriteLine("{0:N2}", d.Value);
                }
            };
        }

        static void PinchReportedTest()
        {
            var gesture = new FingerGesture();
            gesture.PinchReported += (f, b) =>
            {
                if (b)
                {
                    Console.WriteLine("Pinched {0:HH:mm:ss.fff}", DateTime.Now);
                }
            };
        }

        static void DragTest()
        {
            var gesture = new FingerGesture();
            gesture.DragStarted += f =>
            {
                Console.WriteLine("Drag Started");
            };
            gesture.Dragged += (f, v) =>
            {
                Console.WriteLine(v);
            };
            gesture.Dropped += (f, v) =>
            {
                Console.WriteLine(v.HasValue ? v.Value.ToString() : "Not Tracked");
                Console.WriteLine("Dropped");
            };
        }
    }
}
