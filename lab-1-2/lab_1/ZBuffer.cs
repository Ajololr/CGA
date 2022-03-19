using System;

namespace lab_1
{
    public class ZBuffer
    {
        private double[,] _buffer;
        public int Height;
        public int Width;

        public ZBuffer(int height, int width)
        {
            Height = height;
            Width = width;
            _buffer = new double[height + 1, width + 1];
            for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                _buffer[i, j] = double.MaxValue;
        }

        public double this[int x, int y]
        {
            get 
            {
                if (IsValidPoint(x, y))
                    return _buffer[y, x];
                else
                    throw new Exception();
            }
            set
            {
                if (IsValidPoint(x, y))
                    _buffer[y, x] = value;
                else
                    throw new Exception();
            }
        }

        private bool IsValidPoint(int x, int y)
        {
            return y >= 0 && x >= 0 && x <= Width && y <= Height;
        }
    }
}