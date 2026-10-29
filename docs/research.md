# Mod 开发调查

## 2026-10-29 Better Hotkeys 0.2.1：动态操作提示

- 用户反馈原生快捷键提示与实际操作冲突，要求仅实际启用时修改，并跟随自定义绑定。
- 本机 DLL 的 StoreUIManager.controlHintOverlay 是原生面板；ToggleControlHintOverlay 只切换显隐。只读检查 level1 场景确认 Shortcut 区域的键帽固定为 CTRL/SHIFT/ALT，旁边八条说明是独立本地化文本，原生没有读取本 Mod 配置的路径。原生键帽宽度约 22–32，无法可靠显示长组合键。
- 在原区域（700×352）建立动态双列列表，隐藏并记录原子节点的原始显隐状态，不改本地化表、按键绑定或游戏资源。只在实际 Active 且面板可见时创建/刷新；OnDisable/Fail 恢复原始节点。使用 StoreUIManager.Update 后置补丁，因为没有对应逐帧 ModHook；错误时恢复原生并记录日志。
- 直接读取 Settings.Current，配置待手势结束前不提前显示新键；接管/原生兼容模式、禁用、解绑、左右键等价、三种标记触发均有相应提示。排序跟随游戏时读取游戏实际键，旋转/翻转/对话同样读取原生绑定。
- 新增 Unity.TextMeshPro 本机引用（Private=false），使用游戏自带中/英字体。Preferences 单独成文件供输入和可测提示模型共享，不改变配置 schema。
- 正式版编译零警告/错误、110 项逻辑检查及程序集绑定检查通过。真实面板布局、长组合键、中英文、启停恢复、原生语言切换与其他 Mod 组合仍需游戏内验证。

## 2026-10-29 Better Hotkeys 0.2.0

- 用户授权实现 ModConfig、中英双语，更名 Better Hotkeys / 更好的快捷键，署名 FIELD9z；内部 ID 与原有两个排序键保持不变（已对照旧版 DLL）。
- 直接使用本机 ModConfig.Define、OnModConfigApplied，组合键用文本字段。应用时校验语法、保留键与本 Mod 重叠绑定；无效时恢复旧配置。原生 Apply 先 Save 再触发事件；ConfigEntry.Set 会自动 Save，恢复和排序记忆均可保存。
- 空闲输入帧或重新启用时应用配置快照。两种模式为接管、原生＋辅助。当前所有公开绑定及可用储藏预设在触发时检测；本机 SmoothMouseMovement.Update 硬编码 T，作为明确冲突阻止助手执行。其他 Mod 独立轮询无法完整检测。
- 标记/整理错误分别停用对应运行时模块并写日志；输入主流程错误仍停用本 Mod 行为。颜色为七种预设；英文和中文提示/说明齐全，主清单用双语名称。
- 编译 0 警告/0 错误、90 项逻辑检查、实际程序集绑定检查通过。配置窗口、应用/重启持久化、拖动中的配置变更、模式切换及实际 Mod 组合均待游戏内验证；未上传 Workshop。

## 2026-10-29 安装验证修正

### 0.1.5 标记朝向修正

- 用户截图显示部分横放物品仍使用竖向框、框线越过容器边界。旧边框 stretch 填满 middle 节点；本机 IL 显示该节点位置由 modifiedShape.minX/minY 更新，但尺寸沿用原始渲染尺寸，不能直接作为旋转后外框。
- 边框锚定 middle 左上角，尺寸明确采用 modifiedShape.globalWidth/globalHeight × UISettings.gridSize。GridShapeBuilder 的两个 global 尺寸 getter 已核查：奇数朝向交换宽高；翻转不改变外包围框尺寸。已有边框每帧更新几何，避免沿用上一次扫描时的方向。
- 本机正式版编译和绑定检查通过；实际旋转、翻转后的画面对齐仍需用户游戏内复测。

### 0.1.4 双击 B 标记

