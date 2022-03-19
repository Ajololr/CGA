using System.Drawing;
using System.Numerics;

namespace lab_1
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float Z { get; set; }
        public Vector3 Normal { get; set; }
        
        public Point(int x, int y, float z, Vector3 normal)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Normal = normal;
        }
    }
}