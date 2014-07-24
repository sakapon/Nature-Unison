﻿using NatureUnison;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleAppTest
{
    class FingerGestureTest
    {
        FingerGesture gesture = new FingerGesture();

        public void DistanceTest()
        {
            gesture.TwoFingersDistance += (f, d) =>
            {
                if (d.HasValue)
                {
                    Console.WriteLine("{0:N2}", d.Value);
                }
            };
        }

        public void PinchReportedTest()
        {
            gesture.PinchReported += (f, b) =>
            {
                if (b)
                {
                    Console.WriteLine("Pinched {0:HH:mm:ss.fff}", DateTime.Now);
                }
            };
        }

        public void DragTest()
        {
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
                Console.WriteLine("Finger Push Started");
            };
            gesture.PushProgress += (f, v) =>
            {
                Console.WriteLine(v);
            };
            gesture.Pushed += f =>
            {
                Console.WriteLine("Finger Pushed");
            };
            gesture.PushCancelled += f =>
            {
                Console.WriteLine("Finger Push Cancelled");
            };
        }
    }
}
