using System.Collections.Generic;

namespace BetterHotkeys
{
    // Coordinates are local to the transformed shape, with Y increasing downward.
    internal static class OutlineGeometry
    {
        internal readonly struct Strip
        {
            internal readonly float X, Y, Width, Height;
            internal Strip(float x, float y, float width, float height)
            { X = x; Y = y; Width = width; Height = height; }
        }

        internal static List<Strip> Build(bool[,] cells, float cell, float inset, float width)
        {
            var result = new List<Strip>();
            var columns = cells.GetLength(0); var rows = cells.GetLength(1);
            bool Filled(int x, int y) => x >= 0 && y >= 0 && x < columns && y < rows && cells[x, y];
            for (var y = 0; y < rows; y++)
                for (var x = 0; x < columns; x++)
                {
                    if (!cells[x, y]) continue;
                    var left = !Filled(x - 1, y); var right = !Filled(x + 1, y);
                    var top = !Filled(x, y - 1); var bottom = !Filled(x, y + 1);
                    var x0 = x * cell + (left ? inset : 0);
                    var x1 = (x + 1) * cell - (right ? inset : 0);
                    var y0 = y * cell + (top ? inset : 0);
                    var y1 = (y + 1) * cell - (bottom ? inset : 0);
                    if (top) result.Add(new Strip(x0, y0, x1 - x0, width));
                    if (bottom) result.Add(new Strip(x0, y1 - width, x1 - x0, width));
                    if (left) result.Add(new Strip(x0, y0, width, y1 - y0));
                    if (right) result.Add(new Strip(x1 - width, y0, width, y1 - y0));
                }
            return result;
        }
    }
}
