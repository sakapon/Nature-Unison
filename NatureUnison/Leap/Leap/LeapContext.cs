using KLibrary.ComponentModel;
using Leap;
using System;
using System.Windows.Media.Media3D;

namespace NatureUnison.Leap
{
    public class LeapContext : NotifyBase, IDisposable
    {
        public static LeapContext Current { get; private set; }

        static LeapContext()
        {
            Current = new LeapContext();
            AppDomain.CurrentDomain.ProcessExit += (o, e) =>
            {
                ((IDisposable)Current).Dispose();
            };
        }

        public bool AllowBackground
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public bool IsConnected
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public LeapSettings Settings { get; private set; }

        Controller controller;
        FrameListener listener;

        public event Action<Frame> FrameUpdated
        {
            add { listener.FrameUpdated += value; }
            remove { listener.FrameUpdated -= value; }
        }

        LeapContext()
        {
            Settings = new LeapSettings();

            controller = new Controller();
            listener = new FrameListener();
            listener.ConnectionChanged += b => IsConnected = b;
            controller.AddListener(listener);

            AddPropertyChangedHandler("AllowBackground", () =>
            {
                controller.SetPolicyFlags(AllowBackground ? Controller.PolicyFlag.POLICYBACKGROUNDFRAMES : Controller.PolicyFlag.POLICYDEFAULT);
            });
            AllowBackground = true;
        }

        ~LeapContext()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                controller.RemoveListener(listener);
                listener.Dispose();
                controller.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }

    public class LeapSettings
    {
        public Vector3D ScreenCenter { get; set; }
        public Vector3D VirtualScreenCenter { get; set; }
        public double AngleDegrees { get; set; }
        public double PixelsPerMillimeter { get; set; }

        // TODO: .config file.
        public LeapSettings()
        {
            ScreenCenter = new Vector3D(0, 300, 0);
            VirtualScreenCenter = new Vector3D(960, 540, 0);
            AngleDegrees = 0;
            PixelsPerMillimeter = 5;
        }
    }
}
