using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using PalmFrame = System.Tuple<System.DateTime, System.Windows.Media.Media3D.Point3D>;

namespace NatureUnison
{
    public class FingerGesture
    {
        const double MaxPinchDistance = 200;
        const int MaxPinchedPalmHistoryCount = 3;
        const int DefaultPushDepth = 100;
        const double PushReboundRate = 0.8;
        const double DefaultMaxInertialVelocity = 4000;
        const double DefaultMinInertialVelocity = 1500;

        public event Action<HandFrame?, double?> TwoFingersDistance = (f, d) => { };
        public event Action<HandFrame?, bool> PinchReported = (f, b) => { };
        //public event Action<HandFrame?, bool> PinchChanged = (f, b) => { };

        public event Action<HandFrame?> DragStarted = f => { };
        public event Action<HandFrame?, Vector3D> Dragged = (f, d) => { };
        public event Action<HandFrame?, Vector3D, Vector3D?> Dropped = (f, d, v) => { };
        public event Action<HandFrame?> DragCancelled = f => { };

        public event Action<HandFrame?, bool> HoldUpReported = (f, b) => { };
        public event Action<HandFrame?, bool> HoldUpChanged = (f, b) => { };

        public event Action<HandFrame?> PushStarted = f => { };
        public event Action<HandFrame?, double> PushProgress = (f, v) => { };
        public event Action<HandFrame?> Pushed = f => { };
        public event Action<HandFrame?> PushCancelled = f => { };

        int _PushDepth;

        /// <summary>
        /// Gets or sets the depth to push down to raise the <see cref="Pushed"/> event.
        /// </summary>
        /// <value>The depth to push down to raise the <see cref="Pushed"/> event.</value>
        public int PushDepth
        {
            get { return _PushDepth; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value", value, "The value must be positive.");
                _PushDepth = value;
            }
        }

        double _MaxInertialVelocity;

        /// <summary>
        /// Gets or sets the upper bound of inertial velocity of the hand when the <see cref="Dropped"/> event occurs.
        /// </summary>
        /// <value>The upper bound of inertial velocity of the hand when the <see cref="Dropped"/> event occurs.</value>
        public double MaxInertialVelocity
        {
            get { return _MaxInertialVelocity; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value", value, "The value must be positive.");
                _MaxInertialVelocity = value;
            }
        }

        double _MinInertialVelocity;

        /// <summary>
        /// Gets or sets the lower bound of inertial velocity of the hand when the <see cref="Dropped"/> event occurs.
        /// </summary>
        /// <value>The lower bound of inertial velocity of the hand when the <see cref="Dropped"/> event occurs.</value>
        public double MinInertialVelocity
        {
            get { return _MinInertialVelocity; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value", value, "The value must be positive.");
                _MinInertialVelocity = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FingerGesture"/> class.
        /// </summary>
        public FingerGesture()
        {
            PushDepth = DefaultPushDepth;
            MaxInertialVelocity = DefaultMaxInertialVelocity;
            MinInertialVelocity = DefaultMinInertialVelocity;

            HandsContext.Current.SingleHandFrame += f =>
            {
                if (!f.HasValue)
                {
                    TwoFingersDistance(f, null);
                    return;
                }

                var frontFingers = f.Value.Fingers
                    .Where(_f => !double.IsNaN(_f.TipPosition.Z))
                    .OrderBy(_f => _f.TipPosition.Z)
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
            var pinchedPalmHistory = new ValueHistory<PalmFrame>(MaxPinchedPalmHistoryCount);
            PinchReported += (f, b) =>
            {
                isPinched2.UpdateValue(b);

                if (isPinched2.Previous)
                {
                    if (isPinched2.Current)
                    {
                        pinchedPalmHistory.UpdateValue(Tuple.Create(DateTime.Now, f.Value.PalmPosition));
                        Dragged(f, f.Value.PalmPosition - dragStartedFrame.PalmPosition);
                    }
                    else
                    {
                        if (f.HasValue)
                        {
                            Dropped(f, f.Value.PalmPosition - dragStartedFrame.PalmPosition, ToInertialVelocity(pinchedPalmHistory));
                        }
                        else
                        {
                            DragCancelled(f);
                        }
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
                HoldUpReported(f, f.HasValue && f.Value.Fingers.Length == 1 && IsFingerUpward(f.Value) && IsPalmForward(f.Value));
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

                if (isPinched) return;

                if (isHeldUp.Current)
                {
                    pushProgress = (pushStartedFrame.FrontmostFinger.Value.TipPosition.Z - f.Value.FrontmostFinger.Value.TipPosition.Z) / PushDepth;

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
                            var finger = pushStartedFrame.FrontmostFinger.Value;
                            finger.TipPosition.Z = f.Value.FrontmostFinger.Value.TipPosition.Z + PushDepth;
                            pushStartedFrame.FrontmostFinger = finger;
                        }
                    }
                }
            };

            HoldUpChanged += (f, b) =>
            {
                if (b && !isPinched)
                {
                    isPushStarted = true;
                    pushStartedFrame = f.Value;
                    PushStarted(f);
                }
                else if (!b && isPushStarted)
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

            // Map to plane x = 0 and determine if being in (180, 315) in degrees on y-z system.
            var d = h.FrontmostFinger.Value.Direction;
            var dAtan = Math.Atan2(d.Z, d.Y);
            return -Math.PI < dAtan && dAtan < -Math.PI / 4;
        }

        static bool IsPalmForward(HandFrame h)
        {
            // Map to plane x = 0 and determine if being in (270, 45) in degrees on y-z system.
            var d = h.PalmDirection;
            var dAtan = Math.Atan2(d.Z, d.Y);
            return -Math.PI / 2 < dAtan && dAtan < Math.PI / 4;
        }

        Vector3D? ToInertialVelocity(ValueHistory<PalmFrame> palmHistory)
        {
            if (!palmHistory.IsFull) return null;

            var firstFrame = palmHistory.History.First();
            var lastFrame = palmHistory.History.Last();

            var v = (lastFrame.Item2 - firstFrame.Item2) / (lastFrame.Item1 - firstFrame.Item1).TotalSeconds;
            return
                v.Length < MinInertialVelocity ? default(Vector3D?) :
                v.Length <= MaxInertialVelocity ? v :
                MaxInertialVelocity * v / v.Length;
        }
    }
}
