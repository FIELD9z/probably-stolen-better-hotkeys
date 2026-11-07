# Better Hotkeys & Storage / 更好的快捷键与收纳 — 0.5.5

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
| Shift＋左键 | 库存/自有容器/设备成品区→出售柜台；出售柜台→库存 |
| Ctrl＋左键 | 库存/其他容器/设备成品区→选定自有容器；从目标容器取出→库存；无目标时容器/成品→库存；柜台不执行 |
| Alt＋左键 | 固定或解除固定；点击选区内物品时操作同来源选区 |
| 悬停标题栏＋游戏整理键（默认 S） | 整理该容器，默认记住最近一次原生菜单排序方式 |
| 双击 V | 开关赃物/违禁品标记；默认黄/红/紫分别表示赃物/违禁品/两者 |

固定物品不会被转移。目标不足时允许部分接收，不自动绕路或整理目标。普通拖动、滚轮数量调整及 Shift 拖动半堆保留；不新增单取键。

### 设置

- 常规：语言、输入模式，转移、固定、标题栏整理、标记分别启停。
- 按键：五个动作支持文本组合键，如 Shift、RightAlt、Ctrl+Q、B；None 或空白解除绑定。可用 A–Z、F1–F12（F8/F9 保留）、Space、Tab、BackQuote、Home、End、PageUp、PageDown、Insert 以及 Shift/Ctrl/Alt 的左右变体；最多一个普通键。T 被本机游戏硬编码占用，运行时提示冲突并不执行。左右等价默认开启；关闭后 Shift/Ctrl/Alt 表示左侧。
- 标记触发：Press 单按开关、DoublePress 双击开关、Hold 按住显示；双击间隔 0.15–0.8 秒。
- 行为：同来源批量操作；仅标题栏或容器任意区域指定目标；目标边框；记住最近排序或固定使用大/小件优先、起始/末端。
- 标记：两类分别启停、嵌套汇总、仅自有物品、三种分类配色（七色预设）、粗细档位 1–4（实际线宽 0.5–2 UI 像素，默认 1 像素）、透明度 0.2–1、扫描间隔 0.1–0.5 秒。扫描不会执行盘查或改变热度。
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
| Shift + left click | Inventory/owned container/machine output → sales counter; sales counter → inventory |
| Ctrl + left click | Inventory/other container/output → selected owned container; from selected target → inventory; no target: container/output → inventory; no route from counter |
| Alt + left click | Pin/unpin; selected items in the same direct inventory are processed together |
| Hover header + game sort key (default S) | Sort that inventory; remembers the last vanilla menu sort choice by default |
| Double V | Toggle stolen/contraband markers: yellow/red/purple for stolen/contraband/both |

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

## 0.5.2 — 容器互转与设备成品 / Container transfers and machine outputs

普通左键点击已打开的自有容器选定目标（可设置为仅标题栏），快捷操作不会改选目标。关闭目标窗口或切换其分页后，必须重新指定目标。目标容器自身的 Ctrl 点击仍返回库存；没有目标时库存物品提示先选目标，容器与成品区物品返回库存。手动目标规则冲突仍提示确认。

设备仅支持可见、未锁窗口中原生标为 output 的独立成品区，包括单物品槽。已核对熔炉、打印机、瓶子打印机、化学处理设备、解序设备、水培设备的正式版输出绑定。Shift 送出售柜台；Ctrl 按上述目标规则；C 按收纳规则分配。原料、燃料、加工位及没有明确 output 区域的设备不扩展，设备不作为收纳目标。固定、原生取出/容量/插入限制继续有效。

A plain left click selects an open owned container (header-only remains optional). Shortcut actions preserve that target. Closing its window or changing its page clears it. Ctrl from the target returns to inventory; without a target, container/output items return to inventory while inventory items prompt for a target. Manual rule-conflict confirmation is unchanged.

Machine sources must be native output panels in visible, unlocked owned windows; both grid and single-slot outputs are supported. Shift sends to the sales counter, Ctrl follows the target rules, and C stores by rules. Inputs, fuel, processing slots and machines without explicit output panels are excluded; machines are not storage destinations. Native removal/placement restrictions and pinning still apply. Build and automated checks do not replace in-game verification.

## 0.5.3 — 分类目录与拼音搜索 / Categorized catalogs and pinyin search

- 物品名单和标签规则上方始终独立显示已选允许／排除项，支持逐项移除、分别清空；搜索、折叠或点选物品筛选不会隐藏这些已选规则。修改仍需点击保存，取消不改变规则。
- 物品目录记录本存档中曾在打开窗口内显示的自有物品；关闭容器的内容不提前收录，卖掉后目录仍保留。旧点选记录及现有名单继续可用，缺少类别的放入“其他 / 类别未记录”。不创建游戏物品来生成目录。
- 物品按游戏原生主类折叠分组，跨类物品可出现在多个组；搜索跨全部分类并去重。标签分为主要类别、其他类型／属性、属性／品质、状态；旧的未知项保留并可移除。
- 搜索支持显示名称的中英文文字、拼音首字母、全拼、大小写和中英数字混合，例如“手枪”可搜 sq、shouqiang。ü 可输入 v、ü 或 u:。多音字接受读音表中的多种读音，因此可能出现额外匹配；这是检索辅助，不改变物品名称。
- 标签点选仍可查看和采集物品标签；开始输入搜索会退出点选筛选，搜索全部标签。“显示全部”清除筛选。允许／排除冲突依然由排除优先。
- 目录、名称和规则随当前存档保存，读档不混用其他存档目录；拼音字典内置，运行时不联网。名称采用最近一次实际观察时的游戏显示名称。

