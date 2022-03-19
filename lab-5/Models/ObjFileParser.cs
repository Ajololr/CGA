using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Viewer3D.Models
{
    public static class ObjFileParser
    {
        public static ObjFileData GetObjectInfo(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var vertices = new List<Vector4>();
            var textureVertices = new List<Vector4>();
            var normalVectors = new List<Vector4>();
            var polygons = new List<Polygon>();
            var separatorArray = new[] {' '};

            foreach (var line in lines)
            {
                if (line.ToLower().StartsWith("vt"))
                {
                    AddToTextureVertices(line, separatorArray, ref textureVertices);

                    continue;
                }

                if (line.ToLower().StartsWith("vn"))
                {
                    AddToNormalVectors(line, separatorArray, ref normalVectors);

                    continue;
                }

                if (line.ToLower().StartsWith("v"))
                {
                    AddToVertices(line, separatorArray, ref vertices);

                    continue;
                }

                if (!line.ToLower().StartsWith("f"))
                {
                    continue;
                }

                AddToPolygons(line, separatorArray, vertices.Count, textureVertices.Count, normalVectors.Count, ref polygons);
            }

            return new ObjFileData
            {
                Vertices = vertices.ToArray(),
                TextureVertices = textureVertices.ToArray(),
                NormalVectors = normalVectors.ToArray(),
                Polygons = polygons.ToArray()
            };
        }

        private static void AddToTextureVertices(string line, char[] separatorArray, ref List<Vector4> textureVertices)
        {
            var textureVertex = GetLineValues(line, separatorArray)
                .Select(x => float.Parse(x, CultureInfo.InvariantCulture))
                .ToArray();

            switch (textureVertex.Length)
            {
                case 1:
                    textureVertices.Add(new Vector4(textureVertex[0], 1, 1, 1));
                    break;

                case 2:
                    textureVertices.Add(new Vector4(textureVertex[0], textureVertex[1], 1, 1));
                    break;

                case 3:
                    textureVertices.Add(new Vector4(textureVertex[0], textureVertex[1], textureVertex[2], 1));
                    break;
            }
        }

        private static void AddToNormalVectors(string line, char[] separatorArray, ref List<Vector4> normalVectors)
        {
            var normalVector = GetLineValues(line, separatorArray)
                .Select(x => float.Parse(x, CultureInfo.InvariantCulture))
                .ToArray();

            normalVectors.Add(new Vector4(normalVector[0], normalVector[1], normalVector[2], 1));
        }

        private static void AddToVertices(string line, char[] separatorArray, ref List<Vector4> vertices)
        {
            var vertex = GetLineValues(line, separatorArray)
                .Select(x => float.Parse(x, CultureInfo.InvariantCulture))
                .ToArray();

            vertices.Add(new Vector4(vertex[0], vertex[1], vertex[2], 1));
        }

        private static List<string> GetLineValues(string line, char[] separatorArray)
        {
            return line.Split(separatorArray, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();
        }

        private static void AddToPolygons(string line, char[] separatorArray, int vLength, int vtLength, int vnLength, ref List<Polygon> polygons)
        {
            Polygon polygon = null;

            var polygonsData = GetLineValues(line, separatorArray);

            for (var i = 0; i < polygonsData.Count; i++)
            {
                var elements = polygonsData[i].Split('/');
                var minVertexCount = polygonsData.Count;

                if (polygon == null)
                {
                    polygon = new Polygon(minVertexCount);
                }

                var vIndex = int.Parse(elements[0]);
                var vtIndex = int.Parse(elements[1]);
                var vnIndex = int.Parse(elements[2]);

                polygon.VertexIndices[i] = vIndex > 0 ? vIndex - 1 : vLength + vIndex;
                polygon.TextureIndices[i] = vtIndex > 0 ? vtIndex - 1 : vtLength + vtIndex;
                polygon.NormalIndices[i] = vnIndex > 0 ? vnIndex - 1 : vnLength + vnIndex;
            }

            for (var i = 0; i < polygonsData.Count - 2; i++)
            {
                polygons.Add(new Polygon
                {
                    VertexIndices = new[]
                        {polygon.VertexIndices[0], polygon.VertexIndices[i + 1], polygon.VertexIndices[i + 2]},
                    NormalIndices = new[]
                        {polygon.NormalIndices[0], polygon.NormalIndices[i + 1], polygon.NormalIndices[i + 2]},
                    TextureIndices = new[]
                        {polygon.TextureIndices[0], polygon.TextureIndices[i + 1], polygon.TextureIndices[i + 2]}
                });
            }
        }
    }
}