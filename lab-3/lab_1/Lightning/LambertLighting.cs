using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3DModelViewer.Lightning
{
    public class LambertLighting : ILighting
    {
        private Vector3 lightVector;

        public LambertLighting(Vector3 vector)
        {
           lightVector = vector;
        }

        public Color GetPointColor(Vector3 normal, Color color)
        {
            double coef = Math.Max(Vector3.Dot(normal, Vector3.Normalize(lightVector)), 0);
            byte r = (byte)Math.Round(color.R * coef);
            byte g = (byte)Math.Round(color.G * coef);
            byte b = (byte)Math.Round(color.B * coef);

            return Color.FromArgb(255, r, g, b);
        }
    }
}
