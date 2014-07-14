using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace NatureUnison
{
    public class HandGesture
    {
        const int MaxFingersHistoryCount = 50;

        public event Action<HandFrame?, bool> GripReported = (f, b) => { };
        //public event Action<HandFrame?, bool> GripChanged = (f, b) => { };

        public event Action<HandFrame?> DragStarted = f => { };
        public event Action<HandFrame?, Vector3D> Dragged = (f, v) => { };
        public event Action<HandFrame?, Vector3D?> Dropped = (f, v) => { };

        public HandGesture()
        {
            var fingersCount = new ValueHistory<int?>(MaxFingersHistoryCount);
            var isGripped = false;
            HandsContext.Current.SingleHandFrame += f =>
            {
                fingersCount.UpdateValue(f.HasValue ? (int?)f.Value.Fingers.Length : null);

                if (!fingersCount.IsFull || !fingersCount.History.Last().HasValue || fingersCount.History.Last().Value > 0)
                {
                    isGripped = false;
                }
                else if (!isGripped && fingersCount.History.All(i => i.HasValue) && fingersCount.History.Contains(5))
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
        }
    }
}
