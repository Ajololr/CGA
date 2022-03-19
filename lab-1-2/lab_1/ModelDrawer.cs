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
        private ModelProps modelProps;
        private ZBuffer zbuffer;
        private Vector3 normalLight;

        public ModelDrawer(Model model, ModelProps modelProps)
        {
            this._model = model;
            this.modelProps = modelProps;
        }

        private Model TransformModel()
        {
            var cModel = (Model)this._model.Clone();

            var eye = new Vector3(modelProps.CameraX, modelProps.CameraY, modelProps.CameraZ);
            var target = new Vector3(0, 0, 0);
            var up = new Vector3(0, 1, 0);

            var radian = (float)Math.PI / 180;
            var worldMatrix = Matrix4x4.CreateFromYawPitchRoll((modelProps.CameraYRotation % 360) * radian, 
                (modelProps.CameraXRotation % 360) * radian, (modelProps.CameraZRotation % 360) * radian);

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


            for (int i = 0; i < cModel.normals.Count; i++)
            {
                cModel.normals[i] = Vector3.Normalize(Vector3.TransformNormal(cModel.normals[i], viewerMatrix));
            }
            
            for (int i = 0; i < cModel.vertices.Count; i++)
            {
                cModel.vertices[i] = Vector4.Transform(cModel.vertices[i], viewPortMatrix);
            }

            return cModel;
        }

        public Matrix4x4 GetViewerMatrix(Vector3 eye, Vector3 target, Vector3 up)
        {
            var zAxis = Vector3.Normalize(eye - target);
            var xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis));
            var yAxis = Vector3.Normalize(Vector3.Cross(zAxis, xAxis));

            var viewerMatrix = new Matrix4x4(xAxis.X, xAxis.Y, xAxis.Z, 0,
                                             yAxis.X, yAxis.Y, yAxis.Z, 0,
                                             zAxis.X, zAxis.Y, zAxis.Z, 0,
                                                0,       0,       0,    1);

            var translation = Matrix4x4.CreateTranslation(new Vector3(-Vector3.Dot(xAxis, eye),
                -Vector3.Dot(yAxis, eye), -Vector3.Dot(zAxis, eye)));

            return viewerMatrix * translation;
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            var zNear = 0.1f;
            var zFar = 2000;
            var aspect = modelProps.ImageWidth / (float)modelProps.ImageHeight;
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
            var width = modelProps.ImageWidth * 3 / 4;
            var height = modelProps.ImageHeight * 3 / 4;

            var xMin = modelProps.ImageWidth / 8; 
            var yMin = modelProps.ImageHeight / 8;

            var m00 = width / 2;
            var m11 = -height / 2;
            var m03 = xMin + (width / 2);
            var m13 = yMin + (height / 2);
            var m22 = 255 / 2f;

            var viewerMatrix = new Matrix4x4(m00, 0, 0, m03,
                                               0, m11, 0, m13,
                                               0, 0, m22, m22,
                                               0, 0, 0, 1);

            viewerMatrix.Translation = new Vector3(m03, m13, m22);

            return Matrix4x4.Transpose(viewerMatrix);
        }

        public Bitmap Draw()
        {
            var transformedModel = TransformModel();
            return RenderWithLight(transformedModel, modelProps);
        }
        
        public Bitmap RenderWithLight(Model model, ModelProps modelParams)
        {
            var width = modelParams.ImageWidth;
            var height = modelParams.ImageHeight;

            zbuffer = new ZBuffer(height, width);
            modelProps = modelParams;
            normalLight = Vector3.Normalize(modelParams.Light);
            var normalCamera = Vector3.Normalize( new Vector3(modelParams.CameraX, modelParams.CameraY, modelParams.CameraZ));

            var bm = new Bitmap(width, height);
            using (var gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.Red);
            }

            var polygons = model.polygons.Where(p => p.Length == 3);

            foreach (var poligon in polygons)
            {
                try
                {
                    var vertex1 = model.vertices[poligon[0].v];
                    var vertex2 = model.vertices[poligon[1].v];
                    var vertex3 = model.vertices[poligon[2].v];

                    Vector3 n = Vector3.Cross((ToVector3(vertex3) - ToVector3(vertex1)), (ToVector3(vertex2) - ToVector3(vertex1)));
                    n = Vector3.Normalize(n);
                    float intensity = Vector3.Dot(n, Vector3.Normalize(normalLight));
                    
                    
                    float cameraIntensity = Vector3.Dot(n, Vector3.Normalize(normalCamera));
                    
                    if (cameraIntensity <= 0)
                    {
                        continue;
                    } 

                    var color = Color.FromArgb(255, 255, 255);
                    // ??
                    // if (intensity > 0)
                    {
                        var faceColor = GetPoligonColor(model, poligon, color);
                        DrawTriangle(ToVector3(vertex1), ToVector3(vertex2), ToVector3(vertex3), bm, faceColor);
                    }
                }
                catch (Exception ex)
                {
                    var t = (ex.ToString());
                    Debug.WriteLine(ex.ToString());
                }
            }
            
            return bm;
        }
        
        public void DrawTriangle(Vector3 t0, Vector3 t1, Vector3 t2, Bitmap bm, Color color)
        {
            if (t0.Y == t1.Y && t0.Y == t2.Y) return; 
            
            if (t0.Y > t1.Y) Swap(ref t0, ref t1);
            if (t0.Y > t2.Y) Swap(ref t0, ref t2);
            if (t1.Y > t2.Y) Swap(ref t1, ref t2);
            var total_height = t2.Y - t0.Y;
            for (int i = 0; i <= total_height; i++)
            {
                bool second_half = i > t1.Y - t0.Y || t1.Y == t0.Y;
                var segment_height = (second_half ? t2.Y - t1.Y : t1.Y - t0.Y);
                segment_height = segment_height == 0 ? 1 : segment_height;
                float alpha = i / total_height;
                float beta = (float)(i - (second_half ? t1.Y - t0.Y : 0)) / segment_height;
                Vector3 A = t0 + ((t2 - t0) * alpha);
                Vector3 B = second_half ? t1 + (t2 - t1) * beta : t0 + (t1 - t0) * beta;
                if (A.X > B.X) Swap(ref A, ref B);
                for (int j = (int)Math.Floor(A.X); j <= B.X; j++)
                {
                    float phi = B.X==A.X ? 1f : (float)(j-A.X)/(float)(B.X-A.X);
                    Vector3 P = A + (B-A)*phi;
                    DrawPixel((int)P.X, (int)P.Y, P.Z, bm, color);
                }
            }
        }

        public void DrawPixel(int x, int y, float z, Bitmap bm, Color color)
        {
            if (x <= modelProps.ImageWidth && x >= 0 && y <= modelProps.ImageHeight && y >= 0 && zbuffer[x, y] >= z)
            {
                zbuffer[x, y] = z;
                bm.SetPixel(x, y, color);
            }
        }

        public Bitmap DisplayModel(Model model , ModelProps modelProps)
        {
            var width = modelProps.ImageWidth;
            var height = modelProps.ImageHeight;

            var bm = new Bitmap(width, height);
            using (var gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.Black);

                foreach (var poligon in model.polygons)
                {

                    for (int i = 0; i < poligon.Length; i++)
                    {
                        var k = poligon[i].v - 1;
                        var j = poligon[(i + 1) % poligon.Length].v - 1;

                        var X1 = (model.vertices[k].X);
                        var X2 = (model.vertices[j].X);
                        var Y1 = (model.vertices[k].Y);
                        var Y2 = (model.vertices[j].Y);
                        DrawLineBres((int)X1, (int)Y1, (int)X2, (int)Y2, gr);
                    };
                };
            }

            return bm;
        }  

        protected Color GetPoligonColor(Model model, (int v , int vt, int vn)[] face, Color color)
        {
            var normal1 = model.normals[(int)face[0].vn];
            var normal2 = model.normals[(int)face[1].vn];
            var normal3 = model.normals[(int)face[2].vn];

            Color color1 = GetPointColor( normal1, color);
            Color color2 = GetPointColor( normal2, color);
            Color color3 = GetPointColor( normal3, color);

            return GetAverageColor(color1, color2, color3);
        }
        
        public Color GetPointColor(Vector3 normal, Color color)
        {
            double coef = Math.Max(Vector3.Dot(normal, Vector3.Normalize(normalLight)), 0);
            byte r = (byte)Math.Round(color.R * coef);
            byte g = (byte)Math.Round(color.G * coef);
            byte b = (byte)Math.Round(color.B * coef);

            return Color.FromArgb(255, r, g, b);
        }
        
        public static Color GetAverageColor(Color color1, Color color2, Color color3)
        {
            int sumR = color1.R + color2.R + color3.R;
            int sumG = color1.G + color2.G + color3.G;
            int sumB = color1.B + color2.B + color3.B;
            int sumA = color1.A + color2.A + color3.A;

            byte r = (byte)Math.Round(sumR / 3.0);
            byte g = (byte)Math.Round(sumG / 3.0);
            byte b = (byte)Math.Round(sumB / 3.0);
            byte a = (byte)Math.Round(sumA / 3.0);

            return Color.FromArgb(a, r, g, b);
        }
        
        public void DrawLineBres(int x1, int y1, int x2, int y2, Graphics bm)
        {
            bool steep = false;
            if (Math.Abs(x1 - x2) < Math.Abs(y1 - y2))
            {
                Swap(ref x1, ref y1);
                Swap(ref x2, ref y2);
                steep = true;
            }

            if (x1 > x2)
            {
                Swap(ref x2, ref x1);
                Swap(ref y2, ref y1);
            }
            
            void Swap(ref int a, ref int b)
            {
                int t = a;
                a = b;
                b = t;
            }

            Brush aBrush = Brushes.White;
            for (int x = x1; x <= x2; x++)
            {
                float t = (x2 - x1 == 0) ? 1 : (x - x1) / (float)(x2 - x1);
                int y = (int)(y1 * (1.0 - t) + y2 * t);

                if (steep)
                    bm.FillRectangle(aBrush, y, x, 1, 1);
                else
                    bm.FillRectangle(aBrush, x, y, 1, 1);
            }
        }

        public void DrawLineDDA(int x1, int y1, int x2, int y2, Graphics bm)
        {
            float x = x1;
            float y = y1;
            var dx = Math.Abs(x2 - x1);
            var dy = Math.Abs(y2 - y1);
            var length = dx >= dy ? dx : dy;
            var stepx = (x2 - x1) / (float)length;
            var stepy = (y2 - y1) / (float)length;

            Brush aBrush = Brushes.White;
            bm.FillRectangle(aBrush, x, y, 1, 1);
            for (int i = 1; i <= length; i++)
            {
                bm.FillRectangle(aBrush, (float)Math.Round(x), (float)Math.Round(y), 1, 1);
                x += stepx;
                y += stepy;
            }
        }
        
        public Vector3 ToVector3(Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        
        public static void Swap<T>(ref T a, ref T b)
        {
            (a, b) = (b, a);
        }
    }
}
