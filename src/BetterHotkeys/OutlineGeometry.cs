using System;
using System.Collections.Generic;

namespace BetterHotkeys
{
    internal enum OutlineCorners { TopRightBottomLeft, TopLeftBottomRight }

    // Coordinates are local to the transformed shape, with Y increasing downward.
    internal static class OutlineGeometry
    {
        internal readonly struct Strip
        {
            internal readonly float X, Y, Width, Height;
            internal Strip(float x, float y, float width, float height)
            { X = x; Y = y; Width = width; Height = height; }
        }

        internal static List<Strip> Build(bool[,] cells, float cell, float inset, float width, OutlineCorners corners)
        {
            var result = new List<Strip>();
            var columns = cells.GetLength(0); var rows = cells.GetLength(1);
            var length = Math.Min(8f, cell * .25f);
            void Corner(int x, int y, bool right, bool bottom)
            {
                var x0 = x * cell + inset; var x1 = (x + 1) * cell - inset;
                var y0 = y * cell + inset; var y1 = (y + 1) * cell - inset;
                result.Add(new Strip(right ? x1 - length : x0, bottom ? y1 - width : y0, length, width));
                result.Add(new Strip(right ? x1 - width : x0, bottom ? y1 - length : y0, width, length));
            }
            // Extreme occupied rows provide actual convex corners even when bounding-box
            // corners are empty. Each short L stays inside one occupied cell.
            bool AddRow(bool bottom, bool right)
            {
                for (var row = 0; row < rows; row++)
                {
                    var y = bottom ? rows - 1 - row : row;
                    for (var column = 0; column < columns; column++)
                    {
                        var x = right ? columns - 1 - column : column;
                        if (!cells[x, y]) continue;
                        Corner(x, y, right, bottom);
                        return true;
                    }
                }
                return false;
            }
            var risk = corners == OutlineCorners.TopRightBottomLeft;
            if (AddRow(false, risk)) AddRow(true, !risk);
            return result;
        }
    }
}