Selected allow/exclude lists remain visible independently of filters, with per-entry removal and per-group clearing. Changes still require Save; Cancel discards rule edits. The item catalog remembers owned items displayed in open windows in the current save, including items later sold. Closed-container contents are not discovered automatically; old picked entries and existing rules remain available.

Browse collapsible native major categories (an item may belong to several) or search globally with deduplicated results. Tags are grouped by major category, other types/attributes, properties/qualities, and states. Search supports displayed Chinese/English names, pinyin initials/full syllables, mixed Latin/digits, and alternative readings. Use v, ü, or u: for ü. Search exits sampled-tag filtering; Show all clears it. Search data is embedded and offline. Remembered names reflect the most recent observed game display name.

### 本次游戏内复测 / In-game checks for this update

1. 打开 A/B，普通点击 B 后 Ctrl 点击 A：物品入 B；再 Ctrl 点击 B 内物品：返回库存。确认快捷点击不改变目标边框。
2. 关闭 B 或切换其储藏页，确认目标失效；无目标时 A 中物品回库存，库存中的物品提示先选目标。
3. 在有独立 output 区域的设备产出物品，分别测试 Shift、Ctrl、C；原料／燃料／加工位应不被转移。验证固定物品、满目标与不允许取出物品仍受限制。
4. 搜索 sq / shouqiang，展开分类、点选采集标签后，仍能在上方删除已选标签；清空一组不影响另一组。分别测试保存与取消、重新开窗与读档。
5. 只打开外层箱子，确认内部未展开内容不会进入目录；打开内层后确认收录。卖掉物品后仍能搜索；换存档不应混入旧目录。
6. 检查长名称、省略号悬停提示、允许／排除两个滚动区及不同 UI 缩放下的窗口布局。

Compilation, logic tests and native API binding checks passed; these checks do not exercise Unity layout, actual item callbacks or in-game save/reload.


## 0.5.4 — 更细的占格轮廓 / Thinner cell outlines

顾客需求框固定为 1 UI 像素；赃物／违禁品框沿用原粗细设置，实际线宽改为每档 0.5 UI 像素（默认第 2 档为 1 像素）。已有设置无需重置。风险框距占格外沿内缩 0.5 像素，需求框紧邻其内侧并留 0.5 像素间隙；两类框都靠近外沿，保留独立颜色。异形物品沿旋转／翻转后的实际占格轮廓绘制，不再用外接矩形穿过空格。线条留在自身占格内，避免正常库存中相邻物品的轮廓相交。容器需求角标从 8×8 缩到 5×5，并放在实际占格内。

复测：同一个物品同时命中需求和违禁／赃物时，两种颜色应均可见；将矩形和异形物品紧邻摆放，检查边框没有交叉；旋转、翻转、拖动、缩放界面后检查轮廓跟随；切换 D/V 和粗细设置后检查显示更新。窗口互相遮挡时沿用原生窗口层级，这里保证的是同一正常库存内不重叠占格的物品边框。

Demand outlines use 1 UI pixel. Risk outlines use 0.5 UI pixels per existing width level (default level 2 = 1 pixel). Both follow occupied cells after rotation/reflection, stay inside their own cells and retain separate color lanes. Nested-demand badges shrink from 8×8 to 5×5 and sit in an occupied cell. Check adjacent irregular items, both colors on one item, rotation, dragging, UI scaling and D/V toggles in game. Compilation and automated geometry checks do not verify the final Unity rendering.


## 0.5.5 — 对角角框 / Opposite-corner markers

替代 0.5.4 的完整轮廓：违禁品／赃物仅显示右上与左下两个 L 形角框；顾客需求仅显示左上与右下两个 L 形角框。两类标记共用距占格外沿 0.5 UI 像素的位置，不再让需求框向内错开。颜色、风险粗细档位与 D/V 开关沿用。L 形短边不超过 8 UI 像素或一个格子边长的四分之一，小物品也不会连成整框。

异形物品使用最上／最下有占格行中对应的左右端点，角框始终位于实际占格内；旋转／翻转后重新选择屏幕方向的角落。容器的需求角标改到左上占格内侧，与右上风险角框错开。

Contraband/stolen markers now use only the top-right and bottom-left L corners. Demand markers use top-left and bottom-right, sharing the same outer inset. Arms are capped at 8 UI pixels and one quarter of a cell. Irregular shapes use occupied corners in their extreme rows; nested-demand badges move inside the top-left occupied cell. Existing colors, thickness levels and D/V controls are preserved.

实机重点复测：同一物品同时命中两类标记、单格物品、异形物品紧邻摆放、旋转／翻转，以及容器内有需求物品时的角标。自动检查不代替最终游戏画面验证。
