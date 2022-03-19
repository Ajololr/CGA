using System.Numerics;

namespace Viewer3D.Models
{
    public class ObjFileData
    {
        public Vector4[] Vertices { get; set; }

        public Vector4[] TextureVertices { get; set; }

        public Vector4[] NormalVectors { get; set; }

        public Polygon[] Polygons { get; set; }
    }
}
