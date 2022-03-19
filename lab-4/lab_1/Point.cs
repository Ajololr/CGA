using System.Drawing;
using System.Numerics;

namespace lab_1
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
        public Vector3 Normal { get; set; }
        public Vector3 Texel { get; set; }
        
        public Point(int x, int y, float z, float w, Vector3 texel)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
            this.Texel = texel;
        }
        
        public Point(int x, int y, float z, float w, Vector3 normal, Vector3 texel)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
            this.Normal = normal;
            this.Texel = texel;
        }
    }
}