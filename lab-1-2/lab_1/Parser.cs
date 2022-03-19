using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace lab_1
{
    public class Parser
    {
        public Model ParseFileToModel(string filePath)
        {
            var lines = File.ReadAllLines(filePath);

            var vertex = new List<Vector4>();
            var poligons = new List<(int v, int vt, int vn)[]>();
            var normalVectors = new List<Vector3>();

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
                    var coords = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Skip(1)
                        .Select(c => c.Split('/'))
                        .Select(c => (v : Int32.Parse(c[0]) - 1, vt : Int32.Parse(c[1]) - 1, vn : Int32.Parse(c[2]) - 1))
                        .ToArray();

                    poligons.Add(coords);
                }
            }

            return new Model { vertices =  vertex, polygons = poligons, normals = normalVectors };
        }
    }
}
