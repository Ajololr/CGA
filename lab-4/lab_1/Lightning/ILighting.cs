using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinForms3DModelViewer.Lightning
{
    public interface ILighting
    {
        Color GetPointColor(Vector3 normal, Color color);
    }
}
