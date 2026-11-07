using BetterHotkeys;

internal static class OutlineChecks
{
    internal static void Run(Action<string, bool> check)
    {
        var single = new bool[,] { { true } };
        var risk = OutlineGeometry.Build(single, 32, .5f, 1);
        var demand = OutlineGeometry.Build(single, 32, 2, 1);
        check("default risk outline is one pixel with half-pixel outer clearance",
            risk.Count == 4 && risk[0].Y == .5f && risk[0].Height == 1 && risk[0].X == .5f && risk[0].Width == 31);
        check("demand outline moves outward and uses one pixel",
            demand.Count == 4 && demand[0].Y == 2 && demand[0].Height == 1);
        check("risk and demand remain separate on the same item", !Overlap(risk, demand));
        check("empty shapes have no outline", OutlineGeometry.Build(new bool[2, 2], 32, .5f, 1).Count == 0);

        var rectangle = new bool[,] { { true, true }, { true, true }, { true, true } };
        var border = OutlineGeometry.Build(rectangle, 32, .5f, 1);
        check("solid multi-cell item has no internal grid lines", border.All(s =>
            s.X == .5f || s.Y == .5f || s.X + s.Width == 95.5f || s.Y + s.Height == 63.5f));

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
                    var ar = OutlineGeometry.Build(a, cell, .5f, setting * .5f);
                    var ad = OutlineGeometry.Build(a, cell, 1 + setting * .5f, 1);
                    var br = OutlineGeometry.Build(b, cell, .5f, setting * .5f);
                    var bd = OutlineGeometry.Build(b, cell, 1 + setting * .5f, 1);
                    contained &= ar.Concat(ad).All(s => WithinOccupiedCell(s, a, cell));
                    disjoint &= !Overlap(ar.Concat(ad), br.Concat(bd));
                    independent &= !Overlap(ar, ad);
                }
        check("all 3x3 contours stay inside their own occupied cells", contained);
        check("interlocking neighboring items never have crossing outlines", disjoint);
        check("risk and demand lanes never overlap for any tested shape or width", independent);
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
