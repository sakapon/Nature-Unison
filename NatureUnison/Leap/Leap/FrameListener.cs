﻿using Leap;
using System;

namespace NatureUnison.Leap
{
    class FrameListener : Listener
    {
        public event Action<Frame> FrameUpdated = f => { };
        public event Action<bool> ConnectionChanged = b => { };
        public event Action<bool> FocusChanged = b => { };

        public override void OnFrame(Controller controller)
        {
            using (var frame = controller.Frame())
            {
                FrameUpdated(frame);
            }
        }

        public override void OnConnect(Controller controller)
        {
            ConnectionChanged(true);
        }

        public override void OnDisconnect(Controller controller)
        {
            ConnectionChanged(false);
        }

        public override void OnFocusGained(Controller controller)
        {
            FocusChanged(true);
        }

        public override void OnFocusLost(Controller controller)
        {
            FocusChanged(false);
        }
    }
}
