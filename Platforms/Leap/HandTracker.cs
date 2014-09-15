using KLibrary.ComponentModel;
using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace NatureUnison.Platforms.Leap
{
    public class HandTracker : NotifyBase
    {
        public LeapContext Context { get; private set; }

        public Point3D? PalmPosition
        {
            get { return GetValue<Point3D?>(); }
            private set { SetValue(value); }
        }

        public Point3D? FingerTipPosition
        {
            get { return GetValue<Point3D?>(); }
            private set { SetValue(value); }
        }

        public Vector3D? FingerDirection
        {
            get { return GetValue<Vector3D?>(); }
            private set { SetValue(value); }
        }

        public Quaternion? FingerOrientation
        {
            get { return GetValue<Quaternion?>(); }
            private set { SetValue(value); }
        }

        public HandTracker()
        {
            Context = LeapContext.Current;
            Context.FrameArrived += Context_FrameArrived;
        }

        void Context_FrameArrived(Frame frame)
        {
            var hand = frame.Hands.Frontmost;
            if (hand == null || !hand.IsValid)
            {
                Clear();
                return;
            }

            var p = hand.Pointables.Frontmost;
            if (p == null || !p.IsValid)
            {
                Clear();
                return;
            }

            PalmPosition = hand.StabilizedPalmPosition.ToPoint3DOrNull();
            FingerTipPosition = p.StabilizedTipPosition.ToPoint3DOrNull();
            FingerDirection = p.Direction.ToVector3DOrNull();
            // TODO: FingerOrientation
        }

        void Clear()
        {
            PalmPosition = null;
            FingerTipPosition = null;
            FingerDirection = null;
            FingerOrientation = null;
        }
    }
}
