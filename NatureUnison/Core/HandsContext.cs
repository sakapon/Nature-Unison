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

        public event Action<HandFrame?, double?> TwoFingersDistance = (f, d) => { };
        public event Action<HandFrame?, bool> PinchReported = (f, b) => { };
        //public event Action<HandFrame?, bool> PinchChanged = (f, b) => { };

        public event Action<HandFrame?> DragStarted = f => { };
        public event Action<HandFrame?, Vector3D> Dragged = (f, v) => { };
        public event Action<HandFrame?, Vector3D?> Dropped = (f, v) => { };

        const double MaxPinchDistance = 200;

        public HandsContext()
        {
            SingleHandFrame += f =>
            {
                if (!f.HasValue)
                {
                    TwoFingersDistance(f, null);
                    return;
                }

                var frontFingers = f.Value.Fingers
                    .Where(_f => !double.IsNaN(_f.TipPosition.Z))
                    .OrderByDescending(_f => _f.TipPosition.Z)
                    .Take(2)
                    .ToArray();
                if (frontFingers.Length != 2)
                {
                    TwoFingersDistance(f, null);
                    return;
                }

                var d = (frontFingers[0].TipPosition - frontFingers[1].TipPosition).Length;
                TwoFingersDistance(f, d);
            };

            var distanceState = PinchDistanceState.Unknown;
            var isPinched = false;
            TwoFingersDistance += (f, d) =>
            {
                var distanceState_old = distanceState;
                distanceState = ToPinchDistanceState(d);

                if (distanceState_old == PinchDistanceState.InRange && distanceState == PinchDistanceState.Unknown && f.HasValue)
                {
                    isPinched = true;
                }
                else if (isPinched && (distanceState != PinchDistanceState.Unknown || !f.HasValue))
                {
                    isPinched = false;
                }

                PinchReported(f, isPinched);
            };

            var dragStartedFrame = default(HandFrame);
            var isPinched2 = false;
            PinchReported += (f, b) =>
            {
                var isPinched2_old = isPinched2;
                isPinched2 = b;

                if (isPinched2_old)
                {
                    if (isPinched2)
                    {
                        Dragged(f, f.Value.PalmPosition - dragStartedFrame.PalmPosition);
                    }
                    else
                    {
                        Dropped(f, f.HasValue ? f.Value.PalmPosition - dragStartedFrame.PalmPosition : default(Vector3D?));
                    }
                }
                else
                {
                    if (isPinched2)
                    {
                        dragStartedFrame = f.Value;
                        DragStarted(f);
                    }
                }
            };
        }

        static readonly Func<double?, PinchDistanceState> ToPinchDistanceState = d =>
            !d.HasValue ? PinchDistanceState.Unknown :
            d.Value < MaxPinchDistance ? PinchDistanceState.InRange :
            PinchDistanceState.OutOfRange;

        enum PinchDistanceState
        {
            Unknown,
            InRange,
            OutOfRange,
        }
    }

    public struct HandFrame
    {
        public Point3D PalmPosition;
        public FingerFrame[] Fingers;
        public FingerFrame? FrontmostFinger;
    }

    public struct FingerFrame
    {
        public Point3D TipPosition;
    }
}
