using System.Numerics;

namespace ObjParser
{
    public class WavefrontObject
    {
        public ObjGroup[] Groups { get; }
        public ObjMaterial[] Materials { get; }
        public Vector4[] Positions { get; }
        public Vector3[] Normals { get; }
        public Vector2[] Textures { get; }
        public WavefrontObject(in ObjGroup[] groups, in ObjMaterial[] materials, in Vector4[] positions, in Vector3[] normals, in Vector2[] textures)
        {
            Groups = groups;
            Materials = materials;
            Positions = positions;
            Normals = normals;
            Textures = textures;
        }
    }
}