- 用户确认仅标记外层容器，打开后逐层标记内容，最终按键选择双击 B；正常完整转移已在 0.1.3 改为静默。
- 新增只读图扫描：读取 STOLEN_TAG 与原生 IsContraband（IL 确认仅检查 CONTRABAND_ITEM_TAG）。不调用实际盘查或修改热度接口。遍历可见店内窗口及其所有嵌套内容/分页，以反向传播合并标记；处理共享节点和循环引用。
- UI 边框使用原生物品框相同的 middle 渲染节点，但节点独立且不拦截鼠标，不覆盖原生多选/固定数据。每 0.2 秒刷新，关窗/物品销毁后移除，离店/读档/停用时清空。
- 双击阈值 0.35 秒；输入阻挡、编辑器、拖动、修饰键会取消待完成双击。检查当前按键绑定和可用储藏协调器预设，B 冲突时不触发。
- 编译 0 警告、0 错误；66 项逻辑测试与程序集绑定检查通过；独立只读审查未发现阻断问题。图形位置、实际开关及容器展开效果待游戏内验证。

### 0.1.1 游戏反馈修复

**后续纠正：0.1.1 的柜台映射仍然错误，见下方 0.1.2；不能继续把 frontInvinvElement 当作出售柜台。**

- 用户实测：Shift 错转展示柜、提示只有“整理助手”、升级分页储藏区不能选择。
- 柜台分类和转移目标改为 `frontInvinvElement`；本机 `PlayerStore.TryAddDirectSellingItemToTable` 使用该网格。原实现误用了独立的 `showcaseElement`。
- `StoreUIManager.Notify` 参数实际为 message/color，原实现误当标题/正文。现传入完整消息与原生 `default` 配色。
- 升级储藏区使用 `MachineryPanelRow.GetPanel("storage").content` 的 `MachineryTabs`，仅解析 `GetPage(selected)` 中的 `MachineryIntake.content`。切换分页使旧目标失效，点击当前页内部可重新指定目标。
- 目标边框和标题栏整理命中宽度按窗口宽度计算，修复只覆盖左上角小块的问题。
- 本机正式版编译通过（0 警告、0 错误），45 项现有规则/转移模拟测试通过。模拟测试不覆盖真实 UI 或程序集字段的玩法含义；上述修复仍需游戏内复测。

### 0.1.2 柜台映射与原生回退纠正

- 用户截图确认 `frontInvinvElement` 是外面交易台。直接检查 `PlayerStore.SellItemFromTable` 与 `SellAllItemFromTable`，两者读取 `backInvinvElement`；现在来源分类和目的地统一使用后者。之前只看 `TryAddDirectSellingItemToTable` 名称推断玩家出售柜台，证据不足。
- 原生快速转移三个处理器的 Press/Release/Update 在 Mod 启用时全部拦截并清空状态，不依赖当前命中位置或来源识别。删除 PrimeVanillaQuick 回退路径；停用 Mod 后还原原生入口。
- 新增 `scripts/Test-GameBindings.ps1`：对照本机实际出售方法检查编译产物的柜台引用，核对三个处理器补丁实例类型/入口及启用保护和清理调用；该检查、45 项逻辑测试及编译均通过。仍不等同游戏内验证通过。

- 用户已在游戏 Mod 列表看到 Probably Stolen Organizer，但启用提示清单缺少反射权限。实现使用反射访问内部成员，清单已补充 `<Capability>reflection-external</Capability>`；名称与 XML 结构通过本机 DLL 核对。
- 主菜单显示 `VERSION 100-Rev3`，但 ModUIManager.VersionMatchesGame 使用 Unity Application.version。当前 Player.log 的 ModLoader audit 明确记录 `game 0.46D`，因此本机安装包应传入 `-GameVersion '0.46D'`，不能使用主菜单显示文字。
- 上述修正尚待替换游戏目录清单后验证；不能视为启用或玩法验证通过。

