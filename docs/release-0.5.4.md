# 0.5.4 — Thinner item outlines / 更细的物品轮廓

- 顾客需求框由 2 改为 1 UI 像素并向外靠近占格边缘。
- 赃物／违禁品框的现有 1–4 档改为实际 0.5–2 UI 像素，默认 1；保留用户配置值。
- 沿实际占格轮廓绘制，支持旋转和异形物品交错摆放；线条限制在自身占格内，两类颜色保持分离。
- 容器需求角标缩为 5×5，放在实际占格内。

Demand outlines are now 1 UI pixel, closer to the cell boundary. Risk outlines use half the previous thickness while preserving saved width levels. Both follow occupied cells rather than bounding rectangles, avoiding crossings between adjacent non-overlapping items. Nested-demand badges are smaller and positioned in an occupied cell.

Validation: full-game compilation (0 warnings/errors), 197 logic checks and native binding checks passed. Geometry checks exhaust all 512 3×3 occupancy patterns at three cell sizes and four width levels. Final Unity rendering and visual comfort require in-game verification. Workshop publication is not part of this patch installation.
