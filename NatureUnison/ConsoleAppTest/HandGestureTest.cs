using NatureUnison;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleAppTest
{
    static class HandGestureTest
    {
        public static void GripReportedTest()
        {
            var gesture = new HandGesture();
            gesture.GripReported += (f, b) =>
            {
                if (b)
                {
                    Console.WriteLine("Gripped {0:HH:mm:ss.fff}", DateTime.Now);
                }
            };
        }

        public static void DragTest()
        {
            var gesture = new HandGesture();
            gesture.DragStarted += f =>
            {
                Console.WriteLine("Hand Drag Started");
            };
            gesture.Dragged += (f, v) =>
            {
                Console.WriteLine(v);
            };
            gesture.Dropped += (f, v) =>
            {
                Console.WriteLine(v.HasValue ? v.Value.ToString() : "Not Tracked");
                Console.WriteLine("Hand Dropped");
            };
        }

        public static void HoldUpReportedTest()
        {
            var gesture = new HandGesture();
            gesture.HoldUpReported += (f, b) =>
            {
                if (b)
                {
                    Console.WriteLine("Held up {0:HH:mm:ss.fff}", DateTime.Now);
                }
            };
        }

        public static void HoldUpChangedTest()
        {
            var gesture = new HandGesture();
            gesture.HoldUpChanged += (f, b) =>
            {
                Console.WriteLine(b ? "Hold Up Started" : "Hold Up Ended");
            };
        }

        public static void PushTest()
        {
            var gesture = new HandGesture();
            gesture.PushStarted += f =>
            {
                Console.WriteLine("Hand Push Started");
            };
            gesture.PushProgress += (f, v) =>
            {
                Console.WriteLine(v);
            };
            gesture.Pushed += f =>
            {
                Console.WriteLine("Hand Pushed");
            };
            gesture.PushCancelled += f =>
            {
                Console.WriteLine("Hand Push Cancelled");
            };
        }
    }
}
