﻿using System.Numerics;

 namespace Viewer3D.Models
{
    internal static class ViewPortMatrix4x4
    {
        public static Matrix4x4 Create(int width, int height, int xMin, int yMin)
        {
            return new Matrix4x4
            {
                M11 = width / 2,
                M12 = 0,
                M13 = 0,
                M14 = xMin + width / 2,

                M21 = 0,
                M22 = -height / 2,
                M23 = 0,
                M24 = yMin + height / 2,

                M31 = 0,
                M32 = 0,
                M33 = 1,
                M34 = 0,

                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 1,
            };
        }
    }
}
