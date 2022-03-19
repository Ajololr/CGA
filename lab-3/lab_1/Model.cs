using System;
using System.Collections.Generic;
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
    }
    
    public class Model : ICloneable
    {
        public List<Vector4> vertices;
        public List<(int v, int vt, int vn)[]> polygons;
        public List<Vector3> normals;

        public object Clone()
        {
            return new Model()
            {
                vertices = new List<Vector4>(vertices),
                polygons = new List<(int v, int vt, int vn)[]>(polygons),
                normals = new List<Vector3>(normals),
            };
        }
    }
}
