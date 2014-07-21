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

        const int InvalidHandId = -1;
        int handId = InvalidHandId;

        void Context_FrameUpdated(Frame f)
        {
            var h = f.Hand(handId);
            if (h == null || !h.IsValid)
            {
                h = f.Hands.Frontmost;
            }

            if (h == null || !h.IsValid)
            {
                handId = InvalidHandId;
                NotifySingleHandFrame(null);
            }
            else
            {
                handId = h.Id;
                NotifySingleHandFrame(h.ToHandFrame());
            }
        }
    }
}
