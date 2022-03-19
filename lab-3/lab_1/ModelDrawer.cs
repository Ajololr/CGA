using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace lab_1
{
    public class ModelDrawer
    {
        private Model _model;
        private ModelProps _modelProps;
        private ZBuffer _zbuffer;
        private Bitmap _bm;
        private Model _transformedModel;
        private PhongLight _light;
        private Matrix4x4 _invertedMainMatrix;
        private Vector3 eye;

        public ModelDrawer(Model model, ModelProps modelProps)
        {
            _model = model;
            _modelProps = modelProps;
        }

        private Model TransformModel()
        {
            var cModel = (Model)_model.Clone();

            eye = new Vector3(_modelProps.CameraX, _modelProps.CameraY, _modelProps.CameraZ);
            var target = new Vector3(0, 3, 0);
            var up = new Vector3(0, 1, 0);

            var radian = (float)Math.PI / 180;
            var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll((_modelProps.CameraYRotation % 360) * radian, 
                (_modelProps.CameraXRotation % 360) * radian, (_modelProps.CameraZRotation % 360) * radian);

            var worldMatrix = rotationMatrix;

            eye = Vector3.Transform(eye, worldMatrix);
            up = Vector3.Transform(up, worldMatrix);

            var viewerMatrix = GetViewerMatrix(eye, target, up);
            var projectionMatrix = GetProjectionMatrix();
            var viewPortMatrix = GetViewPortMatrix();
            
            var mainMatrix = viewerMatrix * projectionMatrix;
            var w = new float[cModel.vertices.Count];

            for (int i = 0; i < cModel.vertices.Count; i++)
            {
                cModel.vertices[i] = Vector4.Transform(cModel.vertices[i], mainMatrix);
                w[i] = cModel.vertices[i].W;
                cModel.vertices[i] /= cModel.vertices[i].W;
            }

            cModel.polygons = cModel.polygons.Where(poligon =>
                !poligon.Any(i => cModel.vertices[i.v].Z < 0 || cModel.vertices[i.v].Z > 1)).ToList();

            Matrix4x4.Invert(mainMatrix * viewPortMatrix, out _invertedMainMatrix);
            
            for (int i = 0; i < cModel.vertices.Count; i++)
            {
                cModel.vertices[i] = Vector4.Transform(cModel.vertices[i], viewPortMatrix);
                var temp = cModel.vertices[i];
                temp.W = w[i];
                cModel.vertices[i] = temp;
            }

            return cModel;
        }

        public Matrix4x4 GetViewerMatrix(Vector3 eye, Vector3 target, Vector3 up)
        {
            var zAxis = Vector3.Normalize(eye - target);
            var xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis));
            var yAxis = Vector3.Normalize(Vector3.Cross(zAxis, xAxis));

            var viewerMatrix = new Matrix4x4(xAxis.X, xAxis.Y, xAxis.Z, -Vector3.Dot(xAxis, eye),
                yAxis.X, yAxis.Y, yAxis.Z, -Vector3.Dot(yAxis, eye),
                zAxis.X, zAxis.Y, zAxis.Z, -Vector3.Dot(zAxis, eye),
                0,       0,       0,                       1);

            return Matrix4x4.Transpose(viewerMatrix);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            var zNear = 0.2f;
            var zFar = 2000;
            var aspect = _modelProps.ImageWidth / (float)_modelProps.ImageHeight;
            var fov = (float) Math.PI * (45) / 180;

            var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                                            fov,
                                            aspect,
                                            zNear,
                                            zFar);

            return projectionMatrix;
        }

        public Matrix4x4 GetViewPortMatrix()
        {
            var width = _modelProps.ImageWidth * 3 / 4;
            var height = _modelProps.ImageHeight * 3 / 4;

            var xMin = _modelProps.ImageWidth / 8; 
            var yMin = _modelProps.ImageHeight / 8;

            var m00 = width / 2;
            var m11 = -height / 2;
            var m03 = xMin + (width / 2);
            var m13 = yMin + (height / 2);
            var m22 = 255 / 2f;

            var viewerMatrix = new Matrix4x4(m00, 0, 0, m03,
                                               0, m11, 0, m13,
                                               0, 0, m22, m22,
                                               0, 0, 0, 1);

            return Matrix4x4.Transpose(viewerMatrix);
        }

        public Bitmap Draw()
        {
            _transformedModel = TransformModel();
            return RenderWithLight();
        }
        
        public Bitmap RenderWithLight()
        {
            var width = _modelProps.ImageWidth;
            var height = _modelProps.ImageHeight;
            
            _light = new PhongLight(
                _modelProps.Light,
                eye,
                new Vector3(0.1f),
                new Vector3(.7f),
                new Vector3(0.8f),
                new Vector3(255,0,0),
                new Vector3(255, 0, 0),
                new Vector3(255),
                30f
            );


            _zbuffer = new ZBuffer(height, width);
            
            var nc = Vector3.Normalize( new Vector3(_modelProps.CameraX, _modelProps.CameraY, _modelProps.CameraZ));

            _bm = new Bitmap(width, height);
            using (var gr = Graphics.FromImage(_bm))
            {
                gr.Clear(Color.Gray);
            }

            var polygons = _transformedModel.polygons.Where(p => p.Length == 3);

            foreach (var poligon in polygons)
            {
                try
                {
                    var vertex1 = _transformedModel.vertices[poligon[0].v];
                    var vertex2 = _transformedModel.vertices[poligon[1].v];
                    var vertex3 = _transformedModel.vertices[poligon[2].v];

                    Vector3 n = Vector3.Cross((ToVector3(vertex3) - ToVector3(vertex1)), (ToVector3(vertex2) - ToVector3(vertex1)));
                    n = Vector3.Normalize(n);
                    float ci = Vector3.Dot(n, Vector3.Normalize(nc));
                    
                    if (ci <= 0)
                    {
                        continue;
                    } 
                    
                    DrawTriangle(poligon);
                }
                catch (Exception ex)
                {
                    var t = (ex.ToString());
                    Debug.WriteLine(ex.ToString());
                }
            }
            
            return _bm;
        }
        
        public void DrawTriangle((int v,int vt,int vn)[] poligon)
        {
            var t0 = _transformedModel.vertices[poligon[0].v];
            var t1 = _transformedModel.vertices[poligon[1].v];
            var t2 = _transformedModel.vertices[poligon[2].v];

            var n0 = _transformedModel.normals[poligon[0].vn];
            var n1 = _transformedModel.normals[poligon[1].vn];
            var n2 = _transformedModel.normals[poligon[2].vn];
            
            var allPoints = new List<Point>();

            allPoints.AddRange(GetLinePointsBetweenTwoPoints(t0, t1, n0, n1));
            allPoints.AddRange(GetLinePointsBetweenTwoPoints(t1, t2, n1, n2));
            allPoints.AddRange(GetLinePointsBetweenTwoPoints(t0, t2, n0, n2));

            var pointsLookup = allPoints.ToLookup(p => p.Y, p => p);

            int maxY = (int)Math.Max(t0.Y, Math.Max(t1.Y, t2.Y));
            int minY = (int)Math.Min(t0.Y, Math.Min(t1.Y, t2.Y));

            for (int y = minY; y < maxY; y++)
            {
                if (pointsLookup.Contains(y))
                {
                    var points = pointsLookup[y].OrderBy(p => p.X);
                    var firstPoints = points.First();
                    var lastPoints = points.Last();
                    DrawLine(firstPoints, lastPoints);
                }
            }
        }

        public List<Point> GetLinePointsBetweenTwoPoints(Vector4 t1, Vector4 t2, Vector3 n1, Vector3 n2)
        {
            List<Point> points = new List<Point>();

            var x1 = (int)t1.X;
            var y1 = (int)t1.Y;
            var z1 = t1.Z;

            var x2 = (int)t2.X;
            var y2 = (int)t2.Y;
            var z2 = t2.Z;

            float x = x1;
            float y = y1;
            float z = z1;
            var dx = Math.Abs(x2 - x1);
            var dy = Math.Abs(y2 - y1);
            var length = dx >= dy ? dx : dy;
            var stepx = (x2 - x1) / (float)length;
            var stepy = (y2 - y1) / (float)length;
            var stepz = (z2 - z1) / length;

            var deltaNormal = (n2 - n1) / length;
            var curNormal = n1;

            for (int i = 0; i <= length; i++, x += stepx, y += stepy, z += stepz, curNormal += deltaNormal)
            {
                points.Add(new Point((int)x, (int)y, z, curNormal));
            }

            return points;
        }

        public void DrawLine(Point p1, Point p2)
        {
            var x1 = (int)p1.X;
            var y1 = (int)p1.Y;
            var z1 = p1.Z;

            var x2 = (int)p2.X;
            var y2 = (int)p2.Y;
            var z2 = p2.Z;

            float x = x1;
            float y = y1;
            float z = z1;
            var dx = Math.Abs(x2 - x1);
            var dy = Math.Abs(y2 - y1);
            var length = dx >= dy ? dx : dy;
            var stepx = (x2 - x1) / (float)length;
            var stepy = (y2 - y1) / (float)length;
            var stepz = (z2 - z1) / length;

            var deltaNormal = (p2.Normal - p1.Normal) / length;
            var curNormal = p1.Normal;

            for (int i = 0; i <= length; i++, curNormal += deltaNormal)
            {
                DrawPoint((int)x, (int)y, z, curNormal);
                x += stepx;
                y += stepy;
                z += stepz;
            }
        }

        public void DrawPoint(int x, int y, float z, Vector3 normal)
        {
            normal = Vector3.Normalize(normal);
            Color pointColor =  _light.GetPointColor(Vector3.Transform(new Vector3(x,y,z), _invertedMainMatrix), normal);

            if (x <= _modelProps.ImageWidth && x >= 0
                                            && y <= _modelProps.ImageHeight && y >= 0
                                            && _zbuffer[x, y] > z)
            {
                _zbuffer[x, y] = z;
                _bm.SetPixel(x, y, pointColor);
            }
        }
        
        public Vector3 ToVector3(Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}
