# 0.5.5 — Opposite-corner markers / 对角角框

- 违禁品／赃物：右上、左下 L 形角框。
- 顾客需求：左上、右下 L 形角框，与风险标记共用同一外沿，不再内缩避让。
- 短边最多 8 UI 像素且不超过格子边长的四分之一；异形物品选实际占格角落，避免相邻物品交叉。
- 容器需求角标移到左上有效占格内侧。颜色、线宽设置与 D/V 操作沿用。

Risk markers use top-right/bottom-left L corners; demand markers use top-left/bottom-right at the same outer inset. Short arms stay inside occupied cells, including irregular and rotated items. Nested-demand badges move inside the top-left occupied cell.

Validation: full-game compilation (0 warnings/errors), 199 logic checks and native binding checks passed. On 2026-11-07 the user tested the installed corner markers and reported no issues with this design, then authorized Workshop publication. This is not an exhaustive test of every configuration.
