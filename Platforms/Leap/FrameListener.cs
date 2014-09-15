using Leap;
using System;

namespace NatureUnison.Platforms.Leap
{
    class FrameListener : Listener
    {
        public event Action<Frame> FrameArrived;
        public event Action<bool> ConnectionChanged = b => { };

        public override void OnFrame(Controller controller)
        {
            var h = FrameArrived;
            if (h != null)
            {
                using (var frame = controller.Frame())
                {
                    h(frame);
                }
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
    }
}
