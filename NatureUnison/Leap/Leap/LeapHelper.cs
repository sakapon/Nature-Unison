using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace NatureUnison.Leap
{
    public static class LeapHelper
    {
        internal static Vector3D ToVector3D(this Vector v)
        {
            if (v == null) return default(Vector3D);
            return new Vector3D(v.x, v.y, v.z);
        }

        internal static Point3D ToPoint3D(this Vector v)
        {
            if (v == null) return default(Point3D);
            return new Point3D(v.x, v.y, v.z);
        }

        /// <summary>
        /// Leap Motion Controller の座標系におけるベクトルをスクリーンの座標系に変換します。
        /// </summary>
        /// <param name="p">Leap Motion Controller の座標系における座標。</param>
        /// <returns>スクリーンの座標系における座標。</returns>
        public static Point3D ToScreenPosition(this Point3D p)
        {
            var m = new Matrix3D();
            m.Translate(-LeapContext.Current.Settings.ScreenCenter);
            m.Rotate(new Quaternion(new Vector3D(1, 0, 0), LeapContext.Current.Settings.AngleDegrees));
            var scale = LeapContext.Current.Settings.PixelsPerMillimeter;
            m.Scale(new Vector3D(scale, -scale, scale));
            m.Translate(LeapContext.Current.Settings.VirtualScreenCenter);

            return (p * m).Floor();
        }

        public static Point3D Floor(this Point3D p)
        {
            return new Point3D(Math.Floor(p.X), Math.Floor(p.Y), Math.Floor(p.Z));
        }
    }
}
