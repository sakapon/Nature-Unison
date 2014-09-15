using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace KinectTestWpf
{
    public class VectorStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Point3D ? ToDisplay((Point3D)value)
                : value is Vector3D ? ToDisplay((Vector3D)value)
                : value is Quaternion ? ToDisplay2((Quaternion)value)
                : "---";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        static string ToDisplay(Point3D p)
        {
            return string.Format("({0:N0}, {1:N0}, {2:N0})", p.X, p.Y, p.Z);
        }

        static string ToDisplay(Vector3D v)
        {
            return string.Format("({0:N3}, {1:N3}, {2:N3})", v.X, v.Y, v.Z);
        }

        static string ToDisplay(Quaternion q)
        {
            return string.Format("({0:N3}, {1:N3}, {2:N3}, {3:N3})", q.X, q.Y, q.Z, q.W);
        }

        static string ToDisplay2(Quaternion q)
        {
            return string.Format("Axis: {0}, Angle: {1:N1} °", ToDisplay(q.Axis), q.Angle);
        }
    }
}
