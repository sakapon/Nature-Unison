using KLibrary.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NatureUnison.Platforms.Kinect
{
    public class KinectContext : NotifyBase, IDisposable
    {
        static Lazy<KinectContext> current = new Lazy<KinectContext>(() => new KinectContext());

        public static KinectContext Current
        {
            get { return current.Value; }
        }

        [DefaultValue(true)]
        public bool IsAutoDisposing
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        KinectContext()
        {
            AppDomain.CurrentDomain.ProcessExit += (o, e) =>
            {
                if (IsAutoDisposing) Dispose();
            };
        }

        ~KinectContext()
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
                // TODO: 
            }
            catch (Exception)
            {
            }
        }
    }
}
