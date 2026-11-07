using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BetterHotkeys
{
    internal sealed class ItemOutline : IDisposable
    {
        internal const float OuterInset = .5f;
        internal static float RiskWidth => Settings.Current.Width * .5f;
        internal static float DemandInset => OuterInset + RiskWidth + .5f;
        private readonly List<Image> edges = new List<Image>();
        internal readonly RectTransform Rect;
        private readonly float inset, width;
        private bool[,] cells = new bool[0, 0];
        private float cellSize;
        private Color color;
        private bool visible;
        internal bool Alive => Rect != null;
        internal Vector2 BadgePosition { get; private set; }

        internal ItemOutline(RectTransform parent, string name, float inset, float width)
        {
            this.inset = inset; this.width = width;
            Rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            Rect.SetParent(parent, false);
            Rect.anchorMin = Rect.anchorMax = Rect.pivot = new Vector2(0, 1);
            Rect.anchoredPosition = Vector2.zero;
        }

        internal void Geometry(GridShape shape)
        {
            var columns = shape.globalWidth; var rows = shape.globalHeight;
            var cell = UISettings.current.gridSize;
            var changed = cell != cellSize || cells.GetLength(0) != columns || cells.GetLength(1) != rows;
            if (cells.GetLength(0) != columns || cells.GetLength(1) != rows) cells = new bool[columns, rows];
            for (var y = 0; y < rows; y++)
                for (var x = 0; x < columns; x++)
                {
                    // Native Intersects uses exactly value 1 for an occupied cell.
                    var filled = shape.Get(shape.minX + x, shape.minY + y) == 1;
                    changed |= cells[x, y] != filled;
                    cells[x, y] = filled;
                }
            if (!changed) return;
            cellSize = cell;
            Rect.sizeDelta = new Vector2(columns * cell, rows * cell);
            var strips = OutlineGeometry.Build(cells, cell, inset, width);
            for (var i = 0; i < strips.Count; i++)
            {
                if (i == edges.Count)
                {
                    var obj = new GameObject("Outline edge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    obj.transform.SetParent(Rect, false);
                    var image = obj.GetComponent<Image>(); image.raycastTarget = false;
                    edges.Add(image);
                }
                var rect = edges[i].rectTransform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
                var strip = strips[i];
                rect.anchoredPosition = new Vector2(strip.X, -strip.Y);
                rect.sizeDelta = new Vector2(strip.Width, strip.Height);
                edges[i].color = color;
                edges[i].gameObject.SetActive(visible);
            }
            for (var i = edges.Count - 1; i >= strips.Count; i--)
            {
                edges[i].gameObject.SetActive(false);
                UnityEngine.Object.Destroy(edges[i].gameObject);
                edges.RemoveAt(i);
            }
            // Put the nested-content badge in a real cell, never in a shape's empty corner.
            for (var y = 0; y < rows; y++)
                for (var x = columns - 1; x >= 0; x--)
                    if (cells[x, y]) { BadgePosition = new Vector2((x + 1) * cell - inset, -y * cell - inset); return; }
        }

        internal void Display(Color value, bool show)
        {
            color = value; visible = show;
            foreach (var edge in edges) { edge.color = color; edge.gameObject.SetActive(show); }
        }

        public void Dispose()
        {
            if (Rect != null) { Rect.gameObject.SetActive(false); UnityEngine.Object.Destroy(Rect.gameObject); }
        }
    }
}
