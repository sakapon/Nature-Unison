using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace NatureUnison
{
    public class HandGesture
    {
        const int MaxFingersHistoryCount = 50;
        const int DefaultPushDepth = 500;

        public event Action<HandFrame?, bool> GripReported = (f, b) => { };
        //public event Action<HandFrame?, bool> GripChanged = (f, b) => { };

        public event Action<HandFrame?> DragStarted = f => { };
        public event Action<HandFrame?, Vector3D> Dragged = (f, v) => { };
        public event Action<HandFrame?, Vector3D?> Dropped = (f, v) => { };

        public event Action<HandFrame?, bool> HoldUpReported = (f, b) => { };
        //public event Action<HandFrame?, bool> HoldUpChanged = (f, b) => { };

        public event Action<HandFrame?> PushStarted = f => { };
        public event Action<HandFrame?, double> PushProgress = (f, v) => { };
        public event Action<HandFrame?> Pushed = f => { };
        public event Action<HandFrame?> PushCancelled = f => { };

        int pushDepth;

        public int PushDepth
        {
            get { return pushDepth; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value", value, "The value must be positive.");
                pushDepth = value;
            }
        }

        public HandGesture()
        {
            PushDepth = DefaultPushDepth;

            var fingersCount = new ValueHistory<int?>(MaxFingersHistoryCount);
            var isGripped = false;
            HandsContext.Current.SingleHandFrame += f =>
            {
                fingersCount.UpdateValue(f.HasValue ? (int?)f.Value.Fingers.Length : null);

                if (fingersCount.History.Last() != 0)
                {
                    isGripped = false;
                }
                else if (!isGripped && fingersCount.History.Reverse().TakeWhile(i => i.HasValue).Contains(5))
                {
                    isGripped = true;
                }

                GripReported(f, isGripped);
            };

            var dragStartedFrame = default(HandFrame);
            var isGripped2 = new ShortValueHistory<bool>(false);
            GripReported += (f, b) =>
            {
                isGripped2.UpdateValue(b);

                if (isGripped2.Previous)
                {
                    if (isGripped2.Current)
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
                    if (isGripped2.Current)
                    {
                        dragStartedFrame = f.Value;
                        DragStarted(f);
                    }
                }
            };

            HandsContext.Current.SingleHandFrame += f =>
            {
                HoldUpReported(f, f.HasValue && IsHandUpward(f.Value) && IsPalmForward(f.Value));
            };
        }

        static bool IsHandUpward(HandFrame h)
        {
            // Map to plane x = 0 and determine if being in (135, 225) in degrees on y-z system.
            var handDirection = h.Direction;
            var handDirectionTan = handDirection.Z / handDirection.Y;
            return handDirection.Y < 0 && -1 < handDirectionTan && handDirectionTan < 1;
        }

        static bool IsPalmForward(HandFrame h)
        {
            // Map to plane x = 0 and determine if being in (225, 315) in degrees on y-z system.
            var palmDirection = h.PalmDirection;
            var palmDirectionTan = palmDirection.Y / palmDirection.Z;
            return palmDirection.Z < 0 && -1 < palmDirectionTan && palmDirectionTan < 1;
        }
    }
}
