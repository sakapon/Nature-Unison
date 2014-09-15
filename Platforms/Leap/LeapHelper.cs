using Leap;
using System;
using System.Windows.Media.Media3D;

namespace NatureUnison.Platforms.Leap
{
    public static class LeapHelper
    {
        internal static Point3D ToPoint3D(this Vector v)
        {
            if (v == null || !v.IsValid()) throw new ArgumentNullException("v");
            return new Point3D(v.x, v.y, v.z);
        }

        internal static Point3D? ToPoint3DOrNull(this Vector v)
        {
            return v == null || !v.IsValid()
                ? default(Point3D?)
                : new Point3D(v.x, v.y, v.z);
        }

        internal static Vector3D ToVector3D(this Vector v)
        {
            if (v == null || !v.IsValid()) throw new ArgumentNullException("v");
            return new Vector3D(v.x, v.y, v.z);
        }

        internal static Vector3D? ToVector3DOrNull(this Vector v)
        {
            return v == null || !v.IsValid()
                ? default(Vector3D?)
                : new Vector3D(v.x, v.y, v.z);
        }
    }
}
