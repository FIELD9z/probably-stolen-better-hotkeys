# Better Hotkeys & Storage / 更好的快捷键与收纳 — 0.5.0

Author / 作者：FIELD9z。适用本机正式版内部版本 0.46D。

## 中文

在 Mods 中打开 Better Hotkeys 的设置，修改后点击应用。设置跨存档保存；当前鼠标手势或拖动结束后生效。语言可选 Auto（跟随游戏，中文以外使用英文）、English、简体中文；修改语言后重新打开面板。

历史行为（已在 0.4.1 移除）：0.2.1 的原生操作提示面板的快捷键区域在 Mod 实际启用时显示动态列表，使用当前已生效的绑定，约 0.15 秒刷新；关闭、未绑定和冲突状态明确显示。接管模式显示助手动作，原生＋辅助模式显示原生转移键。改键后无需重启；停用 Mod 或输入模块故障导致停用时恢复原生区域。其他店铺说明不改动。此提示布局仍待游戏内验证。

### 0.4.0 物品规则与虚影

- 默认键为 **D** 需求高亮、**C** 按规则收纳。
- 容器／每页可选择“物品＋标签”或“仅允许物品白名单”。点击“点选添加允许物品／拒绝物品”后，面板暂时隐藏；连续左键点击真实物品记录**同种物品**，数量、朝向和位置不影响识别。右键完成并返回草稿，再点保存生效，取消不保存规则。原生分页可以切换；即使切换目标窗口页签，草稿仍属于打开编辑器时的原始页。
- 容器顶部独立预览区分允许／拒绝两行，白边／黑边的半透明图标按添加顺序排列。可横向滚动；悬停查看名称，点击移除该条规则（立即保存）。虚影不是游戏物品，不占格子和容量。小箭头展开标签逻辑，长说明可悬停查看。图标资源缺失时显示问号，规则保留。
- 对每个待收物品，先排除所有命中黑名单的容器，再尝试明确白名单目标，之后才尝试标签目标；“仅白名单”目标不进入标签阶段。同一物品同时在白／黑名单中时拒绝优先。各层内再按数字优先级与稳定顺序分配。
- 白名单满后继续标签目标；设置中的“无可用规则目标时使用空规则容器”默认关闭，打开后最后尝试既无标签又无物品规则的容器。仅设置名称的容器仍属于空规则；仅白名单模式或仅含黑名单的容器不属于空规则。所有阶段遵守 Open／All 范围。
- “仅白名单但名单为空”“白名单全部被拒绝”“混合模式没有接收规则”会出现配置提醒。普通空规则容器显示“未配置”；满空间及当前没有匹配品不是配置错误。
- 本 Mod **Ctrl＋左键明确目标转移**遇到拒收规则时，每批提醒一次，列出冲突种类和数量。可选“仅转移符合项／全部强制放入／取消”。只有选择强制放入并勾选“不再提醒”才保存全局偏好；可在 Mod 设置中关闭“手动指定目标时允许强制放入”恢复提醒。C 自动收纳永远遵守规则。
- 强制只绕过 Mod 规则，仍检查游戏容量、锁定、固定物品和类型限制。不改原生拖放或原生兼容模式的转移流程。未配置容器的手动操作保持原样。
- 0.3.0 已保存标签规则自动兼容，默认仍为物品＋标签模式，白／黑名单为空。

### 0.3.0 顾客需求与标签收纳

- **D** 开关当前顾客的需求高亮。首次默认开启，状态跨重启保存；下一位顾客自动更新。青色内框表示物品匹配需求，右上角标表示容器内有匹配物品，两者可以同时出现。不按预算过滤；订单高亮表示该物品可贡献，不表示数量已经凑齐。未知特殊订单明确提示无法识别。
- **C** 立即收纳选区；持续按住 C，再逐个点击物品可连续收纳，点击不触发拖动。D/C 都可在 Mod 设置中改键；冲突时不会执行。
- 点击自有容器标题栏的**设置图标**，编辑当前容器／当前页的名称、标签、满足任意／全部及优先级。名称是 Mod 显示别名，完整保存；标题空间不足时只影响显示，不截断保存内容。
- 标签可搜索游戏分类、公开品质／条件和受支持的离散状态；本次不提供耐久、纯度等数值范围条件。标签不读取隐藏真假属性。已有规则随当前存档保存。
- **Open** 范围只使用打开且当前可见的页；默认 **All** 使用玩家库存中全部合规储藏页及嵌套容器，包括关闭页，不切换当前页面。待售区、顾客物品、垃圾和机器输入口不作为目标。
- 储藏升级后**每页独立名称和规则**；原第 1 页的规则继续保留。优先级越大越先尝试，相同优先级按容器身份和页号稳定排序；满后尝试下一目标，允许拆分堆叠，不重排目标内原有物品。
- 固定物品及已设置收纳标签的容器跳过；普通未贴标签容器按自身属性整只转移。未匹配／放不下的物品留在原处并提示数量。
- **空标签仅表示不参与 C 自动收纳，手动放入与原有 Shift/Ctrl 转移不受影响。**

这些新增功能已通过编译与逻辑检查；标题栏布局、跨页转移、保存／读档仍需在游戏内验证。

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

