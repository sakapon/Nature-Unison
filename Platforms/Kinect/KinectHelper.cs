using Microsoft.Kinect;
using System;
using System.Windows.Media.Media3D;

namespace NatureUnison.Platforms.Kinect
{
    public static class KinectHelper
    {
        public static Point3D ToPoint3D(this SkeletonPoint p)
        {
            return new Point3D(p.X, p.Y, p.Z);
        }

        public static Quaternion ToQuaternion(this Vector4 v)
        {
            return new Quaternion(v.X, v.Y, v.Z, v.W);
        }
    }
}
