using BetterHotkeys;

internal static class OutlineChecks
{
    internal static void Run(Action<string, bool> check)
    {
        var riskCorners = OutlineCorners.TopRightBottomLeft;
        var demandCorners = OutlineCorners.TopLeftBottomRight;
        var single = new bool[,] { { true } };
        var risk = OutlineGeometry.Build(single, 32, .5f, 1, riskCorners);
        var demand = OutlineGeometry.Build(single, 32, .5f, 1, demandCorners);
        check("risk L marks occupy top-right and bottom-left only",
            risk.Count == 4 && risk[0].X == 23.5f && risk[0].Y == .5f &&
            risk[1].X == 30.5f && risk[2].X == .5f && risk[2].Y == 30.5f && risk[3].Y == 23.5f);
        check("demand L marks occupy top-left and bottom-right at the same outer inset",
            demand.Count == 4 && demand[0].X == .5f && demand[0].Y == .5f &&
            demand[1].X == .5f && demand[2].X == 23.5f && demand[2].Y == 30.5f && demand[3].X == 30.5f);
        check("corners retain one-pixel thickness and eight-pixel arms",
            risk.Concat(demand).All(s => (s.Width == 1 && s.Height == 8) || (s.Width == 8 && s.Height == 1)));
        check("risk and demand corners remain separate on the same item", !Overlap(risk, demand));
        check("empty shapes have no corners", OutlineGeometry.Build(new bool[2, 2], 32, .5f, 1, riskCorners).Count == 0);
        var rectangle = new bool[,] { { true, true }, { true, true }, { true, true } };
        var border = OutlineGeometry.Build(rectangle, 32, .5f, 1, riskCorners);
        check("large rectangular items have exactly two short L marks", border.Count == 4 &&
            border[0].X == 87.5f && border[0].Y == .5f && border[2].X == .5f && border[2].Y == 62.5f);
        var tee = new bool[,] { { false, true, false }, { true, true, true }, { false, true, false } };
        var teeBorder = OutlineGeometry.Build(tee, 32, .5f, 1, riskCorners);
        check("empty bounding-box corners relocate to occupied extreme rows",
            teeBorder[0].X == 55.5f && teeBorder[2].X == 32.5f && teeBorder[2].Y == 94.5f);

        // Exhaust all 3x3 occupancy patterns, including holes, interlocking L/T shapes,
        // disconnected components and every rotation/reflection, at all width settings.
        var contained = true; var disjoint = true; var independent = true;
        for (var mask = 0; mask < 512; mask++)
            foreach (var cell in new[] { 16f, 32f, 48f })
                for (var setting = 1; setting <= 4; setting++)
                {
                    var a = new bool[3, 3]; var b = new bool[3, 3];
                    for (var y = 0; y < 3; y++)
                        for (var x = 0; x < 3; x++)
                        { a[x, y] = (mask & (1 << (y * 3 + x))) != 0; b[x, y] = !a[x, y]; }
                    var ar = OutlineGeometry.Build(a, cell, .5f, setting * .5f, riskCorners);
                    var ad = OutlineGeometry.Build(a, cell, .5f, 1, demandCorners);
                    var br = OutlineGeometry.Build(b, cell, .5f, setting * .5f, riskCorners);
                    var bd = OutlineGeometry.Build(b, cell, .5f, 1, demandCorners);
                    contained &= ar.Concat(ad).All(s => WithinOccupiedCell(s, a, cell));
                    disjoint &= !Overlap(ar.Concat(ad), br.Concat(bd));
                    independent &= !Overlap(ar, ad);
                }
        check("all 3x3 contours stay inside their own occupied cells", contained);
        check("interlocking neighboring items never have crossing outlines", disjoint);
        check("risk and demand corners never overlap for any tested shape or width", independent);
    }

    private static bool WithinOccupiedCell(OutlineGeometry.Strip s, bool[,] cells, float cell)
    {
        if (s.Width <= 0 || s.Height <= 0) return false;
        for (var y = 0; y < cells.GetLength(1); y++)
            for (var x = 0; x < cells.GetLength(0); x++)
                if (cells[x, y] && s.X >= x * cell && s.Y >= y * cell &&
                    s.X + s.Width <= (x + 1) * cell && s.Y + s.Height <= (y + 1) * cell) return true;
        return false;
    }

    private static bool Overlap(IEnumerable<OutlineGeometry.Strip> a, IEnumerable<OutlineGeometry.Strip> b) =>
        a.Any(x => b.Any(y => x.X < y.X + y.Width && y.X < x.X + x.Width &&
            x.Y < y.Y + y.Height && y.Y < x.Y + x.Height));
}
