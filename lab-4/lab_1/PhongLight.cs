using System;
using System.Drawing;
using System.Numerics;

namespace lab_1
{
    public class PhongLight
    {
        readonly Vector3 _lightVector;
        readonly Vector3 _eye;
        readonly Vector3 _ambientRatio;
        readonly Vector3 _diffuseRatio;
        readonly Vector3 _mirrorRatio;
        readonly float   _shiness;
        readonly Model   _model;
        public PhongLight(
            Vector3 lightVector,
            Vector3 eye,
            Vector3 ambientRatio,
            Vector3 diffuseRatio,
            Vector3 mirrorRatio,
            float shiness,
            Model model)
        {
            _lightVector = lightVector;
            _eye = eye;
            _ambientRatio = ambientRatio;  
            _diffuseRatio = diffuseRatio;  
            _mirrorRatio = mirrorRatio;
            _shiness = shiness;
            _model = model;
        }

        public Color GetPointColor(Vector3 point, float w, Vector3 texel)
        {
            texel /= w;
            var x = (texel.X * _model.DiffuseTexture.Width) % _model.DiffuseTexture.Width;
            var y = ((1 - texel.Y) * _model.DiffuseTexture.Height) % _model.DiffuseTexture.Height;
            
            var color = _model.DiffuseTexture.GetPixel((int)x, (int)y);

            var colorVector = new Vector3(color.R, color.G, color.B);
            
            var Ia = _ambientRatio * colorVector;
            
            var normalColor = _model.NormalsTexture.GetPixel((int)x, (int)y);
            var normal = new Vector3(normalColor.R, normalColor.G, normalColor.B);
            normal = 2 * normal / 255 - Vector3.One;
            normal = Vector3.Normalize(normal);
            
            var Id = new Vector3(color.R, color.G, color.B) * _diffuseRatio * Math.Max(Vector3.Dot(normal, Vector3.Normalize(_lightVector)), 0);
            
            var intensity = Vector3.Dot(_lightVector, normal);
            var reflectionVector = Vector3.Normalize(Vector3.Reflect(-_lightVector, normal));
            var specularColor = _model.SpecularTexture.GetPixel((int)x, (int)y);
            var specularColorVector = new Vector3(specularColor.R, specularColor.G, specularColor.B);
            
            var Is = intensity > 0 
                ? specularColorVector * 
                  _mirrorRatio * 
                  (float)Math.Pow(Math.Max(0, Vector3.Dot(reflectionVector, Vector3.Normalize(_eye-point))), _shiness)
                : Vector3.Zero;

            var I = Ia + Id + Is;

            byte r = (byte)Math.Min(I.X, 255);
            byte g = (byte)Math.Min(I.Y, 255);
            byte b = (byte)Math.Min(I.Z, 255);

            return Color.FromArgb(255, r, g, b);
        }
    }
}