using System.Numerics;

namespace ObjParser
{
    public readonly struct ObjColor
    {
        public readonly float Red;
        public readonly float Green;
        public readonly float Blue;
        public readonly Vector4 vecColor;
        public ObjColor(float r, float g, float b)
        {
            Red = r;
            Blue = b;
            Green = g;
            vecColor = new Vector4(r, g,  b, 1);
        }
    }
}