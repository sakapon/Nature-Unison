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
            //FingerGestureTest.DragTest();
            //HandGestureTest.DragTest();
            HandGestureTest.PushTest();

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
    }
}
