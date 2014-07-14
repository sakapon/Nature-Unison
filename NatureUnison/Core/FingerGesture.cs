using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace NatureUnison
{
    public class FingerGesture
    {
        const double MaxPinchDistance = 200;

        public event Action<HandFrame?, double?> TwoFingersDistance = (f, d) => { };
        public event Action<HandFrame?, bool> PinchReported = (f, b) => { };
        //public event Action<HandFrame?, bool> PinchChanged = (f, b) => { };

        public event Action<HandFrame?> DragStarted = f => { };
        public event Action<HandFrame?, Vector3D> Dragged = (f, v) => { };
        public event Action<HandFrame?, Vector3D?> Dropped = (f, v) => { };

        public FingerGesture()
        {
            HandsContext.Current.SingleHandFrame += f =>
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

            var distanceState = new ShortValueHistory<PinchDistanceState>(PinchDistanceState.Unknown);
            var isPinched = false;
            TwoFingersDistance += (f, d) =>
            {
                distanceState.UpdateValue(ToPinchDistanceState(d));

                if (distanceState.Previous == PinchDistanceState.InRange && distanceState.Current == PinchDistanceState.Unknown && f.HasValue)
                {
                    isPinched = true;
                }
                else if (isPinched && (distanceState.Current != PinchDistanceState.Unknown || !f.HasValue))
                {
                    isPinched = false;
                }

                PinchReported(f, isPinched);
            };

            var dragStartedFrame = default(HandFrame);
            var isPinched2 = new ShortValueHistory<bool>(false);
            PinchReported += (f, b) =>
            {
                isPinched2.UpdateValue(b);

                if (isPinched2.Previous)
                {
                    if (isPinched2.Current)
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
                    if (isPinched2.Current)
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
}
