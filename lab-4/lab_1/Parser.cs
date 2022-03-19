using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace lab_1
{
    public class Parser
    {
        private const string NormalPath = "normal.png";
        private const string DiffusePath = "albedo.png";
        private const string SpecularPath = "specular.png";
        public Model ParseFileToModel(string filePath)
        {
            var lines = File.ReadAllLines(filePath + "model.obj");

            var vertex = new List<Vector4>();
            var poligons = new List<(int v, int vt, int vn)[]>();
            var normalVectors = new List<Vector3>();
            var texturevVertex = new List<Vector3>();

            foreach (var line in lines)
            {
                if (line.StartsWith("v "))
                {
                    var coords = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Skip(1)
                        .Select(c => float.TryParse(c, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0)
                        .ToArray();

                    vertex.Add(new Vector4(coords[0], coords[1], coords[2], coords.Length > 3 ? coords[3] : 1));
                }
                if (line.StartsWith("vn"))
                {
                    var coords = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Skip(1)
                        .Select(c => float.TryParse(c, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0)
                        .ToArray();

                    normalVectors.Add(new Vector3(coords[0], coords[1], coords[2]));
                }
                if (line.StartsWith("f "))
                {
                    var coords = line.Split(' ')
                            .Skip(1)
                            .Select(c => c.Split('/'))
                            .Select(c => (v: Int32.Parse(c[0]) - 1, vt: Int32.Parse(c[1]) - 1,
                                vn: 0))
                            .ToArray();

                        poligons.Add(coords);
                }
                if (line.StartsWith("vt"))
                {
                    var coords = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Skip(1)
                        .Select(c => float.TryParse(c, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0)
                        .ToArray();

                    texturevVertex.Add(new Vector3(coords[0], coords[1],  0));
                    continue;
                }
            }
            
            var normalMap = Path.Combine(filePath, NormalPath);
            var diffuseMap = Path.Combine(filePath, DiffusePath);
            var specularMap = Path.Combine(filePath, SpecularPath);

            return new Model { vertices =  vertex, polygons = poligons, normals = normalVectors, textures = texturevVertex, NormalsTexture = new Bitmap(normalMap), DiffuseTexture = new Bitmap(diffuseMap), SpecularTexture = new Bitmap(specularMap)};
        }
    }
}
