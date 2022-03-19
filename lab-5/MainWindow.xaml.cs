using ObjParser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Viewer3D.Models;

namespace Viewer3D
{
    public partial class MainWindow
    {
        private const double PiDivOn180 = Math.PI / 180;
        private const int ViewXMin = 0;
        private const int ViewYMin = 0;

        private readonly Vector4 modelColor = new Vector4(102, 0, 102, 255);
        private readonly Vector4 lightColor = new Vector4(1, 1, 1, 255);

        private ObjFileData _info;
        private WavefrontObject _info2;
        private string _targetFileName = "Jacket";

        private Matrix4x4 _translationMatrix;
        private Matrix4x4 _scaleMatrix;

        private Matrix4x4 _rotationXMatrix;
        private Matrix4x4 _rotationYMatrix;
        private Matrix4x4 _rotationZMatrix;
        private Matrix4x4 _lightRotationYMatrix;

        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;
        private Matrix4x4 _viewportMatrix;

        private Vector4 _lightPosition;
        private Vector3 _targetPosition;
        private Vector3 _cameraPosition;

        private float _currentTargetRotationX;
        private float _currentTargetRotationY;
        private float _currentTargetRotationZ;

        private float _currentTranslationX;
        private float _currentTranslationY;
        private float _currentTranslationZ;

        private float _currentCameraPositionX;
        private float _currentCameraPositionY;
        private float _currentCameraPositionZ = 20;

        private float _currentScalePositionX = 1;
        private float _currentScalePositionY = 1;
        private float _currentScalePositionZ = 1;

        private float _currentTargetPositionX;
        private float _currentTargetPositionY;
        private float _currentTargetPositionZ;

        private float _currentLightPositionX;
        private float _currentLightPositionY;
        private float _currentLightPositionZ;
        private float _currentLightRotationY;

        private Bitmap albedoMapTexture;
        private Bitmap normalMapTexture;
        private Bitmap specularMapTexture;
        private System.Drawing.Color[,] albedoMap;
        private System.Drawing.Color[,] normalMap;
        private System.Drawing.Color[,] specularMap;
        class BitmapInfo{
            public System.Drawing.Color[,] pixels { get; set; }
            public int width { get; set; }
            public int height { get; set; }

