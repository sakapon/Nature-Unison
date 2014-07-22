using NatureUnison;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleAppTest
{
    static class FingerGestureTest
    {
        public static void DistanceTest()
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

        public static void PinchReportedTest()
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

        public static void DragTest()
        {
            var gesture = new FingerGesture();
            gesture.DragStarted += f =>
            {
                Console.WriteLine("Finger Drag Started");
            };
            gesture.Dragged += (f, v) =>
            {
                Console.WriteLine(v);
            };
            gesture.Dropped += (f, v) =>
            {
                Console.WriteLine(v.HasValue ? v.Value.ToString() : "Not Tracked");
                Console.WriteLine("Finger Dropped");
            };
        }
    }
}
