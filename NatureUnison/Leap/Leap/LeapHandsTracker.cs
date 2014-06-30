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

                var hand = new HandFrame
                {
                    PalmPosition = h.StabilizedPalmPosition.ToPoint3D().ToScreenPosition(),
                    Fingers = h.Pointables.Where(p => p.IsValid).Select(LeapHelper.ToFingerFrame).ToArray(),
                };

                var frontPointable = h.Pointables.Frontmost;
                if (frontPointable != null && frontPointable.IsValid)
                {
                    hand.FrontmostFinger = frontPointable.ToFingerFrame();
                }

                NotifySingleHandFrame(hand);
            }
        }
    }
}
