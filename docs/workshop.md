# Workshop release draft / 创意工坊发布草稿

Title: Better Hotkeys / 更好的快捷键

Author: FIELD9z

Version: 0.2.1 — full game App ID 4348910, locally verified internal game version 0.46D.

## 当前发布材料（2026-10-29）

- 玩家页面描述以 [中文版](workshop-description.zh-CN.txt) 和 [英文版](workshop-description.en.txt) 为准；下方为早期草稿。两份描述含作者原文、完整操作说明及顾客需求高亮的后续计划。
- 现有封面：`artwork/better-hotkeys-cover-v1.png`；约 1.45 MB，上传副本方案待用户选择。
- 发布包：`dist/StarterMod/`；0.2.1 / 游戏内部版本 0.46D，包含 StarterMod.dll、manifest.xml、USER-GUIDE.md。
- 已重新编译并通过 110 项逻辑检查及程序集绑定检查。内部游戏验证状态不因移除玩家文案中的开发说明而改变。
- 本机上传配置：`.local/workshop/better-hotkeys.vdf`，当前为新条目 ID 0、私密可见性 2；尚未上传，待确认现有条目及发布可见性。首次上传使用英文描述，之后在创意工坊编辑页面单独填写简体中文标题与描述。
- 成功上传后 SteamCMD 会将条目 ID 写回 VDF，后续更新须保留该 ID。上传前不得重新生成覆盖已有 ID 的配置。

## English description

Customizable hotkeys for clear inventory routes, batch pinning, header sorting and stolen/contraband markers. Configure keys, marker gestures, colors, notices and compatibility modes through the game's ModConfig interface. English and Simplified Chinese included.

Defaults: Shift-click for sales-counter transfers; Ctrl-click for inventory/container transfers; Alt-click to pin; hover a header and use the game's sort key; double B to toggle item markers. Items remain subject to ownership, pinning, capacity and native movement rules. Nested contents can contribute to a container's marker without opening it.

TakeOver disables native quick transfers. VanillaWithHelpers restores them and disables this mod's transfer/pin gestures while retaining sorting and markers. Known limitation: input mods may conflict; shared Harmony patches are logged, but independent key polling cannot be fully detected.

No new save schema is added; transfers, pinning and sorting change the normal game state and are saved by the game. Preferences are global ModConfig settings. Markers do not perform inspections or alter heat. No telemetry or network calls are added.

0.2.1 has passed compilation and local logic/binding checks; configuration UI and gameplay integration still need in-game verification before a public release claim. See the included USER-GUIDE.md for controls and limitations.

## 中文描述

可自定义的快捷键：明确目标转移、同来源批量固定、标题栏整理、赃物/违禁品标记。通过游戏 ModConfig 调整按键、标记手势、颜色、提示及兼容模式，支持英文和简体中文。

默认 Shift＋左键连接出售柜台，Ctrl＋左键连接库存与容器，Alt＋左键固定，悬停标题栏使用游戏整理键，双击 B 开关标记。保留所有权、固定、容量和原生移动限制；无需打开容器即可汇总内部风险类别。

接管模式停用原生快速转移；原生＋辅助模式恢复原生转移并停用本 Mod 转移/固定手势，保留整理与标记。输入类 Mod 仍可能冲突，共享补丁会记录日志，但无法完全识别独立按键轮询。

不新增存档结构；转移、固定、整理会改变正常游戏状态并由游戏保存。配置跨存档共用；标记不执行盘查、不改变热度。不添加遥测或联网行为。

0.2.1 已通过编译与本地逻辑/绑定检查，配置界面及玩法仍需游戏内验证。操作说明见包内 USER-GUIDE.md。

## Packaging and publication notes

- Only our DLL, manifest and usage document are packaged. Do not include game assemblies, saves, local paths or third-party assets without rights.
- Manifest declares reflection-external, required for private input/selection/UI access. Internal mod ID and StarterMod.dll filename remain unchanged to preserve settings and avoid installing a duplicate identity.
- Public upload is a separate user action: review the description, actual gameplay results and uploader rights; accept Steam's applicable agreement through Steam. No Workshop item has been created or uploaded by this task.
- A preview image and public visibility choice can be prepared when publishing is requested; neither is falsely represented as completed here.

## Sources checked 2026-10-29

- [Game modding code of conduct](https://github.com/dragonpan2/probably-stolen-modding/blob/main/CodeOfConduct.md): capability disclosure, accurate behavior descriptions, no unauthorized game/third-party assets, no hidden data transmission or malicious behavior.
- [Game modding guide](https://github.com/dragonpan2/probably-stolen-modding/blob/main/ModdingGuide.md): manifest, assembly and version requirements.
- [Steam Subscriber Agreement, section 6](https://store.steampowered.com/agreement/?l=english): rights and terms for user-generated content and Workshop contributions.
- [Full-game Workshop](https://steamcommunity.com/app/4348910/workshop/): the full-game destination. Bilingual support is this project's requirement, not a Steam-wide requirement.
