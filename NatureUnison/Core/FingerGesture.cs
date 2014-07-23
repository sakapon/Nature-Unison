using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace NatureUnison
{
    public class FingerGesture
    {
        const double MaxPinchDistance = 200;
        const int DefaultPushDepth = 100;
        const double PushReboundRate = 0.8;

        public event Action<HandFrame?, double?> TwoFingersDistance = (f, d) => { };
        public event Action<HandFrame?, bool> PinchReported = (f, b) => { };
        //public event Action<HandFrame?, bool> PinchChanged = (f, b) => { };

        public event Action<HandFrame?> DragStarted = f => { };
        public event Action<HandFrame?, Vector3D> Dragged = (f, v) => { };
        public event Action<HandFrame?, Vector3D?> Dropped = (f, v) => { };

        public event Action<HandFrame?, bool> HoldUpReported = (f, b) => { };
        public event Action<HandFrame?, bool> HoldUpChanged = (f, b) => { };

        public event Action<HandFrame?> PushStarted = f => { };
        public event Action<HandFrame?, double> PushProgress = (f, v) => { };
        public event Action<HandFrame?> Pushed = f => { };
        public event Action<HandFrame?> PushCancelled = f => { };

        int pushDepth;

        /// <summary>
        /// Gets or sets the depth to push down to raise the <see cref="Pushed"/> event.
        /// </summary>
        /// <value>The depth to push down to raise the <see cref="Pushed"/> event.</value>
        public int PushDepth
        {
            get { return pushDepth; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value", value, "The value must be positive.");
                pushDepth = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FingerGesture"/> class.
        /// </summary>
        public FingerGesture()
        {
            PushDepth = DefaultPushDepth;

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

            HandsContext.Current.SingleHandFrame += f =>
            {
                HoldUpReported(f, f.HasValue && IsFingerUpward(f.Value) && IsPalmForward(f.Value));
            };

            var isHeldUp = new ShortValueHistory<bool>(false);
            var isPushStarted = false;
            var pushStartedFrame = default(HandFrame);
            var pushProgress = 0.0;
            HoldUpReported += (f, b) =>
            {
                isHeldUp.UpdateValue(b);

                if (isHeldUp.Previous != isHeldUp.Current)
                {
                    HoldUpChanged(f, b);
                }

                if (isHeldUp.Current)
                {
                    pushProgress = (pushStartedFrame.PalmPosition.Z - f.Value.PalmPosition.Z) / PushDepth;

                    if (isPushStarted)
                    {
                        if (pushProgress < 0.0)
                        {
                            pushStartedFrame = f.Value;
                            pushProgress = 0.0;
                        }
                        else if (pushProgress > 1.0)
                        {
                            pushProgress = 1.0;
                        }

                        PushProgress(f, pushProgress);
                        if (pushProgress == 1.0)
                        {
                            isPushStarted = false;
                            Pushed(f);
                        }
                    }
                    else
                    {
                        if (pushProgress < PushReboundRate)
                        {
                            isPushStarted = true;
                            PushStarted(f);
                        }
                        else if (pushProgress > 1.0)
                        {
                            pushStartedFrame.PalmPosition.Z = f.Value.PalmPosition.Z + PushDepth;
                        }
                    }
                }
            };

            HoldUpChanged += (f, b) =>
            {
                if (b)
                {
                    isPushStarted = true;
                    pushStartedFrame = f.Value;
                    PushStarted(f);
                }
                else if (isPushStarted)
                {
                    isPushStarted = false;
                    pushProgress = 0.0;
                    PushCancelled(f);
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

        static bool IsFingerUpward(HandFrame h)
        {
            if (!h.FrontmostFinger.HasValue) return false;

            // Map to plane x = 0 and determine if being in (180, 270) in degrees on y-z system.
            var d = h.FrontmostFinger.Value.Direction;
            return d.Y < 0 && d.Z < 0;
        }

        static bool IsPalmForward(HandFrame h)
        {
            // Map to plane x = 0 and determine if being in (270, 360) in degrees on y-z system.
            var d = h.PalmDirection;
            return d.Y > 0 && d.Z < 0;
        }
    }
}
