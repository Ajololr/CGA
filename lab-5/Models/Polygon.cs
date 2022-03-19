using ObjParser;

namespace Viewer3D.Models
{
    public class Polygon
    {
        public Polygon(int minVertexCount = 3)
        {
            VertexIndices = new int[minVertexCount];
            TextureIndices = new int[minVertexCount];
            NormalIndices = new int[minVertexCount];
        }
        public Polygon(ObjFace data)
        {
            Material = data.Material;
            SmoothGroup = data.SmoothGroup;
            VertexIndices = new int[data.Vertices.Length];
            TextureIndices = new int[data.Vertices.Length];
            NormalIndices = new int[data.Vertices.Length];
            for (var i = 0; i < data.Vertices.Length; i++)
            {
                VertexIndices[i] = data.Vertices[i].VertexIndex;
                TextureIndices[i] = data.Vertices[i].TextureIndex;
                NormalIndices[i] = data.Vertices[i].NormalIndex;
            }
            
        }
        public int[] VertexIndices { get; set; }

        public int[] TextureIndices { get; set; }

        public int[] NormalIndices { get; set; }
        public int Material { get; set; }
        public int SmoothGroup { get; set; }
    }
}
