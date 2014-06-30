using Leap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NatureUnison.Leap
{
    public class LeapHandsTracker : HandsTracker
    {
        public LeapHandsTracker()
        {
            LeapContext.Current.FrameUpdated += Context_FrameUpdated;
        }

        void Context_FrameUpdated(Frame f)
        {
            var h = f.Hands.Frontmost;
            if (h != null && h.IsValid)
            {
                var hand = new HandFrame
                {
                    PalmPosition = h.PalmPosition.ToPoint3D().ToScreenPosition(),
                };

                NotifySingleHandFrame(hand);
            }
            else
            {
                NotifySingleHandFrame(null);
            }
        }
    }
}
