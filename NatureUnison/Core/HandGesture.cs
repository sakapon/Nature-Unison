using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using PalmFrame = System.Tuple<System.DateTime, System.Windows.Media.Media3D.Point3D>;

namespace NatureUnison
{
    public class HandGesture
    {
        const int MaxFingersHistoryCount = 50;
        const int MaxGrippedPalmHistoryCount = 3;
        const int DefaultPushDepth = 100;
        const double PushReboundRate = 0.8;
        const double DefaultMaxInertialVelocity = 4000;
        const double DefaultMinInertialVelocity = 2000;

        public event Action<HandFrame?, bool> GripReported = (f, b) => { };
        //public event Action<HandFrame?, bool> GripChanged = (f, b) => { };

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
        /// Gets or sets the upper bound of inertial velocity when the <see cref="Dropped"/> event occurs.
        /// </summary>
        /// <value>The upper bound of inertial velocity when the <see cref="Dropped"/> event occurs.</value>
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
        /// Gets or sets the lower bound of inertial velocity when the <see cref="Dropped"/> event occurs.
        /// </summary>
        /// <value>The lower bound of inertial velocity when the <see cref="Dropped"/> event occurs.</value>
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
        /// Initializes a new instance of the <see cref="HandGesture"/> class.
        /// </summary>
        public HandGesture()
        {
            PushDepth = DefaultPushDepth;
            MaxInertialVelocity = DefaultMaxInertialVelocity;
            MinInertialVelocity = DefaultMinInertialVelocity;

            var fingersCount = new ValueHistory<int?>(MaxFingersHistoryCount);
            var isGripped = false;
            HandsContext.Current.SingleHandFrame += f =>
            {
                fingersCount.UpdateValue(f.HasValue ? (int?)f.Value.Fingers.Length : null);

                var currentFingersCount = fingersCount.History.Last();
                if (!currentFingersCount.HasValue || currentFingersCount > 1)
                {
                    isGripped = false;
                }
                else if (!isGripped && fingersCount.History.Reverse().TakeWhile(i => i.HasValue).Any(i => i >= 4))
                {
                    isGripped = true;
                }

                GripReported(f, isGripped);
            };

            var dragStartedFrame = default(HandFrame);
            var isGripped2 = new ShortValueHistory<bool>(false);
            var grippedPalmHistory = new ValueHistory<PalmFrame>(MaxGrippedPalmHistoryCount);
            GripReported += (f, b) =>
            {
                isGripped2.UpdateValue(b);

                if (isGripped2.Previous)
                {
                    if (isGripped2.Current)
                    {
                        grippedPalmHistory.UpdateValue(Tuple.Create(DateTime.Now, f.Value.PalmPosition));
                        Dragged(f, f.Value.PalmPosition - dragStartedFrame.PalmPosition);
                    }
                    else
                    {
                        if (f.HasValue)
                        {
                            Dropped(f, f.Value.PalmPosition - dragStartedFrame.PalmPosition, ToInertialVelocity(grippedPalmHistory));
                        }
                        else
                        {
                            DragCancelled(f);
                        }
                    }
                }
                else
                {
                    if (isGripped2.Current)
                    {
                        dragStartedFrame = f.Value;
                        DragStarted(f);
                    }
                }
            };

            HandsContext.Current.SingleHandFrame += f =>
            {
                HoldUpReported(f, f.HasValue && f.Value.Fingers.Length >= 4 && IsHandUpward(f.Value) && IsPalmForward(f.Value));
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

        static bool IsHandUpward(HandFrame h)
        {
            // Map to plane x = 0 and determine if being in (135, 225) in degrees on y-z system.
            var d = h.Direction;
            var dTan = d.Z / d.Y;
            return d.Y < 0 && -1 < dTan && dTan < 1;
        }

        static bool IsPalmForward(HandFrame h)
        {
            // Map to plane x = 0 and determine if being in (225, 315) in degrees on y-z system.
            var d = h.PalmDirection;
            var dTan = d.Y / d.Z;
            return d.Z < 0 && -1 < dTan && dTan < 1;
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