            public BitmapInfo(System.Drawing.Color[,] pixels, int width,int height)
            {
                this.pixels = pixels;
                this.width = width;
                this.height = height;
            }
        }
        private Dictionary<string, BitmapInfo> dictionaryMaps= new Dictionary<string, BitmapInfo>();
        public MainWindow()
        {

            InitializeComponent();
            RenderOptions.ProcessRenderMode = RenderMode.Default;
        }
        
        
        unsafe private System.Drawing.Color[,] GetPixels(Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData =
            bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
            bitmap.PixelFormat);
            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            IntPtr ptr = bmpData.Scan0;
            byte* stream = (byte*)ptr;
            System.Drawing.Color[,] colors = new System.Drawing.Color[bitmap.Width, bitmap.Height];
            var pixelFormatSize = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat);
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int byteIndex = y * bmpData.Stride + x * pixelFormatSize / 8;
                    byte r = stream[byteIndex + 2];
                    byte g = stream[byteIndex + 1];
                    byte b = stream[byteIndex];
                    colors[x, y] = System.Drawing.Color.FromArgb(255, r, g, b);
                }
            bitmap.UnlockBits(bmpData);

            return colors;
        }
        
        void AddToDictionary(string name)
        {
            var bitmap = new Bitmap($".\\{_targetFileName}\\{name}");
            dictionaryMaps[name] = new BitmapInfo(GetPixels(bitmap), bitmap.Width - 1, bitmap.Height - 1);
            bitmap.Dispose();
        }

        void InitMapDictionary()
        {
            foreach (var material in _info2.Materials)
            {
                if (material.AlphaMap != null) AddToDictionary(material.AlphaMap);
                if (material.AmbientMap != null) AddToDictionary(material.AmbientMap);
                if (material.BumpMap != null) AddToDictionary(material.BumpMap);
                if (material.DiffuseMap != null) AddToDictionary(material.DiffuseMap);
                if (material.NormalMap != null) AddToDictionary(material.NormalMap);
            }
        }

        private async void InitRender(object sender, EventArgs e)
        {
            _info = ObjFileParser.GetObjectInfo($".\\{_targetFileName}\\Model.obj");
            var loader = new ObjLoader();
            _info2 = await loader.LoadFromFile($".\\{_targetFileName}\\Model.obj");
            dictionaryMaps.Clear();
            InitMapDictionary();
            try
            {
                albedoMapTexture = new Bitmap($".\\{_targetFileName}\\Albedo Map.png");
                albedoMap = GetPixels(albedoMapTexture);
            }
            catch
            {
            }

            try
            {
                normalMapTexture = new Bitmap($".\\{_targetFileName}\\Normal Map.png");
                normalMap = GetPixels(normalMapTexture);
            }
            catch
            {
            }

            try
            {
                specularMapTexture = new Bitmap($".\\{_targetFileName}\\Specular Map.png");
                specularMap = GetPixels(specularMapTexture);
            }
            catch
            {
            }

            _currentTargetRotationX = 0;
            _currentTargetRotationY = 0;
            _currentTargetRotationZ = 0;

            _currentTranslationX = 0;
            _currentTranslationY = 0;
            _currentTranslationZ = 0;

            _currentCameraPositionX = 0;
            _currentCameraPositionY = 0;
            _currentCameraPositionZ = 10;

            _currentScalePositionX = 1;
            _currentScalePositionY = 1;
            _currentScalePositionZ = 1;

            _currentTargetPositionX = 0;
            _currentTargetPositionY = 0;
            _currentTargetPositionZ = 0;

            _currentLightPositionX = 0;
            _currentLightPositionY = 0;
            _currentLightPositionZ = 5;
            _currentLightRotationY = 0;

            Draw();
        }
        
        private void ComboBox_Selected(object sender, RoutedEventArgs e)
        {
            var selectedItem = (ComboBoxItem) ComboBox.SelectedItem;
            var tb = (TextBlock) selectedItem.Content;

            _targetFileName = tb.Text;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // Rotation
                case Key.A:
                    _currentTargetRotationY += 3;
                    Draw();
                    break;
                case Key.D:
                    _currentTargetRotationY -= 3;
                    Draw();
                    break;
                case Key.W:
                    _currentTargetRotationX -= 3;
                    Draw();
                    break;
                case Key.S:
                    _currentTargetRotationX += 3;
                    Draw();
                    break;
                case Key.Q:
                    _currentTargetRotationZ -= 3;
                    Draw();
                    break;
                case Key.E:
                    _currentTargetRotationZ += 3;
                    Draw();
                    break;
                // Transition
                case Key.F:
                    _currentTranslationY += 1;
                    Draw();
                    break;
                case Key.H:
                    _currentTranslationY -= 1;
                    Draw();
                    break;
                case Key.T:
                    _currentTranslationX -= 1;
                    Draw();
                    break;
                case Key.G:
                    _currentTranslationX += 1;
                    Draw();
                    break;
                case Key.R:
                    _currentTranslationZ -= 1;
                    Draw();
                    break;
                case Key.Y:
                    _currentTranslationZ += 1;
                    Draw();
                    break;
                // Camera
                case Key.I:
                    _currentCameraPositionY += 1;
                    Draw();
                    break;
                case Key.K:
                    _currentCameraPositionY -= 1;
                    Draw();
                    break;
                case Key.J:
                    _currentCameraPositionX -= 1;
                    Draw();
                    break;
                case Key.L:
                    _currentCameraPositionX += 1;
                    Draw();
                    break;
                case Key.U:
                    _currentCameraPositionZ -= 1;
                    Draw();
                    break;
                case Key.O:
                    _currentCameraPositionZ += 1;
                    Draw();
                    break;
                // Scale
                case Key.Up:
                    _currentScalePositionY = (float) (_currentScalePositionY + 0.1);
                    Draw();
                    break;
                case Key.Down:
                    _currentScalePositionY = (float) (_currentScalePositionY - 0.1);
                    Draw();
                    break;
                case Key.Left:
                    _currentScalePositionX = (float) (_currentScalePositionX - 0.1);
                    Draw();
                    break;
                case Key.Right:
                    _currentScalePositionX = (float) (_currentScalePositionX + 0.1);
                    Draw();
                    break;
                case Key.NumPad1:
                    _currentScalePositionZ = (float) (_currentScalePositionZ - 0.1);
                    Draw();
                    break;
                case Key.NumPad2:
                    _currentScalePositionZ = (float) (_currentScalePositionZ + 0.1);
                    Draw();
                    break;
                // Target posotion
                case Key.NumPad8:
                    _currentTargetPositionY += 1;
                    Draw();
                    break;
                case Key.NumPad5:
                    _currentTargetPositionY -= 1;
                    Draw();
                    break;
                case Key.NumPad4:
                    _currentTargetPositionX -= 1;
                    Draw();
                    break;
                case Key.NumPad6:
                    _currentTargetPositionX += 1;
                    Draw();
                    break;
                case Key.NumPad7:
                    _currentTargetPositionZ -= 1;
                    Draw();
                    break;
                case Key.NumPad9:
                    _currentTargetPositionZ += 1;
                    Draw();
                    break;
                // Light position
                case Key.Z:
                    _currentLightPositionY -= 1;
                    Draw();
                    break;
                case Key.X:
                    _currentLightPositionY += 1;
                    Draw();
                    break;
                case Key.C:
                    _currentLightRotationY += 10;
                    Draw();
                    break;
                case Key.V:
                    _currentLightRotationY -= 10;
                    Draw();
                    break;
            }
        }

        private void InitializeMatrix()
        {
            _translationMatrix = Matrix4x4.CreateTranslation(new Vector3(_currentTranslationX,
                _currentTranslationY, _currentTranslationZ));

            _scaleMatrix = Matrix4x4.CreateScale(new Vector3(_currentScalePositionX,
                _currentScalePositionY, _currentScalePositionZ));

            _rotationXMatrix =
                Matrix4x4.Transpose(Matrix4x4.CreateRotationX((float) (_currentTargetRotationX * PiDivOn180)));
            _rotationYMatrix =
                Matrix4x4.Transpose(Matrix4x4.CreateRotationY((float) (_currentTargetRotationY * PiDivOn180)));
            _rotationZMatrix =
                Matrix4x4.Transpose(Matrix4x4.CreateRotationZ((float) (_currentTargetRotationZ * PiDivOn180)));
            _lightRotationYMatrix =
                Matrix4x4.Transpose(Matrix4x4.CreateRotationY((float) (_currentLightRotationY * PiDivOn180)));

            _cameraPosition = new Vector3(_currentCameraPositionX, _currentCameraPositionY, _currentCameraPositionZ);

            _viewMatrix =
                new Vector3(_currentCameraPositionX, _currentCameraPositionY, _currentCameraPositionZ)
                    .GetViewMatrix4x4(new Vector3(_currentTargetPositionX, _currentTargetPositionY,
                        _currentTargetPositionZ));

            var camAspect = GetActualAspect();
            var camFov = (float) (float.Parse(cameraFOV.Text, CultureInfo.InvariantCulture) * PiDivOn180);

            var camZFar = float.Parse(cameraZFar.Text, CultureInfo.InvariantCulture);
            var camZNear = float.Parse(cameraZNear.Text, CultureInfo.InvariantCulture);

            _viewportMatrix =
                Matrix4x4.Transpose(ViewPortMatrix4x4.Create((int) imageGrid.ActualWidth, (int) imageGrid.ActualHeight,
                    ViewXMin, ViewYMin));
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                camFov, camAspect, camZNear, camZFar);

            _targetPosition = new Vector3(_currentTargetPositionX, _currentTargetPositionY, _currentTargetPositionZ);
            _lightPosition = new Vector4(_currentLightPositionX, _currentLightPositionY, _currentLightPositionZ, 1);

        }

        private System.Drawing.Color GetPixelFromFile(string name, int x, int y) 
        {
            BitmapInfo bitmapinfo;
            dictionaryMaps.TryGetValue(name, out bitmapinfo);
           
                return bitmapinfo.pixels[x, y];
            
        }
        private int GetWidth(string name)
        {
            BitmapInfo bitmapinfo;
            dictionaryMaps.TryGetValue(name, out bitmapinfo);
            return bitmapinfo.width;

        }

        private int GetHeight(string name)
        {
            BitmapInfo bitmapinfo;
            dictionaryMaps.TryGetValue(name, out bitmapinfo);
            return bitmapinfo.height;


        }

        private void Draw()
        {
            if (_info?.Polygons == null || _info.Polygons.Length == 0)
            {
                return;
            }

            InitializeMatrix();

            /*var vertices = new Vector4[_info.Vertices.Length];
            var worldVertices = new Vector4[_info.Vertices.Length];
            var worldNormalVectors = new Vector4[_info.NormalVectors.Length];*/
            var vertices = new Vector4[_info2.Positions.Length];
            var worldVertices = new Vector4[_info2.Positions.Length];
            var worldNormalVectors = new Vector3[_info2.Normals.Length];

            /*_info.Vertices.CopyTo(vertices, 0);
            _info.NormalVectors.CopyTo(worldNormalVectors, 0);*/
            _info2.Positions.CopyTo(vertices, 0);
            _info2.Normals.CopyTo(worldNormalVectors, 0);
            Parallel.For(0, _info2.Positions.Length, i =>
            {
                vertices[i] = Vector4.Transform(vertices[i], _translationMatrix);
                vertices[i] = Vector4.Transform(vertices[i],
                    Matrix4x4.Multiply(Matrix4x4.Multiply(_rotationZMatrix, _rotationYMatrix), _rotationXMatrix));
                vertices[i] = Vector4.Transform(vertices[i], _scaleMatrix);
            });

            _lightPosition = Vector4.Transform(_lightPosition, _lightRotationYMatrix);

            vertices.CopyTo(worldVertices, 0);

            Parallel.For(0, _info2.Positions.Length, i =>
            {
                vertices[i] = Vector4.Transform(vertices[i], _viewMatrix);
                vertices[i] = Vector4.Transform(vertices[i], _projectionMatrix);
                vertices[i] = Vector4.Transform(vertices[i], _viewportMatrix);

                vertices[i].X /= vertices[i].W;
                vertices[i].Y /= vertices[i].W;
                vertices[i].Z /= vertices[i].W;
                vertices[i].W = 1 / vertices[i].W;
            });

            Parallel.For(0, _info2.Normals.Length,
                i =>
                {
                    worldNormalVectors[i] = Vector3.Transform(worldNormalVectors[i],
                        Matrix4x4.Multiply(Matrix4x4.Multiply(_rotationZMatrix, _rotationYMatrix), _rotationXMatrix));
                });

            DrawAllPolygons(vertices, worldVertices, worldNormalVectors);
        }

        private void DrawAllPolygons(Vector4[] vertices, Vector4[] worldVertices,
            Vector3[] worldNormalVectors)
        {
            var wbm = new WriteableBitmap((int) imageGrid.ActualWidth, (int) imageGrid.ActualHeight, 96, 96,
                PixelFormats.Bgra32, null);
            var source = new Bgra32Bitmap(wbm);
            var zBuffer = new ZBuffer(source.PixelWidth, source.PixelHeight);

            source.Source.Lock();
            foreach (var group in _info2.Groups)
            {
                Parallel.For(0, group._faces.Count-1,
                i =>
                {
                    DrawLinesInPolygon(new Polygon(group._faces[i]), vertices, ref source, zBuffer, worldVertices, worldNormalVectors);
                });
               
            }

            /* Parallel.For(0, _info.Polygons.Length,
                 i =>
                 {
                     DrawLinesInPolygon(_info.Polygons[i], vertices, ref source, zBuffer, worldVertices, worldNormalVectors);
                 });*/
            /*foreach (var polygon in _info.Polygons)
            {
                DrawLinesInPolygon(polygon, vertices, ref source, zBuffer, worldVertices, worldNormalVectors);
            }*/

            source.Source.AddDirtyRect(new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight));
            source.Source.Unlock();
            mainImage.Source = source.Source;
        }

        private void DrawLinesInPolygon(Polygon polygon, Vector4[] vertices, ref Bgra32Bitmap source,
            ZBuffer zBuffer, Vector4[] worldVertices, Vector3[] worldNormalVectors)
        {
            var p0 = vertices[polygon.VertexIndices[0]];
            var p1 = vertices[polygon.VertexIndices[1]];
            var p2 = vertices[polygon.VertexIndices[2]];

            var higherPolygon = 0;

            if (p1.Y < p0.Y)
            {
                higherPolygon = 1;
            }

            if (p2.Y < p1.Y)
            {
                higherPolygon = 2;
            }

            var polygonAngleCoefficient =
                ClippingPolygons(worldVertices, polygon, higherPolygon, polygon.VertexIndices.Length);

            if (polygonAngleCoefficient < -0.3)
            {
                return;
            }

            DrawGradientTriangle(polygon, vertices, worldVertices, worldNormalVectors, source, zBuffer);
        }

        private float ClippingPolygons(Vector4[] worldVertices, Polygon currentPolygon, int higherPolygon,
            int polygonLength)
        {
            var v41 = worldVertices[currentPolygon.VertexIndices[(higherPolygon + 2) % polygonLength]]
                      - worldVertices[currentPolygon.VertexIndices[higherPolygon]];
            var v42 = worldVertices[currentPolygon.VertexIndices[(higherPolygon + 1) % polygonLength]]
                      - worldVertices[currentPolygon.VertexIndices[higherPolygon]];
            var v31 = v41.ConvertToVector3();
            var v32 = v42.ConvertToVector3();
            var n = v31.CrossProduct(v32);

            var polygonPosition =
                worldVertices[currentPolygon.VertexIndices[higherPolygon]].ConvertToVector3();
            var cameraDir = Vector3.Subtract(Vector3.Add(polygonPosition, _targetPosition), _cameraPosition);
            var polygonAngleCoefficient = Vector3.Dot(n, cameraDir);

            return polygonAngleCoefficient;
        }
        
        private void DrawGradientTriangle(Polygon polygon, Vector4[] vertices, Vector4[] worldVertices,
            Vector3[] worldNormalVectors, Bgra32Bitmap source, ZBuffer zBuffer)
        {
            // Позиция вершин
            var v0 = vertices[polygon.VertexIndices[0]].Clone();
            var v1 = vertices[polygon.VertexIndices[1]].Clone();
            var v2 = vertices[polygon.VertexIndices[2]].Clone();

            // Позиция вершин в мировом пространстве
            var p0 = worldVertices[polygon.VertexIndices[0]].Clone();
            var p1 = worldVertices[polygon.VertexIndices[1]].Clone();
            var p2 = worldVertices[polygon.VertexIndices[2]].Clone();

            // Вектора нормали вершин
            var n0 = worldNormalVectors[polygon.NormalIndices[0]];
            var n1 = worldNormalVectors[polygon.NormalIndices[1]];
            var n2 = worldNormalVectors[polygon.NormalIndices[2]];

            /*Vector4 t0 = _info.TextureVertices[polygon.TextureIndices[0]].Clone();
            Vector4 t1 = _info.TextureVertices[polygon.TextureIndices[1]].Clone();
            Vector4 t2 = _info.TextureVertices[polygon.TextureIndices[2]].Clone();*/
            Vector2 t0 = _info2.Textures[polygon.TextureIndices[0]];
            Vector2 t1 = _info2.Textures[polygon.TextureIndices[1]];
            Vector2 t2 = _info2.Textures[polygon.TextureIndices[2]];

            if ((int) Math.Ceiling(v0.Y) > (int) Math.Ceiling(v1.Y))
            {
                VectorExtensions.SwapPoints(ref v0, ref v1);
                //VectorExtensions.SwapPoints(ref n0, ref n1);
                var temp = n0;
                n0 = n1;
                n1 = temp;
                VectorExtensions.SwapPoints(ref p0, ref p1);
                // VectorExtensions.SwapPoints(ref t0, ref t1);
                var temp2 = t0;
                t0 = t1;
                t1 = temp2;
            }

            if ((int) Math.Ceiling(v1.Y) > (int) Math.Ceiling(v2.Y))
            {
                VectorExtensions.SwapPoints(ref v1, ref v2);
                //VectorExtensions.SwapPoints(ref n1, ref n2);
                var temp = n1;
                n1 = n2;
                n2 = temp;
                VectorExtensions.SwapPoints(ref p1, ref p2);
                //VectorExtensions.SwapPoints(ref t1, ref t2);
                var temp2 = t1;
                t1 = t2;
                t2 = temp2;
            }
            
            if ((int) Math.Ceiling(v0.Y) > (int) Math.Ceiling(v1.Y))
            {
                VectorExtensions.SwapPoints(ref v0, ref v1);
                // VectorExtensions.SwapPoints(ref n0, ref n1);
                var temp = n0;
                n0 = n1;
                n1 = temp;
                VectorExtensions.SwapPoints(ref p0, ref p1);
                //VectorExtensions.SwapPoints(ref t0, ref t1);
                var temp2 = t0;
                t0 = t1;
                t1 = temp2;
            }

            int p0y = (int) Math.Ceiling(v0.Y);
            int p1y = (int) Math.Ceiling(v1.Y);
            int p2y = (int) Math.Ceiling(v2.Y);

            int total_height = p2y - p0y;

            // Отрисовка треугольника по строкам
            for (int i = 0; i < total_height; i++)
            {
                bool second_half = i > p1y - p0y || p1y == p0y;
                int segment_height = second_half ? p2y - p1y : p1y - p0y;
                if (segment_height == 0)
                {
                    continue;
                }

                float alpha = (float) i / total_height;
                float beta = (float) (i - (second_half ? p1y - p0y : 0)) /
                             segment_height;

                // Точки на экране
                Vector4 A = v0 + (v2 - v0) * alpha;
                Vector4 B = second_half ? v1 + (v2 - v1) * beta : v0 + (v1 - v0) * beta;

                // Нормали в точках A и B
                Vector3 An = n0 + (n2 - n0) * alpha;
                Vector3 Bn = second_half ? n1 + (n2 - n1) * beta : n0 + (n1 - n0) * beta;

                // Мировые координаты точек
                Vector4 Aw = p0 + (p2 - p0) * alpha;
                Vector4 Bw = second_half ? p1 + (p2 - p1) * beta : p0 + (p1 - p0) * beta;

                Vector2 At = ((1 - alpha) * t0 * v0.W + alpha * t2 * v2.W) / ((1 - alpha) * v0.W + alpha * v2.W);
                Vector2 Bt = second_half
                    ? ((1 - beta) * t1 * v1.W + beta * t2 * v2.W) / ((1 - beta) * v1.W + beta * v2.W)
                    : ((1 - beta) * t0 * v0.W + beta * t1 * v1.W) / ((1 - beta) * v0.W + beta * v1.W);

                if ((int) Math.Ceiling(A.X) > (int) Math.Ceiling(B.X))
                {
                    VectorExtensions.SwapPoints(ref A, ref B);
                    // VectorExtensions.SwapPoints(ref An, ref Bn);
                    var temp = An;
                    An = Bn;
                    Bn = temp;
                    VectorExtensions.SwapPoints(ref Aw, ref Bw);
                    //VectorExtensions.SwapPoints(ref At, ref Bt);
                    var temp2 = At;
                    At = Bt;
                    Bt = temp2;
                }

                var leftBorder = (int) Math.Ceiling(A.X);
                var rightBorder = (int) Math.Ceiling(B.X);

                // Отрисовка линии
                for (var j = leftBorder; j < rightBorder; j++)
                {
                    var Ax = (int) Math.Ceiling(A.X);
                    var Bx = (int) Math.Ceiling(B.X);
                    var phi = Bx == Ax ? 1f : (j - Ax) / (float) (Bx - Ax);

                    Vector4 P = A + (B - A) * phi;
                    Vector3 pixelNormalVector = An + (Bn - An) * phi;
                    Vector4 pixelWorldPosition = Aw + (Bw - Aw) * phi;
                    
                    var lerpW = (1 - phi) * A.W + phi * B.W;
                    Vector2 pixelTexture = ((1 - phi) * At * A.W + phi * Bt * B.W) / lerpW;
                   
                    if (pixelTexture.X < 0 || pixelTexture.X > 1 || pixelTexture.Y < 0 || pixelTexture.Y > 1)
                    {
                        if (_info2.Materials[polygon.Material].DiffuseMap == "Model002_Material001.png")
                        {
                            var kru = "rka";
                        }
                        /*  if (pixelTexture.X < 0) pixelTexture.X = 1- (pixelTexture.X - (float)(pixelTexture.X - Math.Floor(pixelTexture.X)));
                          if (pixelTexture.X > 1) pixelTexture.X = (float)(pixelTexture.X - Math.Floor(pixelTexture.X)) - 1;
                          if (pixelTexture.Y < 0) pixelTexture.Y = 1 - (float)(pixelTexture.Y - Math.Floor(pixelTexture.Y));
                          if (pixelTexture.Y > 1) pixelTexture.Y = (float)(pixelTexture.Y - Math.Floor(pixelTexture.Y)) - 1;*/
                        if (pixelTexture.X < 0) pixelTexture.X = 1 - (pixelTexture.X);
                        if (pixelTexture.X > 1) pixelTexture.X = (pixelTexture.X) - 1;
                        if (pixelTexture.Y < 0) pixelTexture.Y = 1 - (pixelTexture.Y);
                        if (pixelTexture.Y > 1) pixelTexture.Y = (pixelTexture.Y) - 1;
                        if (_info2.Materials[polygon.Material].DiffuseMap == "Model002_Material001.png")
                        {
                            var kru = "rka";
                        }
                        // continue;
                    }
                    
                    //var albedo = new Bitmap($".\\{_targetFileName}\\{_info2.Materials[polygon.Material].DiffuseMap}");
                    var nameMap = _info2.Materials[polygon.Material].DiffuseMap ??
                        _info2.Materials[polygon.Material].AlphaMap ??
                        _info2.Materials[polygon.Material].AmbientMap ??
                        _info2.Materials[polygon.Material].BumpMap??"";
                    var textureX = (int) Math.Ceiling(nameMap!=""? GetWidth(nameMap) * pixelTexture.X : 1023f * pixelTexture.X);
                    var textureY = (int) Math.Ceiling(nameMap != "" ? GetHeight(nameMap) * (1 - pixelTexture.Y) : 1023f * (1 - pixelTexture.Y));

                    
                    Vector4 pixelColorVector;
                    if (_info2.Materials[polygon.Material].DiffuseMap != null && dictionaryMaps.ContainsKey(_info2.Materials[polygon.Material].DiffuseMap))
                    {
                        var pixelColor = GetPixelFromFile(_info2.Materials[polygon.Material].DiffuseMap, textureX, textureY);

                        pixelColorVector = new Vector4(pixelColor.R, pixelColor.G, pixelColor.B, pixelColor.A);
                        
                    }
                    else
                    {
                        if (albedoMapTexture != null)
                        {
                            var pixelColor = albedoMap[textureX, textureY];
                            pixelColorVector = new Vector4(pixelColor.R, pixelColor.G, pixelColor.B, pixelColor.A);

                        }
                        else
                        {
                            pixelColorVector = _info2.Materials[polygon.Material].AmbientColor.vecColor*255;
                        }
                    }


                    // Фоновое освещение
                    Vector4 ambient = pixelColorVector; //: lightColor // коэф фонового освещения * цвет фонового света

                    // Рассеяное освещение
                    Vector3 light_dir =
                        Vector3.Normalize(pixelWorldPosition.ConvertToVector3() - _lightPosition.ConvertToVector3());
                    Vector3 normalVector = Vector3.Normalize(pixelNormalVector);

                    if (_info2.Materials[polygon.Material].NormalMap!=null)// || normalMapTexture != null)
                    {
                        var nameMapNorm = _info2.Materials[polygon.Material].NormalMap;
                        var textureXNorm = (int)Math.Ceiling(nameMapNorm != "" ? GetWidth(nameMapNorm) * pixelTexture.X : 1023f * pixelTexture.X);
                        var textureYNorm = (int)Math.Ceiling(nameMapNorm != "" ? GetHeight(nameMapNorm) * (1 - pixelTexture.Y) : 1023f * (1 - pixelTexture.Y));
                        var normalColor = GetPixelFromFile(_info2.Materials[polygon.Material].NormalMap, textureXNorm, textureYNorm);// normalMap[textureX, textureY];
                        // var normalColor = normalMapTexture.GetPixel(textureX, textureY);
                        var tempColor = new Vector4(normalColor.R, normalColor.G, normalColor.B, normalColor.A); // цвет пикселя карты нормалей
                        
                        tempColor = tempColor * 2 - new Vector4(255, 255, 255, 255); //преобразование из значения цвета в значение вектора
                        tempColor = Vector4.Transform(tempColor, //повернуть как надо
                            Matrix4x4.Multiply(Matrix4x4.Multiply(_rotationZMatrix, _rotationYMatrix),
                                _rotationXMatrix));

                        normalVector = Vector3.Normalize(tempColor.ConvertToVector3()); //
                    }
                    else
                    {
                        normalVector = Vector3.Normalize(pixelNormalVector);
                    }

                    var intensity = Vector3.Dot(normalVector, -light_dir); // (N*L)

                    intensity = intensity > 0 ? intensity : 0;

                    // рассеянное =  коэф рассеянного освещения * цвет рассеянного света * (N*L)
                    var diffuse = pixelColorVector  * intensity; // lightColor pixelColorVector * * _info2.Materials[polygon.Material].DiffuseColor.vecColor

                    var ks = new Vector4(0.1f, 0.1f, 0.1f, 255); //коэф зеркального освещения

                    if (specularMapTexture != null)
                    {
                        var bColor = specularMap[textureX, textureY];
                        // var bColor = specularMapTexture.GetPixel(textureX, textureY);
                        ks = new Vector4(bColor.R, bColor.G, bColor.B, bColor.A);
                    }

                     float gloss = _info2.Materials[polygon.Material].Shininess;// : 64; // коэф блеска поверхности
                    var viewDir = Vector3.Normalize(pixelWorldPosition.ConvertToVector3() - _cameraPosition); //вектор взгляда
                    var reflectionDir = light_dir - 2 * normalVector * (Vector3.Dot(light_dir, normalVector)); //вектор отражения

                    //зеркальное =  цвет зеркального света * (взгляда * отражение)^gloss * коэф зерк освещ
                    var specular = _info2.Materials[polygon.Material].SpecularColor.vecColor * (float) (Math.Pow(Math.Max(Vector3.Dot(viewDir, -reflectionDir), 0f),
                        gloss)) * ks; 
                    //_info2.Materials[polygon.Material].SpecularColor.vecColor
                    var resultColor = ambient + diffuse + specular;

                    resultColor.X = Math.Min(resultColor.X, 255);
                    resultColor.Y = Math.Min(resultColor.Y, 255);
                    resultColor.Z = Math.Min(resultColor.Z, 255);
                    resultColor.W = modelColor.W;

                    var x = (int) Math.Ceiling(P.X);
                    var y = (int) Math.Ceiling(P.Y);

                    if (zBuffer[x, y] > P.Z && P.Z >= 0 && P.Z < 1)
                    {
                        zBuffer[x, y] = P.Z;
                        source[x, y] = resultColor;
                    }
                }
            }
        }

        private float GetActualAspect()
        {
            return (float) imageGrid.ActualWidth / (float) imageGrid.ActualHeight;
        }

        private void ImageGrid_SizeChanged(object sender, RoutedEventArgs e)
        {
            Draw();
        }
    }
}