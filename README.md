# Better Hotkeys / 更好的快捷键

FIELD9z 制作的正式版快捷键 Mod（0.2.2）：明确目标的快速转移、批量固定、标题栏整理、赃物/违禁品标记及中英文 ModConfig。

打开游戏 Mods 中本 Mod 的设置，可调整组合键、功能开关、标记触发与显示、提示及输入兼容模式。默认操作保持不变。详见 [中英文使用说明](docs/user-guide.md)、[功能规格](docs/organizer-spec.md)及[发布说明](docs/workshop.md)。

English: Better Hotkeys by FIELD9z adds configurable transfer/pin chords, header sorting and stolen/contraband markers. Open this mod's settings in Mods; select Auto, English or 简体中文. See the [bilingual user guide](docs/user-guide.md).

0.2.2 维护版本：保留已发布快捷键功能范围，移除原生快捷提示接管；使用 Mod 设置及使用说明查看实际绑定。顾客需求与自动收纳开发功能不包含在此分支。

## 开始开发

1. 安装 .NET SDK 6.0 或更高版本，并安装 Steam 正式版游戏。
2. 将 `config/Game.props.example` 复制到 `.local/Game.props`，修改 `GameDir` 为游戏目录。当前机器已配置；本机路径不提交 Git。
3. 在项目根目录运行：

```powershell
./scripts/Build.ps1
```

编译输出：`src/StarterMod/bin/Release/netstandard2.1/StarterMod.dll`。
也可以通过 `./scripts/Build.ps1 -GameDir '你的游戏安装目录'` 临时指定路径。

## 打包及游戏内验证

使用 Player.log 中 ModLoader audit 的内部游戏版本打包。当前本机是 `0.46D`，不是主菜单显示的 `VERSION 100-Rev3`：

```powershell
./scripts/Build.ps1 -GameVersion '0.46D'
```

输出 `dist/StarterMod/`，包含自己的 DLL 和已填写版本的 manifest。Steam Build ID 不是 manifest 的游戏版本号。

退出游戏，将整个安装包目录复制到游戏的 `Mods/StarterMod/`（含 DLL 与 manifest.xml）。启动游戏，在 Mods 菜单启用并重启，通过 F8 查看 `Better Hotkeys initialized` 和 `Better Hotkeys enabled` 日志。退出后检查停用日志；不能将编译通过等同于玩法验证通过。使用单独测试存档。

之前版本已由用户进游戏验证加载；0.2.1 配置面板、语言切换、重启持久化及兼容模式仍待游戏内验证。构建脚本不自动部署或启动游戏；用户已授权助手在游戏退出后安装更新包。

逻辑测试：`dotnet run --project tests/Organizer.Tests/Organizer.Tests.csproj -c Release`。程序集绑定检查：`./scripts/Test-GameBindings.ps1`。发布前确认作者信息与兼容版本。

## 资料

- [调查与本机验证记录](docs/research.md)
- [开发指南（中文）](https://github.com/dragonpan2/probably-stolen-modding/blob/main/ModdingGuide.zh-CN.md)
- [快速入门（中文）](https://github.com/dragonpan2/probably-stolen-modding/blob/main/README.zh-CN.md)
- [API 变更记录](https://github.com/dragonpan2/probably-stolen-modding/blob/main/APIChanges.md)


Source / 源码：[GitHub](https://github.com/FIELD9z/probably-stolen-better-hotkeys/tree/codex/0.2.2-text-fix)。本分支为 0.2.2 维护版；主分支含开发功能。