调查日期：2026-10-28（Asia/Hong_Kong）。

## 结论与资料可信度

- 正式版 Steam App ID 为 **4348910**；Demo 为 **4349200**。正式版于 2026-10-28 发布。[Steam 商店](https://store.steampowered.com/app/4348910/Probably_Stolen/)
- 开发者公告提到 Workshop、Mod API 及 Mod 本地化，同时提醒 Demo Mod 可能不兼容正式版。[开发者公告](https://steamcommunity.com/app/4348910/announcements/detail/1839676055895202)
- 找到 [dragonpan2/probably-stolen-modding](https://github.com/dragonpan2/probably-stolen-modding)。其指南声明由 AI 辅助撰写、开发者审核；这是目前采用的开发参考，不将仓库所有内容视为已在本机版本验证。
- [APIChanges](https://github.com/dragonpan2/probably-stolen-modding/blob/main/APIChanges.md) 顶部仍标记 “Next update (in development)”。因此新能力、扫描命令和开发者模式必须按实际游戏版本验证。

## 本机只读检查

| 项目 | 结果 |
| --- | --- |
| 正式版目录 | `D:\SteamLibrary\steamapps\common\Probably Stolen` |
| Steam Build ID | `26263409`（来自正式版 appmanifest） |
| Steam 安装状态 | StateFlags = 4 |
| .NET SDK | 9.0.316 |
| Unity | 2021.3.45f2（本地 Player.log） |
| 后端 | MonoBleedingEdge 与 Managed 目录均存在 |
| 可用程序集 | Assembly-CSharp.dll、UnityEngine.CoreModule.dll、0Harmony.dll |
| 其他版本 | 同一库中另有 Demo 和 Playtest，不能混用引用 |

这里只读取安装信息与日志，不在仓库复制游戏 DLL、存档或 Steam 账号数据。

## 实现约定

依据[入门指南](https://github.com/dragonpan2/probably-stolen-modding/blob/main/README.md)：采用 C# / netstandard2.1，入口实现 `IMod`，使用 `ModLog` 输出日志。引用本机游戏程序集且 `Private=false`，避免将游戏 DLL 放入发布包。manifest 列出自身程序集并保留空 Capabilities；实际加载行为待游戏内验证。

依据[进阶指南](https://github.com/dragonpan2/probably-stolen-modding/blob/main/ModdingGuide.md)：优先考虑 ModHook；确需修改原方法时再考虑 Harmony。初始化阶段不要假定存档和玩法管理器已建立；注册事件后应在停用时解除。资料还包含物品、资源、本地化、自定义 UI、存档数据和 Workshop 发布说明，按首个功能需求再选择，不预先引入全部依赖。

## 验证边界与下一步

- 已通过本机正式版程序集编译最小入口：`./scripts/Build.ps1` 成功，0 警告、0 错误，确认 IMod / ModManifest / ModLog 基础签名可用。
- 未启动游戏测试加载；未发布 Workshop。
- manifest 兼容版本仍需确认游戏内字符串，不能直接填 Steam Build ID 或照抄文档旧版示例。
- 发布前运行游戏内加载、启用后重启、日志检查及实际功能测试；涉及数据时补充保存/读取验证。
- 未来游戏更新后记录新的 Build ID，重新编译并回归验证。

## 2026-10-29 Better Hotkeys 0.3.0：需求与分页标签收纳

新增顾客需求高亮、容器／储藏分页配置、按标签收纳。完整约定与实机验收见 demand-storage-spec.md；分页结构证据见 storage-pages-research.md。

- 以本机正式版 DLL 核对 CustomerDemandOverlay 中普通收购、GunOrderData.ResolveOrderGun/IsAccepted/GetPlacedServiceGun、BarterOffer.UseCondition/IsBarterAcceptable、BarterCondition.IsItemValid 及六个采购／易货闭包。未知特殊回调明确不支持，不搬动柜台试探。
- TypeHelper.ALL_TYPES_LIST 与 ItemConditionList.conditionLookup 提供分类／条件候选。按 NegociationUIManager.AddFeatureSection 的公开显示门槛过滤；动态数值条件本次经用户确认不做。
- ModHelper.ModDataStore 直接读取当前 PlayerStore.ModData。容器 uniqueId+页号存规则，无跨存档缓存。未加载存档时初始化不读取存档。
- PixelWindow.AddHeaderButton 只在 MachineryValidate 布局。普通 Validate 需后置定位图标并给标题留宽；SetTitle 会截断显示，必须记录实际应用值避免摘要反复叠加。完整用户名称只存 ModData。
- 默认 H/J 不在 KeyBindHelper 原生默认表中；运行时继续检查用户改键和协调器预设。
- 主库存为扫描根，受支持容器逐层枚举；不将全拥有物品API的销售／机器等库存误当收纳目标。标签容器保护独立查询已存规则，避免锁定后从目标枚举消失而被移动。
- 已完成正式版编译和逻辑验证；尚未进行游戏内验证或安装。

## 2026-10-29 Better Hotkeys 0.4.0：物品规则优先分流

- 新增每页AllowedItems/DeniedItems/ItemsOnly，按GameItem.identifier识别同种。拒绝优先于允许；分配顺序为白名单、标签、可选空规则兜底，数字优先级仅在各层内部生效。默认兜底关闭。
- 规则codec兼容原4字段标签记录，新7字段记录保留名单顺序。固定或有任何物品/标签规则的容器不会被C自动收纳搬走。
- 点选过程中拦截真实物品press/release，允许非物品页签/标题事件，Esc/右键消费到释放后恢复草稿。原始目标网格固定，目标失效或读档清空。
- 手动明确目标快捷转移一次批量确认。全局记忆仅在“全部强制放入”且勾选后生效；C不读取此覆盖开关。确认后重新验证目标页和来源，原生限制由DirectTransfer继续执行。
- 物品定义目录是Func<GameItem>工厂，不能靠实例化获取纯预览。因此Observe已有真实物品的atlas/path/name并存当前ModData，虚影仅使用Unity Image。缺图明确显示问号，不删规则。
- PixelWindow.Validate根据heightPixels反向调整内容，故预览只调整渲染inner偏移、背景高度、resize位置，不改库存或窗口逻辑高度。Validate前撤销，之后重施；按实际Canvas世界边界纠正可移动窗口，避免底边越界。嵌套不可移动窗口不强行位移。
- 独立审查核对模式、草稿生命周期、原生UI事件、预览Reset恢复。正式版Build 0警告/0错误；139项逻辑检查、程序集绑定检查及git diff --check通过。未部署、启动游戏或上传；窗口缩放/重排/虚影点击仍待实机验证。


## 2026-10-29 Better Hotkeys 0.4.1：独立说明与正式文件名

- 用户要求保留原生快捷键提示。删除 StoreUIManager.Update 提示接管补丁，不再读取 controlHintOverlay、不隐藏或替换其子节点。
- 本机 ModConfigWindow.Open(LoadedMod) 返回 CustomUIWindow；仅在本 Mod 配置页追加“快捷键说明”按钮，打开独立可关闭窗口，显示已生效绑定。应用改键后重新打开刷新，不修改原生提示文字。
- 源码目录、项目、命名空间、程序集与打包目录统一 BetterHotkeys。内部 Mod ID 和配置键不变，保留存档规则。脚本、测试引用与清单同步。
- 编译 0 警告/0 错误，139 项逻辑检查和实际程序集检查通过；新增检查防止原生提示区引用和旧替换补丁回归。已退出游戏后备份旧安装，部署 Mods/BetterHotkeys，移走 Mods/StarterMod；DLL 哈希一致。独立说明布局待游戏内验证。
