using KLibrary.ComponentModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace NatureUnison.Platforms.Kinect
{
    public class SkeletonTracker : NotifyBase
    {
        public KinectContext Context { get; private set; }

        Lazy<Skeleton[]> skeletons;

        Skeleton[] Skeletons
        {
            get { return skeletons.Value; }
        }

        public Point3D? HeadPosition
        {
            get { return GetValue<Point3D?>(); }
            private set { SetValue(value); }
        }

        public Point3D? HandRightPosition
        {
            get { return GetValue<Point3D?>(); }
            private set { SetValue(value); }
        }

        public Vector3D? HandRightDirection
        {
            get { return GetValue<Vector3D?>(); }
            private set { SetValue(value); }
        }

        public Quaternion? HandRightOrientation
        {
            get { return GetValue<Quaternion?>(); }
            private set { SetValue(value); }
        }

        public SkeletonTracker()
        {
            Context = KinectContext.Current;
            skeletons = new Lazy<Skeleton[]>(() => new Skeleton[Context.SensorChooser.Kinect.SkeletonStream.FrameSkeletonArrayLength]);
            Context.SkeletonFrameArrived += Context_SkeletonFrameArrived;
        }

        void Context_SkeletonFrameArrived(SkeletonFrame frame)
        {
            if (frame == null)
            {
                Clear();
                return;
            }

            frame.CopySkeletonDataTo(Skeletons);

            var skeleton = Skeletons
                .Where(s => s.TrackingState == SkeletonTrackingState.Tracked)
                .OrderBy(s => s.Position.Z)
                .FirstOrDefault();

            if (skeleton == null)
            {
                Clear();
                return;
            }

            var head = skeleton.Joints[JointType.Head];
            var handRight = skeleton.Joints[JointType.HandRight];
            var elbowRight = skeleton.Joints[JointType.ElbowRight];

            HeadPosition = head.TrackingState == JointTrackingState.Tracked ? head.Position.ToPoint3D() : default(Point3D?);
            HandRightPosition = handRight.TrackingState == JointTrackingState.Tracked ? handRight.Position.ToPoint3D() : default(Point3D?);

            if (handRight.TrackingState == JointTrackingState.Tracked && elbowRight.TrackingState == JointTrackingState.Tracked)
            {
                var elbowRightPosition = elbowRight.Position.ToPoint3D();
                var d = HandRightPosition.Value - elbowRightPosition;
                d.Normalize();
                HandRightDirection = d;

                var handRightOrientation = skeleton.BoneOrientations[JointType.HandRight];
                HandRightOrientation = handRightOrientation.HierarchicalRotation.Quaternion.ToQuaternion();
            }
            else
            {
                HandRightDirection = null;
                HandRightOrientation = null;
            }
        }

        void Clear()
        {
            HeadPosition = null;
            HandRightPosition = null;
            HandRightDirection = null;
            HandRightOrientation = null;
        }
    }
}
