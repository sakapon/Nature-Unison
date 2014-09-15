using KLibrary.ComponentModel;
using Leap;
using System;
using System.ComponentModel;

namespace NatureUnison.Platforms.Leap
{
    public class LeapContext : NotifyBase, IDisposable
    {
        static Lazy<LeapContext> current = new Lazy<LeapContext>(() => new LeapContext());

        public static LeapContext Current
        {
            get { return current.Value; }
        }

        Controller controller;
        FrameListener listener;

        [DefaultValue(true)]
        public bool IsAutoDisposing
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public bool IsConnected
        {
            get { return GetValue<bool>(); }
            private set { SetValue(value); }
        }

        public bool AllowBackground
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        public event Action<Frame> FrameArrived
        {
            add { listener.FrameArrived += value; }
            remove { listener.FrameArrived -= value; }
        }

        LeapContext()
        {
            controller = new Controller();
            listener = new FrameListener();
            listener.ConnectionChanged += b => IsConnected = b;
            controller.AddListener(listener);

            AddPropertyChangedHandler("AllowBackground", () =>
            {
                controller.SetPolicyFlags(AllowBackground ? Controller.PolicyFlag.POLICYBACKGROUNDFRAMES : Controller.PolicyFlag.POLICYDEFAULT);
            });
            AllowBackground = true;

            AppDomain.CurrentDomain.ProcessExit += (o, e) =>
            {
                if (IsAutoDisposing) Dispose();
            };
        }

        #region IDisposable members

        ~LeapContext()
        {
            Dispose(false);
        }

        public void Dispose()
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

        #endregion
    }
}
