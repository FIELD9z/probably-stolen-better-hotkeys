# 正式版快捷键与整理操作调查

调查日期：2026-10-29。目标：为物品整理、明确目标容器和批量操作 Mod 提供依据。

## 证据与边界

- 本机正式版 Steam Build ID：26263409。
- 直接读取本机 `Probably Stolen_Data/Managed/Assembly-CSharp.dll` 的元数据和 IL；使用游戏附带的 Mono.Cecil，不加载或执行游戏逻辑。
- DLL SHA256：`DD313F73681D5146581F5DC6F981AEC22E925C4602CD93C1E99FDEE2D38F9152`。
- 已读项目 README、research.md，并核对[开发指南](https://github.com/dragonpan2/probably-stolen-modding/blob/main/ModdingGuide.md)。下面的行为结论以本机 IL 为依据，不以在线文档中的开发中能力为依据。
- 没有启动、部署或修改游戏，没有读取个人按键设置或存档。下表是代码默认值，可能不同于用户当前设置。
- 本次是静态调查，不是游戏内复现。场景序列化的处理器优先级、鼠标按钮字段和实际界面遮挡尚未核实；不能仅凭方法存在宣称 Mod 已兼容。
- 本机分析中间文件放在被 Git 忽略的 `.local/`，未复制游戏 DLL 入项目。

## 默认可改键

来源：`KeyBindHelper..cctor`，共 22 项（13 项动作与 9 个工具栏槽位）。

| 默认键 | 动作 ID | 当前用途 |
| --- | --- | --- |
| E / Q | rotate_cw / rotate_ccw | 拖动物品时顺／逆时针旋转 |
| F | flip_item | 翻转拖动物品 |
| R | reset_orientation | 重置拖动物品方向与翻转 |
| Delete | trash_item | 尝试丢弃鼠标悬停物品 |
| S | sort_menu | 打开整理菜单，不是立即整理 |
| 左 Ctrl | quick_move_inventory | 库存快速转移，实际目标随上下文变化 |
| 左 Shift | quick_move_showcase | 展示柜快速转移，存在特殊分支 |
| 左 Alt | quick_move_storage | 后仓快速转移，存在特殊分支 |
| P | pin_mode | 切换固定物品模式 |
| K | open_options | 游戏选项 |
| O | control_hints | 操作提示 |
| Space | advance_dialogue | 推进对话 |
| 1–9 | toolbox_slot_1 … toolbox_slot_9 | 工具栏槽位 |

`KeyBindHelper` 每个动作只存一个 `KeyCode`，不是“修饰键 + 主键”的组合结构。按键以 `keybind_<id>` 通过 ES3 保存。左右 Ctrl/Shift/Alt 是不同键值，默认快速转移绑定左侧。

设置 UI 的 `PollKeyCapture` / `FindPendingKeyOwner` 会拒绝与其他待应用绑定重复的键；并非没有冲突检测。但检测不覆盖写死在其他操作中的修饰键，也不区分使用场景。捕获只接受键值小于 Mouse0 的非 None 键，不能通过该捕获流程绑定鼠标按钮；Esc 用于取消。`KeyBindHelper.Set` 本身只赋值，不能把 UI 层校验当作所有调用路径都有的保证。

## 整理入口：为什么 S 会显得没有反应

`ItemContextHandler.OnEventPress` 的 S 分支先检查鼠标下是否命中 `GameItem`：命中就返回。只有未命中物品时才调用 `TryOpenSortMenu`，后者要求命中可整理的 `GameGridInventory`。

这意味着：

1. S 是打开菜单，还需要选择菜单操作。
2. 鼠标指着物品不能直接打开它所在容器的整理菜单。
3. 这会让几乎占满的容器更难找到触发位置；这是代码推导的体验问题，尚未游戏内复现。
4. 不能把“没反应”全归因于按键：插入锁定、玩家权限或物品不可完整移动，也可能让整理不可用。

原生已有公开方法：

- `InventorySortHelper.MaySort(GameGridInventory)`
- `InventorySortHelper.CanSort(GameGridInventory)`
- `InventorySortHelper.Sort(GameGridInventory inventory, bool bigFirst, bool fromEnd)`
- `InventorySortHelper.CanUndo(GameGridInventory)`
- `InventorySortHelper.Undo(GameGridInventory)`

`Sort` 读取原生自动旋转选项，先尝试 `PackShelves`，失败后尝试 `PackFirstFit`；两者失败则返回 false，成功才保存快照并提交。快照按容器保存，下一次整理覆盖该容器的旧快照；成功撤销后移除快照。这不是通用的物品转移撤销系统。

## 快速转移：按键名称不等于固定目标

来源：`ItemQuickTransferHandler`、`ItemQuickTransferAltHandler`、`ItemQuickTransferAltHandler1` 的 `OnEventPress` / `OnEventRelease`。

- 按下转移键时设置 `isQuickTransfer`；鼠标按下时记录物品，鼠标释放时重新命中，要求仍是同一物品并满足 `MayRemove` 等条件，再执行转移。
- 因此涉及按下／释放顺序和处理器状态，不只是“当前按着修饰键就移动”。按键顺序导致的具体差异需要游戏内测试。
- 转移请求通常使用 `GraphUtils.TryAcceptOnce(..., -1)`；本轮未完整追踪底层部分接收规则，不能据此保证整堆一定转完。
- 三个处理器均有未购买物品的特殊分支：尝试同一窗口内其他可接收的库存，不是无条件送往背包或仓库。

Ctrl 对应的库存转移中，已确认这些分支：

| 条件 | 代码目标 |
| --- | --- |
| 满足活动容器插入条件 | 在可见窗口列表中 `FindLast` 一个非来源且有有效槽位的窗口，走活动容器插入 |
| 来源为 invWindow，且未进入活动容器分支 | backInvWindow |
| 来源为 showcaseWindow | invWindow |
| 来源为前／后库存窗口 | 依物品所有权、是否外出，在 invWindow、前／后库存、afterhourWindow 间选择 |
| 其他来源 | 依是否外出，通常进入 invWindow 或 afterhourWindow |

Shift / Alt 也分别包含来源窗口、外出状态、所有权及活动容器分支。**快速转移不是三个始终指向同一目标的按钮。** 可见窗口列表的最后一个合适候选如何对应用户眼中的前后层级，仍需实测。

这为“明确目标容器”提供了直接改进方向：用户指定目标后，显示目标并执行校验；容量不足时说明未转移的原因，不再静默改投其他容器。此处是候选设计，尚未实施。

## 拆分、拖动和多选

`ItemMouseDragHandler.StartDrag`：

- 默认取 `MaxNumRemove`（调试菜单物品另有分支）。
- 写死左 Alt：取 1 个。
- 写死左 Shift：取可移数量的一半，向上取整。
- 两者同时按下时，Shift 分支在后，覆盖 Alt 的数量结果。

这些修饰键不经过 `KeyBindHelper`。因此改掉快速转移的 Shift/Alt 绑定，不会同步修改拆分堆叠。不能把“代码共用按键”直接认定为每次都同时触发；最终由输入处理器状态和优先级决定。

拖动还支持滚轮数量调整相关字段，以及右键／中键的旋转或翻转分支，受 `swapDragRotateFlip` 影响。滚轮倍率键与部分鼠标触发键是字段，未核实场景实际值，不在本报告猜测默认键。

`ItemMultiSelectHandler` 已有框选、选择保持、批量拖动和精确放置状态；不是完全缺少批量能力。其默认构造键是 Mouse0，框选启动要求从非物品区域开始。`IsPreciseModifierHeld` 接受左右 Shift；已有选区点击分支又直接检查左右 Ctrl/Shift/Alt，未统一走可改键。三种快速转移处理器则各自读取单个命中物品，没有直接遍历多选列表。

## 输入优先级与界面保护

有两条并行入口：

1. `StoreUIManager.Update` 等组件直接轮询按键，处理提示、删除、工具栏、机器、漏斗、固定模式、Esc、对话等。
2. `InputActionManager.Update` 按 `priority` 降序处理各个输入处理器，依次发送释放、按下、鼠标移动、滚轮和更新事件；处理器更新返回 true 后停止后续处理。`ResetAfter` 等方法重置其他处理器状态。

`InputActionManager.UpdateKeys` 会在输入文字或捕获改键时抑制键盘按下事件，但仍处理释放及独立的鼠标路径。`KeyBindHelper.SuppressHotkeys` 本身只表示改键捕获及捕获结束当帧，不是完整的模态窗口屏蔽器。文字输入保护存在，但分散在不同调用处；不能声称完全没有，也不能假定所有路径都统一遵守。

Esc 已有消费帧标记和明确顺序：部分模式先行消费，随后 `ResolveEscape` 处理选项、结算、Esc 菜单、网络 UI、覆盖层、选中工具、多选、顶层可关闭目标、单物品拖动，最后打开 Esc 菜单。值得实测的是“关闭窗口”排在“取消单物品拖动”之前的体感，不宜直接重写所有 Esc 行为。

## Mod 可行性与建议范围

用户已选择优先改善：**物品整理与转移：一键整理、明确目标容器、批量操作**。

建议第一版围绕以下三件事设计，具体按键仍待确定：

1. **指向物品也能整理所在容器**：从命中物品解析父容器，直接调用原生整理；保留菜单入口，并提供独立的撤销整理动作。
2. **目标可见的快速转移**：选择目标容器后显示名称／高亮，明确来源、目标和数量；复用原生放置与权限检查，返回失败原因。
3. **对选中物品批量执行转移**：复用现有选区，定义固定物品跳过、容量不足及部分成功规则。批量移动的撤销需要另做设计，不能借用原生整理撤销冒充。

本机 `ModHook` 事件列表未发现输入分发、快速转移前置取消或整理菜单触发的专用钩子。生命周期可优先使用 `OnGameLoaded*`，但要替换原按键路径，可能需要小范围 Harmony 补丁；仅新增独立快捷动作则需进一步验证公开类型与输入接入方式。公开方法已在元数据确认，本轮没有编写或编译上述功能的探针，不能宣称实现已验证。

第一版不必立即重写排布算法。先修复容器识别、目标选择和批量入口，能把输入问题与排布效果问题分开评估；这是建议，不是已决定的功能规格。

## 后续验证清单

- 游戏内确认当前按键设置、各处理器实际优先级和鼠标字段。
- 对比鼠标指向物品／空格／满容器时的 S 行为。
- 测试修饰键先按／后按、先松／后松及改键后的拆分行为。
- 同时打开多个容器，核对活动容器选择和不同来源下的 Ctrl/Shift/Alt 去向。
- 检查商店内／外出、未购买／自有、固定物品、锁定容器、容量不足。
- 确认已有多选是否跨容器、部分移动后的选区状态，以及文字输入／菜单／拖动期间的抑制。
- 编译验证拟用 API 后，再由用户授权部署和游戏内验证；编译成功不等于游戏内通过。
