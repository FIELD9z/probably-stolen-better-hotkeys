# Better Hotkeys / 更好的快捷键 — 0.2.3

Author / 作者：FIELD9z。适用本机正式版内部版本 0.46D。

## 中文

在 Mods 中打开 Better Hotkeys 的设置，修改后点击应用。设置跨存档保存；当前鼠标手势或拖动结束后生效。语言可选 Auto（跟随游戏，中文以外使用英文）、English、简体中文；修改语言后重新打开面板。


### 默认操作

| 操作 | 效果 |
| --- | --- |
| Shift＋左键 | 库存/自有容器→出售柜台；出售柜台→库存 |
| Ctrl＋左键 | 库存→最近点击的自有打开容器；容器→库存；柜台不执行 |
| Alt＋左键 | 固定或解除固定；点击选区内物品时操作同来源选区 |
| 悬停标题栏＋游戏整理键（默认 S） | 整理该容器，默认记住最近一次原生菜单排序方式 |
| 双击 B | 开关赃物/违禁品标记；默认黄/红/紫分别表示赃物/违禁品/两者 |

固定物品不会被转移。目标不足时允许部分接收，不自动绕路或整理目标。普通拖动、滚轮数量调整及 Shift 拖动半堆保留；不新增单取键。

### 设置

- 常规：语言、输入模式，转移、固定、标题栏整理、标记分别启停。
- 按键：五个动作支持文本组合键，如 Shift、RightAlt、Ctrl+Q、B；None 或空白解除绑定。可用 A–Z、F1–F12（F8/F9 保留）、Space、Tab、BackQuote、Home、End、PageUp、PageDown、Insert 以及 Shift/Ctrl/Alt 的左右变体；最多一个普通键。T 被本机游戏硬编码占用，运行时提示冲突并不执行。左右等价默认开启；关闭后 Shift/Ctrl/Alt 表示左侧。
- 标记触发：Press 单按开关、DoublePress 双击开关、Hold 按住显示；双击间隔 0.15–0.8 秒。
- 行为：同来源批量操作；仅标题栏或容器任意区域指定目标；目标边框；记住最近排序或固定使用大/小件优先、起始/末端。
- 标记：两类分别启停、嵌套汇总、仅自有物品、三种分类配色（七色预设）、宽度 1–4、透明度 0.2–1、扫描间隔 0.1–0.5 秒。扫描不会执行盘查或改变热度。
- 提示：转移 Exceptions（仅异常，默认）、All、Off；目标变化和标记开关分别启停。

按键字段是文本输入，不是按键捕获控件。应用时拒绝无效或重叠绑定，恢复上一套有效配置；重新打开面板查看恢复后的值。外部冲突在触发动作时检查并说明对象，不自动替换绑定。

### 兼容模式

TakeOver 默认接管原生快速转移。关闭助手转移或解绑某个动作仍不会恢复原生转移。

VanillaWithHelpers 交还原生快速转移，同时关闭本 Mod 转移/固定手势及拖动数量覆盖；标题栏整理与标记仍可使用。要彻底停用所有补丁行为，可在 Mods 停用本 Mod。

本 Mod 检查公开游戏绑定、可用储藏协调器预设，并在启用时记录其他 Mod 对相同关键输入方法的补丁。它无法完整发现其他 Mod 在独立 Update 中直接读取按键，不能保证所有组合兼容。快捷转移手势期间会消费原生输入分发，其他输入 Mod 可能受影响。

## English

Open Better Hotkeys settings in Mods and click Apply. Settings persist across saves. Changes take effect after the current mouse gesture/drag ends. Auto follows the game language (Chinese or English fallback); reopen the panel after changing language.


### Default controls

| Control | Action |
| --- | --- |
| Shift + left click | Inventory/owned container → sales counter; sales counter → inventory |
| Ctrl + left click | Inventory → last clicked open owned container; container → inventory; no route from counter |
| Alt + left click | Pin/unpin; selected items in the same direct inventory are processed together |
| Hover header + game sort key (default S) | Sort that inventory; remembers the last vanilla menu sort choice by default |
| Double B | Toggle stolen/contraband markers: yellow/red/purple for stolen/contraband/both |

