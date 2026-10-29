# Better Hotkeys & Storage / 更好的快捷键与收纳

FIELD9z 制作的正式版快捷键 Mod（0.5.0）：明确目标的快速转移、批量固定、标题栏整理、赃物/违禁品标记、顾客需求高亮、分页标签收纳及中英文 ModConfig。

打开游戏 Mods 中本 Mod 的设置，可调整组合键、功能开关、标记触发与显示、提示及输入兼容模式。默认操作保持不变。详见 [中英文使用说明](docs/user-guide.md)、[功能规格](docs/organizer-spec.md)及[发布说明](docs/workshop.md)。

English: Better Hotkeys by FIELD9z adds configurable transfer/pin chords, header sorting and stolen/contraband markers. Open this mod's settings in Mods; select Auto, English or 简体中文. See the [bilingual user guide](docs/user-guide.md).

0.3.0 新增 D 需求高亮、C 按标签收纳，以及容器标题栏的收纳设置。储藏升级后每页独立规则，支持当前可见页 / 全部页模式。代码与自动检查已完成，尚待游戏内验证，未自动安装。详见 [新功能规格与验收](docs/demand-storage-spec.md)。

0.4.0 增加同种物品白／黑名单、连续点选、非占位虚影预览和手动目标冲突确认。自动收纳按“白名单 → 标签 → 可选空规则兜底”执行，黑名单全程排除。仍需游戏内验证，未自动部署。

## 开始开发

1. 安装 .NET SDK 6.0 或更高版本，并安装 Steam 正式版游戏。
2. 将 `config/Game.props.example` 复制到 `.local/Game.props`，修改 `GameDir` 为游戏目录。当前机器已配置；本机路径不提交 Git。
3. 在项目根目录运行：

```powershell
./scripts/Build.ps1
```

编译输出：`src/BetterHotkeys/bin/Release/netstandard2.1/BetterHotkeys.dll`。
也可以通过 `./scripts/Build.ps1 -GameDir '你的游戏安装目录'` 临时指定路径。

## 打包及游戏内验证

使用 Player.log 中 ModLoader audit 的内部游戏版本打包。当前本机是 `0.46D`，不是主菜单显示的 `VERSION 100-Rev3`：

```powershell
./scripts/Build.ps1 -GameVersion '0.46D'
```

输出 `dist/BetterHotkeys/`，包含自己的 DLL 和已填写版本的 manifest。Steam Build ID 不是 manifest 的游戏版本号。

退出游戏，将整个安装包目录复制到游戏的 `Mods/BetterHotkeys/`（含 DLL 与 manifest.xml）。启动游戏，在 Mods 菜单启用并重启，通过 F8 查看 `Better Hotkeys initialized` 和 `Better Hotkeys enabled` 日志。退出后检查停用日志；不能将编译通过等同于玩法验证通过。使用单独测试存档。

之前版本已由用户进游戏验证加载；0.2.1 配置面板、语言切换、重启持久化及兼容模式仍待游戏内验证。构建脚本不自动部署或启动游戏；用户已授权助手在游戏退出后安装更新包。

逻辑测试：`dotnet run --project tests/Organizer.Tests/Organizer.Tests.csproj -c Release`。程序集绑定检查：`./scripts/Test-GameBindings.ps1`。发布前确认作者信息与兼容版本。

## 资料

- [调查与本机验证记录](docs/research.md)
- [开发指南（中文）](https://github.com/dragonpan2/probably-stolen-modding/blob/main/ModdingGuide.zh-CN.md)
- [快速入门（中文）](https://github.com/dragonpan2/probably-stolen-modding/blob/main/README.zh-CN.md)
- [API 变更记录](https://github.com/dragonpan2/probably-stolen-modding/blob/main/APIChanges.md)

## 开源与可审查性 / Source transparency

本项目自己的 Mod 源码采用 [MIT License](LICENSE)，作者 FIELD9z。Probably Stolen 及 Unity、Harmony 等第三方程序集不包含在仓库中，也不由本项目重新授权。自行编译需要合法安装的正式版游戏；按照上方步骤配置 `.local/Game.props` 后运行 `scripts/Build.ps1`。

源码中的 `manifest.xml` 申请 `reflection-external`，用于读取正式版游戏内部输入、选择和窗口成员；相关实现见 `GameAccess.cs`、`InputPatches.cs` 和 UI 模块。仓库不包含游戏 DLL、存档、本机配置或构建产物。

可独立运行逻辑测试（无需游戏程序集）：

```powershell
dotnet run --project tests/Organizer.Tests/Organizer.Tests.csproj -c Release
```

当前 0.4.0 通过正式版编译、139 项逻辑检查及程序集绑定检查，但新增功能仍在游戏内测试阶段。公开源码供玩家审查与自行编译；它不是对任意来源二进制文件的认证。请核对下载来源和对应源码版本。

The mod's own source is MIT-licensed. Game and third-party assemblies are excluded and retain their own licenses. Build locally against a legitimate full-game installation; local paths, saves and build outputs are not published. The `reflection-external` capability supports the version-sensitive game input/UI integration. Source availability enables inspection and local builds, but does not authenticate binaries from arbitrary sources. Version 0.4.0 is still undergoing in-game testing.


0.4.1：保留原生快捷键提示，不再隐藏或替换原提示区域。在本 Mod 设置底部点击「Better Hotkeys 快捷键说明」查看独立说明（应用改键后重新打开刷新）。程序集及安装目录统一为 BetterHotkeys；内部 ID 保持 probably_stolen_organizer，已有设置与规则沿用。升级前退出游戏，移走旧 Mods/StarterMod 目录，避免重复安装。

0.4.1 preserves native shortcut hints. Open the independent shortcut guide from this mod’s settings; apply binding changes and reopen to refresh. Assembly/package name: BetterHotkeys; the internal mod ID is unchanged. Remove the old StarterMod installation before installing the renamed package.


0.5.0 发布：更名“更好的快捷键与收纳 / Better Hotkeys & Storage”。包含需求高亮、分页规则收纳、物品与标签黑白名单、标签采集及预览修复，保留 0.2.3 的本地化与提示修复。内部 ID、DLL 和安装目录保持不变。
