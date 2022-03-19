using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace lab_1
{
    public class ModelProps
    {
        public float CameraX { get; set; }
        public float CameraY { get; set; }
        public float CameraZ { get; set; }
        public float CameraXRotation { get; set; }
        public float CameraYRotation { get; set; }
        public float CameraZRotation { get; set; }
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }
        public Vector3 Light { get; set; }
        public float LightLevel { get; set; }
        public float Temp { get; set; }
    }
    
    public class Model : ICloneable
    {
        public List<Vector4> vertices;
        public List<(int v, int vt, int vn)[]> polygons;
        public List<Vector3> normals;
        public List<Vector3> textures;
        public Bitmap NormalsTexture { get; set; }
        public Bitmap DiffuseTexture { get; set; }
        public Bitmap SpecularTexture { get; set; }


        public object Clone()
        {
            return new Model()
            {
                vertices = new List<Vector4>(vertices),
                polygons = new List<(int v, int vt, int vn)[]>(polygons),
                normals = new List<Vector3>(normals),
                textures = textures,
                NormalsTexture = NormalsTexture,
                DiffuseTexture = DiffuseTexture,
                SpecularTexture = SpecularTexture,
            };
        }
    }
}
