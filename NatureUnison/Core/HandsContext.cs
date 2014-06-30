using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace NatureUnison
{
    public class HandsContext
    {
        public static HandsContext Current { get; private set; }

        static HandsContext()
        {
            Current = new HandsContext();
        }

        HashSet<HandsTracker> trackers = new HashSet<HandsTracker>();

        public HandsTracker[] GetTrackers()
        {
            return trackers.ToArray();
        }

        public void AddTracker(HandsTracker tracker)
        {
            if (tracker == null) throw new ArgumentNullException("tracker");

            tracker.Context = this;
            trackers.Add(tracker);
        }

        public void RemoveTracker(HandsTracker tracker)
        {
            if (tracker == null) throw new ArgumentNullException("tracker");

            trackers.Remove(tracker);
            tracker.Context = null;
        }

        public event Action<HandFrame?> SingleHandFrame = f => { };

        internal void NotifySingleHandFrame(HandFrame? f)
        {
            SingleHandFrame(f);
        }
    }

    public struct HandFrame
    {
        public Point3D? PalmPosition;
        public FingerFrame[] Fingers;
    }

    public struct FingerFrame
    {
        public Point3D? TipPosition;
    }
}
