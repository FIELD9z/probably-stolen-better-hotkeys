# Better Hotkeys & Storage / 更好的快捷键与收纳

FIELD9z 制作的 Probably Stolen 正式版 Mod，当前版本 **0.5.3**。提供明确目标的快速转移、批量固定、标题栏整理、赃物／违禁品标记、顾客需求高亮、分页规则收纳及中英文设置。

## 安装与使用

- **Steam 订阅：**前往[创意工坊页面](https://steamcommunity.com/sharedfiles/filedetails/?id=3829363629)，订阅后在游戏 Mods 菜单启用并按提示重启。
- **本地安装或自行编译：**阅读[完整本地安装教程](docs/install.md)，包含游戏目录定位、成品包安装、源码编译、脚本安装、升级回退和常见问题。
- **快捷键与设置：**阅读[中英文使用说明](docs/user-guide.md)。在游戏 Mods 中打开本 Mod 设置，可调整按键、功能开关、标记、提示及输入兼容模式。

GitHub 的 **Code → Download ZIP** 是源码，不是成品安装包。自行编译后使用 `dist/BetterHotkeys/`；安装完成时 DLL 应位于 `<游戏根目录>/Mods/BetterHotkeys/BetterHotkeys.dll`。更新前退出游戏并备份，旧 `StarterMod` 与工坊副本的处理也见安装教程。

English: Use the [Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=3829363629) to subscribe, enable the mod in the game's Mods menu and restart as prompted. GitHub source ZIPs require compilation. See the [Windows local installation guide (Chinese)](docs/install.md) and [bilingual user guide](docs/user-guide.md).

## 当前版本与验证状态

0.5.2–0.5.3 支持容器间 Ctrl 转移及设备独立成品区的 Shift/Ctrl/C；物品与标签增加独立已选区、按组清空、分类展开、拼音首字母／全拼搜索。物品目录仅记录当前存档中曾显示的自有物品，保留旧名单。拼音数据随 DLL 内置，运行时无需联网；来源和许可见[第三方说明](THIRD-PARTY-NOTICES.txt)。

0.5.3 已通过正式版编译（0 警告／错误）、189 项逻辑检查及程序集绑定检查。用户安装试用后反馈“没有明显问题”，并已于 2026-11-01 发布至工坊；这不代表全部边界用例或其他 Mod 组合均已验证。记录见[发布说明](docs/release-0.5.3.md)、[发布核验](docs/workshop.md)与[研究记录](docs/research.md)。

旧 README 中按版本追加的状态说明已移到 [README 历史记录](docs/readme-history.md)，不作为当前安装状态。

## 开始开发

需要正式版游戏和 .NET SDK 6.0 或更高版本。首次将 `config/Game.props.example` 复制为 `.local/Game.props`，把 `GameDir` 改为自己的游戏根目录。本机配置不提交 Git，游戏程序集不复制入仓库或发布包。

在仓库根目录的 PowerShell 中执行：

```powershell
# 只编译 DLL
./scripts/Build.ps1

# 编译并生成完整安装包（0.46D 是本项目已验证的内部游戏版本）
./scripts/Build.ps1 -GameVersion '0.46D'
```

DLL 输出：`src/BetterHotkeys/bin/Release/netstandard2.1/BetterHotkeys.dll`；安装包输出：`dist/BetterHotkeys/`。构建脚本不会自动安装。游戏更新后需重新核对内部版本和兼容性，不能使用 Steam Build ID 或主菜单版本文字代替。

开发者可在满足前提后用 PowerShell 7 执行 `pwsh -File ./scripts/Install.ps1`，自动备份与核对安装哈希。该脚本依赖 Git 仓库、相关改动已提交、本机路径配置和有效安装包；源码 ZIP 用户使用手动安装。完整步骤及回退方法见[安装教程](docs/install.md)。

验证命令：

```powershell
# 无需游戏程序集的逻辑检查
dotnet run --project tests/Organizer.Tests/Organizer.Tests.csproj -c Release
# 需要本机正式版程序集的绑定检查
./scripts/Test-GameBindings.ps1
```

编译、检查及安装成功均不等于游戏内验证通过。

## 开源与可审查性 / Source transparency

本项目自己的 Mod 源码采用 [MIT License](LICENSE)，作者 FIELD9z。[源码仓库](https://github.com/FIELD9z/probably-stolen-better-hotkeys)与[当前发布版本 v0.5.3 源码](https://github.com/FIELD9z/probably-stolen-better-hotkeys/tree/v0.5.3)可供审查与自行编译。Probably Stolen 及 Unity、Harmony 等第三方程序集不包含在仓库中，也不由本项目重新授权。

源码中的 `manifest.xml` 申请 `reflection-external`，用于读取正式版游戏内部输入、选择和窗口成员；相关实现见 `GameAccess.cs`、`InputPatches.cs` 和 UI 模块。仓库不包含游戏 DLL、存档、本机配置或构建产物。源码公开不是对任意来源二进制文件的认证，请核对下载来源和对应源码版本。

The mod's own source is MIT-licensed. Game and third-party assemblies retain their own licenses and are excluded, along with saves, local configuration and build outputs. Build against a legitimate full-game installation. The `reflection-external` capability supports version-sensitive game input/UI integration. Version 0.5.3 passed compilation and automated checks; user playtesting found no obvious issues, without exhaustive coverage. Public source enables inspection and local builds but does not authenticate binaries from arbitrary sources.

## 更多资料

- [功能规格](docs/organizer-spec.md)与[需求高亮／收纳规格及验收](docs/demand-storage-spec.md)
- [调查与本机验证记录](docs/research.md)
- [开发指南（中文）](https://github.com/dragonpan2/probably-stolen-modding/blob/main/ModdingGuide.zh-CN.md)
- [快速入门（中文）](https://github.com/dragonpan2/probably-stolen-modding/blob/main/README.zh-CN.md)
- [API 变更记录](https://github.com/dragonpan2/probably-stolen-modding/blob/main/APIChanges.md)
