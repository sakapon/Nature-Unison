using KLibrary.ComponentModel;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
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

        public KinectSensorChooser SensorChooser { get; private set; }

        [DefaultValue(true)]
        public bool IsAutoDisposing
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        event Action<SkeletonFrame> skeletonFrameArrived;
        bool is_SkeletonFrameArrived_handled;
        public event Action<SkeletonFrame> SkeletonFrameArrived
        {
            add
            {
                is_SkeletonFrameArrived_handled = true;
                skeletonFrameArrived += value;
            }
            remove { skeletonFrameArrived -= value; }
        }

        KinectContext()
        {
            SensorChooser = new KinectSensorChooser();
            SensorChooser.KinectChanged += SensorChooser_KinectChanged;

            AppDomain.CurrentDomain.ProcessExit += (o, e) =>
            {
                if (IsAutoDisposing) Dispose();
            };
        }

        #region IDisposable members

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
                if (SensorChooser.Status == ChooserStatus.SensorStarted)
                {
                    SensorChooser.Stop();
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion

        public void Start()
        {
            SensorChooser.Start();
        }

        void SensorChooser_KinectChanged(object sender, KinectChangedEventArgs e)
        {
            if (e.OldSensor != null)
            {
                try
                {
                    if (is_SkeletonFrameArrived_handled)
                    {
                        e.OldSensor.SkeletonFrameReady -= Kinect_SkeletonFrameReady;
                        e.OldSensor.SkeletonStream.Disable();
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (e.NewSensor != null)
            {
                try
                {
                    if (is_SkeletonFrameArrived_handled)
                    {
                        e.NewSensor.SkeletonStream.Enable();

                        try
                        {
                            e.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                            e.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                        }
                        catch (InvalidOperationException)
                        {
                            // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                            e.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                            e.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                        }

                        e.NewSensor.SkeletonFrameReady += Kinect_SkeletonFrameReady;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var h = skeletonFrameArrived;
            if (h != null)
            {
                using (var frame = e.OpenSkeletonFrame())
                {
                    h(frame);
                }
            }
        }
    }
}
