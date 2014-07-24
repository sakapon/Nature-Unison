using NatureUnison;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleAppTest
{
    class HandGestureTest
    {
        HandGesture gesture = new HandGesture();

        public void GripReportedTest()
        {
            gesture.GripReported += (f, b) =>
            {
                if (b)
                {
                    Console.WriteLine("Gripped {0:HH:mm:ss.fff}", DateTime.Now);
                }
            };
        }

        public void DragTest()
        {
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
                Console.WriteLine("Hand Dropped {0}", v);
            };
            gesture.DragCancelled += f =>
            {
                Console.WriteLine("Hand Drag Cancelled");
            };
        }

        public void HoldUpReportedTest()
        {
            gesture.HoldUpReported += (f, b) =>
            {
                if (b)
                {
                    Console.WriteLine("Held up {0:HH:mm:ss.fff}", DateTime.Now);
                }
            };
        }

        public void HoldUpChangedTest()
        {
            gesture.HoldUpChanged += (f, b) =>
            {
                Console.WriteLine(b ? "Hold Up Started" : "Hold Up Ended");
            };
        }

        public void PushTest()
        {
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
