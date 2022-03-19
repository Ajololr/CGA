using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace lab_1
{   
    public partial class MainForm : Form
    {
        private Model model;
        const string path = @"E:\University\7 sem\ACG\Cube\Cube3.obj";

        Point lastPoint = Point.Empty;
        bool isMouseDown;

        Vector3 viewPoint = new Vector3(0, 0, 100);
        
        float alpha = 1f;

        float xRotation;
        float yRotation;

        public void Render()
        {
            var modelProps = new ModelProps()
            {
                ImageHeight = pictureBoxPaintArea.Height,
                ImageWidth = pictureBoxPaintArea.Width,
                CameraX = 0,
                CameraY = 0,
                CameraZ = viewPoint.Z,
                CameraXRotation = xRotation,
                CameraYRotation = yRotation,
                CameraZRotation = 0,
                Light =  new Vector3(0, 1000, 0),
            };

            var modelDrawer = new ModelDrawer(model, modelProps);

            pictureBoxPaintArea.Image = modelDrawer.Draw();
        }

        public MainForm()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            InitializeComponent();
            pictureBoxPaintArea.MouseWheel += _MouseWheel;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Parser parser = new Parser();
            model = parser.ParseFileToModel(path);
        }

        private void _MouseWheel(object sender, MouseEventArgs e)
        {   
            viewPoint.Z -= (e.Delta / 50.0f);
        }

        private void pictureBoxPaintArea_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            lastPoint = Point.Empty;
        }

        private void pictureBoxPaintArea_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = e.Location;
            isMouseDown = true;

        }

        private void pictureBoxPaintArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown && lastPoint != Point.Empty)
            {
                Vector3 v = new Vector3(e.X - lastPoint.X, e.Y - lastPoint.Y, 0);
                xRotation += v.Y;
                yRotation += v.X;
                lastPoint = e.Location;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Keys.W))
            {
                xRotation -= alpha;
            };
            if (Keyboard.IsKeyDown(Keys.S))
            {
                xRotation += alpha;
            };
            if (Keyboard.IsKeyDown(Keys.D))
            {
                yRotation -= alpha;
            };
            if (Keyboard.IsKeyDown(Keys.A))
            {
                yRotation += alpha;
            };

            Render();
        }

        private void pictureBoxPaintArea_Click(object sender, EventArgs e)
        {

        }
    }
}
