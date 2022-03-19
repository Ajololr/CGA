using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace ObjParser
{
    internal class MaterialParser
    {
        public async Task<ObjMaterial[]> ReadFromFile(string materialFile)
        {
             var file =  File.OpenRead(materialFile);

            var builder = new MaterialBuilder();

            await file.ReadToEnd(line =>
            {
                if (!line.StartsWith("#") && line.Length > 0)
                {
                    ParseMaterial(builder, line);
                }
                return Task.CompletedTask;
            });
            return builder.Build();
        }

        private static void ParseMaterial(MaterialBuilder builder, in string line)
        {
            var splitLine = line.Split(' ');
            splitLine = splitLine.Where(
                val => val != "").ToArray();
            var command = splitLine[0];
            var values = new ReadOnlySpan<string>(splitLine, 1, splitLine.Length - 1);
            switch (command.Trim().ToLower())
            {
                case "newmtl": builder.NewMaterial(values); break;
                case "ns": builder.SetShininess(values);break;
                case "d": builder.SetAlpha(values); break;
                case "tr": builder.SetTransparency(values); break;
                case "illum": builder.SetIllumination(values); break;
                case "ka": builder.SetAmbientColor(values); break;
                case "kd": builder.SetDiffuseColor(values); break;
                case "ks": builder.SetSpecularColor(values); break;
                case "ke": builder.SetEmissiveColor(values); break;

                case "map_ka": builder.SetAmbientMap(values); break;
                case "map_kd": builder.SetDiffuseMap(values); break;
                case "map_d": builder.SetAlphaMap(values); break;
                case "map_bump": builder.SetBumpMap(values); break;
                case "bump": builder.SetBumpMap(values); break; 
                case "map_norm": builder.SetNormalMap(values); break;
                // Ignored values
                case "ni": break;
                case "tf": break;
                default: Console.WriteLine($"{command} is not recognized as a material identifier"); break;
            }
        }
    }
}