0.2.1: while this mod is actually enabled, the native control-hint overlay's shortcut region shows a dynamic list of currently effective bindings (refreshed approximately every 0.15 seconds). Disabled, unbound and conflicting shortcuts are labeled. TakeOver shows our actions; VanillaWithHelpers shows native transfer keys. Applied binding changes require no restart. Disabling the mod, including disabling after an input error, restores the original region. Other shop hints are unchanged. The new layout still needs in-game verification.

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

0.2.1 compiled against installed full-game DLLs with zero warnings/errors; 110 logic checks and assembly-binding checks passed. Actual panel layout, language changes, persistence across restart, pending changes during dragging, mode switching, marker alignment and combinations with other mods still require in-game verification. 编译与逻辑测试不等于游戏内验证通过。


## 0.3.0 — Customer demand and tagged storage

- **D** toggles customer demand markers, enabled initially and remembered globally. An inset border marks matching items; a top-right badge marks containers holding matches. Budgets are ignored. An order marker means the item contributes, not that the total quantity is fulfilled. Unknown special requests display an unsupported notice.
- **C** stores the selected items; hold C and click individual items to store repeatedly without dragging. Both keys can be changed in Mod settings and are checked for conflicts.
- Use the **settings icon in a container header** to name and tag the current container/page, select any/all matching, and set priority. Names are complete Mod aliases; limited title space may shorten their display only. Rules are saved separately per game save.
- Every upgraded storage page has independent rules. **Open** targets only visible current pages; **All** includes closed pages and eligible nested storage in the player's inventory, without switching tabs. Sale areas, client property, trash and machine intakes are excluded.
- Higher priority is tried first; ties use stable owner identity and page order. Stacks may split across destinations. Pinned items and tagged storage containers are skipped. Unmatched/full items remain at the source. Empty tags affect only automatic storage, never manual insertion or existing transfer shortcuts.
- Searchable tags cover game categories, public conditions and supported discrete states. Numeric rules such as durability/purity are deferred. Hidden truth is not revealed by tag matching.
- Runtime layout, cross-page transfers and save/reload still require in-game validation.


## 0.4.0 — Specific-item rules

Choose Items + Tags or Whitelist Only per container/page. Pick allowed/denied items repeatedly, then right-click to return to the draft; Save commits rules. Identification uses item kind, not the individual instance. Changing tabs does not redirect the draft to another page.

Automatic storage excludes blacklisted destinations first, then tries explicit allowlists, then tag matches, then optional unconfigured destinations. Full destinations fall through. The final fallback is off by default; each tier retains numeric priority and the Open/All scope. Whitelist-only and deny-only configurations are never unconfigured fallbacks.

Independent allow/deny preview strips show translucent item images with white/black borders. These are UI images, not inventory items. Scroll horizontally, hover for names, click to remove immediately. The arrow expands tag rules. Configuration warnings cover definite empty/contradictory rules only.

This mod's explicit-target shortcut asks once per batch when destination rules reject items. Choose matching-only, override-all or cancel. Remembering overrides is global and reversible in Mod settings. It never affects automatic storage or bypasses native game restrictions. Native drag/drop remains unchanged.


0.4.1：保留原生快捷键提示，不再隐藏或替换原提示区域。在本 Mod 设置底部点击「Better Hotkeys 快捷键说明」查看独立说明（应用改键后重新打开刷新）。程序集及安装目录统一为 BetterHotkeys；内部 ID 保持 probably_stolen_organizer，已有设置与规则沿用。升级前退出游戏，移走旧 Mods/StarterMod 目录，避免重复安装。

0.4.1 preserves native shortcut hints. Open the independent shortcut guide from this mod’s settings; apply binding changes and reopen to refresh. Assembly/package name: BetterHotkeys; the internal mod ID is unchanged. Remove the old StarterMod installation before installing the renamed package.


0.4.2：收纳规则分为基本设置、物品名单、标签规则三个页签。点选入口旁标明右键完成；Esc 不再完成点选。缩小标题栏齿轮，打开规则面板时清除原生悬浮提示。保留原有标签名称与规则格式。


0.4.3：规则预览默认折叠为侧边展开按钮，展开内容放在窗口侧边，不挪动或覆盖本窗口库存格。左右空间不足时保持折叠并提示移动窗口。标签规则页可左键点选面板外物品，筛选该物品可见且可匹配的标签；逐项勾选或点击“采集这些标签”加入草稿。被面板遮住的物品可用“点选物品”临时隐藏面板，右键返回。采集为追加，不清除已有标签；点击保存才生效。


0.4.4：默认使用 All（全部合规分页，包括关闭窗口）。升级首次使用新的范围设置键并默认 All，此后手动选择 Open 会正常保留。修复右键结束点选后物品提示残留：清除原生提示缓存和画面，再等待按键释放后恢复面板。


0.4.5：打开规则面板自动收起预览区。标签支持允许／排除名单；允许标签沿用任意／全部匹配，命中任一排除标签始终拒绝（包括允许物品命中与仅物品模式）。采集下拉框选择加入允许或排除标签；保存后生效。仅排除而无允许条件的规则不会接收物品，也不作为空规则兜底。手动指定目标仍沿用既有冲突确认机制。新版本兼容旧规则；保存含新格式的规则后降回旧版，需要恢复升级前存档，旧二进制不能解析新格式。


0.4.6：手动强制收纳确认框打开前清除物品详情提示及其缓存，避免提示遮住确认按钮；确认与转移规则不变。
