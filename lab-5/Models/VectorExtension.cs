using System.Numerics;

namespace Viewer3D.Models
{
    public static class VectorExtensions
    {
        public static Vector3 CrossProduct(this Vector3 vector, Vector3 secondVector)
            => new Vector3
            {
                X = vector.Y * secondVector.Z - vector.Z * secondVector.Y,
                Y = vector.Z * secondVector.X - vector.X * secondVector.Z,
                Z = vector.X * secondVector.Y - vector.Y * secondVector.X,
            };

        public static Matrix4x4 GetViewMatrix4x4(this Vector3 eye, Vector3 target)
        {
            var distance = eye - target;
            var zAxis = Vector3.Normalize(distance);
            var xAxis = Vector3.Normalize(Vector3.UnitY.CrossProduct(zAxis));
            var yAxis = Vector3.Normalize(zAxis.CrossProduct(xAxis));

            return new Matrix4x4
            {
                M11 = xAxis.X,
                M12 = yAxis.X,
                M13 = zAxis.X,
                M14 = 0,

                M21 = xAxis.Y,
                M22 = yAxis.Y,
                M23 = zAxis.Y,
                M24 = 0,

                M31 = xAxis.Z,
                M32 = yAxis.Z,
                M33 = zAxis.Z,
                M34 = 0,

                M41 = -(Vector3.Dot(xAxis, distance)),
                M42 = -(Vector3.Dot(yAxis, distance)),
                M43 = -(Vector3.Dot(zAxis, distance)),
                M44 = 1,
            };
        }

        public static void SwapPoints(ref Vector4 vector1, ref Vector4 vector2)
        {
            (vector1.X, vector1.Y, vector1.Z, vector1.W, vector2.X, vector2.Y, vector2.Z, vector2.W)
                = (vector2.X, vector2.Y, vector2.Z, vector2.W, vector1.X, vector1.Y, vector1.Z, vector1.W);
        }
        
        public static Vector4 Clone(this Vector4 vector)
        {
            return new Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        public static Vector3 ConvertToVector3(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}