using System;
using System.Drawing;
using System.Numerics;

namespace lab_1
{
    public class PhongLight
    {
        readonly Vector3 _lightVector;
        readonly Vector3 _viewVector;
        readonly Vector3 _ambientRatio;
        readonly Vector3 _diffuseRatio;
        readonly Vector3 _mirrorRatio;
        readonly Vector3 _ambientColor;
        readonly Vector3 _reflectionColor;
        readonly Vector3 _diffuseColor;
        readonly float   _shiness;

        public PhongLight(
            Vector3 lightVector,
            Vector3 viewVector,
            Vector3 ambientRatio,
            Vector3 diffuseRatio,
            Vector3 mirrorRatio,
            Vector3 ambientColor,
            Vector3 diffuseColor,
            Vector3 reflectionColor,
            float shiness)
        {
            _lightVector = lightVector;
            _viewVector = viewVector;
            _ambientRatio = ambientRatio;  
            _diffuseRatio = diffuseRatio;  
            _mirrorRatio = mirrorRatio;
            _diffuseColor = diffuseColor;
            _ambientColor = ambientColor;
            _reflectionColor = reflectionColor;
            _shiness = shiness;
        }

        public Color GetPointColor(Vector3 point, Vector3 normal)
        {
            var Ia = _ambientRatio * _ambientColor;
            var Id = _diffuseColor * _diffuseRatio * Math.Max(Vector3.Dot(normal, Vector3.Normalize(_lightVector)), 0);
            
            var reflectionVector = Vector3.Normalize(Vector3.Reflect(-_lightVector, normal));
            var intensity = Vector3.Dot(_lightVector, normal);
            
            var Is = intensity > 0 ? _reflectionColor * _mirrorRatio * (float)Math.Pow(Math.Max(0, Vector3.Dot(reflectionVector, Vector3.Normalize(_viewVector-point))), _shiness) : Vector3.Zero;

            var I = Ia + Id + Is;

            byte r = (byte)Math.Min(I.X, 255);
            byte g = (byte)Math.Min(I.Y, 255);
            byte b = (byte)Math.Min(I.Z, 255);

            return Color.FromArgb(255, r, g, b);
        }
    }
}