Pinned items stay put. Partial transfers are allowed; no automatic rerouting or destination sorting. Normal dragging, wheel quantity adjustment and Shift half-stack dragging remain available. No new single-item key is added.

### Configuration

- General: language, input mode, separate transfer/pin/header-sort/marker toggles.
- Keys: editable text chords such as Shift, RightAlt, Ctrl+Q or B. None/blank clears a binding. Supports A–Z, F1–F12 except reserved F8/F9, Space, Tab, BackQuote, Home, End, PageUp, PageDown, Insert, and left/right Shift/Ctrl/Alt. At most one ordinary key per chord. T conflicts with the game's hardcoded mouse-smoothing toggle and is blocked at runtime. Left/right equivalence is on by default; when off, unqualified modifiers mean left-side keys.
- Markers: Press to toggle, DoublePress to toggle, or Hold to show; adjustable double-press interval (0.15–0.8 seconds).
- Behavior: same-inventory batch actions, header-only target selection, target highlight, remember menu sorting or use a fixed large/small-first and start/end preference.
- Appearance: stolen/contraband toggles, nested contents, owned-only filter, seven color presets for each category, border width (1–4), opacity (0.2–1), refresh interval (0.1–0.5 seconds). Read-only markers do not run security inspections or change heat.
- Notices: Exceptions (default), All or Off for transfers; independent target-change and marker-toggle notices.

Bindings use text fields, not key-capture widgets. Invalid/overlapping bindings restore the previous valid configuration; reopen the panel to see restored values. External conflicts are checked when an action is attempted; the conflicting action is named and the shortcut is skipped.

### Compatibility

TakeOver (default) disables vanilla quick transfer. Turning off our transfer module or clearing a binding does not restore vanilla routes.

VanillaWithHelpers restores vanilla transfer and disables our transfer/pin gestures and drag-quantity override, while retaining header sorting and markers. Disabling this mod in Mods makes its patches inactive.

Checks cover public game bindings, available storage-coordinator presets and diagnostic logging of shared critical Harmony patch targets. Other mods that poll keys independently cannot be fully detected. Transfer gestures consume native input dispatch and may affect other input mods. No universal compatibility guarantee is made.

## Validation / 验证

本次 0.2.2 保留 0.2.1 原有转移、固定、整理、标记与设置功能范围；移除原生快捷提示面板接管。自定义快捷键请以 Mod 设置和本文为准。0.3/0.4 开发功能不包含在此维护版本。编译与逻辑检查不等于游戏内验证。

Version 0.2.2 retains the established transfer, pin, sorting, marker and settings features. Native shortcut hints are no longer replaced; consult Mod settings and this guide for custom bindings. Development features from 0.3/0.4 are not part of this maintenance release. Build and logic checks do not replace in-game verification.

[Source / 源代码](https://github.com/FIELD9z/probably-stolen-better-hotkeys)


Source / 源码：[GitHub](https://github.com/FIELD9z/probably-stolen-better-hotkeys/tree/codex/0.2.2-text-fix)。本分支为 0.2.2 维护版；主分支含开发功能。


0.2.3 操作提示修正：整理冲突提示仅在实际整理上下文触发；缺少目标时按标题栏设置给出指引；标记开启提示反映类别开关并翻译颜色；转移摘要区分数量与物品堆、限定本次处理范围；无配置变更时不再复用上一次应用结果。未加入独立动态说明窗口。

0.2.3 notice fixes: sorting conflicts are checked only in an actionable header context; destination guidance respects header-only selection; marker notices reflect enabled categories and localize color names; transfer summaries distinguish units from stacks and refer only to the current operation; applying unchanged settings no longer repeats stale feedback. No independent dynamic guide is included.


0.2.3 source / 对应源码：[v0.2.3](https://github.com/FIELD9z/probably-stolen-better-hotkeys/tree/v0.2.3)。
