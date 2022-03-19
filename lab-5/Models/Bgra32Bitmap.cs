using System.Numerics;
using System.Windows.Media.Imaging;

namespace Viewer3D.Models
{
    public class Bgra32Bitmap
    {
        private readonly long _backBuffer;
        private readonly int _backBufferStride;
        private readonly int _bytesPerPixel;

        public readonly int PixelWidth;
        public readonly int PixelHeight;

        public Bgra32Bitmap(WriteableBitmap source)
        {
            Source = source;
            PixelHeight = source.PixelHeight;
            PixelWidth = source.PixelWidth;
            _backBuffer = source.BackBuffer.ToInt64();
            _backBufferStride = source.BackBufferStride;
            _bytesPerPixel = source.Format.BitsPerPixel / 8;
        }

        public WriteableBitmap Source { get; }

        public unsafe Vector4 this[int x, int y]
        {
            get
            {
                var address = GetAddress(x, y);

                return new Vector4
                {
                    X = address[2],
                    Y = address[1],
                    Z = address[0],
                    W = address[3]
                };
            }

            set
            {
                if (x < 0 || x >= PixelWidth || y < 0 || y >= PixelHeight)
                {
                    return;
                }

                var address = GetAddress(x, y);

                address[0] = (byte) value.Z;
                address[1] = (byte) value.Y;
                address[2] = (byte) value.X;
                address[3] = (byte) value.W;
            }
        }

        private unsafe byte* GetAddress(int x, int y)
        {
            return (byte*) (_backBuffer + y * _backBufferStride + x * _bytesPerPixel);
        }
    }
}