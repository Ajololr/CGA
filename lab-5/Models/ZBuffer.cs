namespace Viewer3D.Models
{
    public class ZBuffer
    {
        private readonly int _pixelWidth;
        private readonly int _pixelHeight;
        private readonly float[] _buffer;

        public ZBuffer(int width, int height)
        {
            _buffer = new float[width * height];

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    _buffer[i * width + j] = float.PositiveInfinity;
                }
            }

            _pixelWidth = width;
            _pixelHeight = height;
        }

        private int GetAddress(int x, int y)
        {
            return y * _pixelWidth + x;
        }

        public float this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= _pixelWidth || y < 0 || y >= _pixelHeight)
                {
                    return float.NegativeInfinity;
                }

                var address = GetAddress(x, y);

                return _buffer[address];
            }

            set
            {
                if (x < 0 || x >= _pixelWidth || y < 0 || y >= _pixelHeight)
                {
                    return;
                }

                var address = GetAddress(x, y);

                _buffer[address] = value;
            }
        }
    }
}