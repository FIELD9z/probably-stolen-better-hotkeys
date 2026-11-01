# 本地安装教程（Windows 正式版）

适用 Better Hotkeys & Storage / 更好的快捷键与收纳。当前 Mod 版本为 0.5.3；本项目记录的正式版内部游戏版本为 `0.46D`。本教程使用游戏自带 Mods 加载方式，不需要另装 MelonLoader。

## 选择安装方式

- 只想玩：可在 [Steam 创意工坊页面](https://steamcommunity.com/sharedfiles/filedetails/?id=3829363629)订阅，进入游戏 Mods 菜单启用并按提示重启。
- 已有成品包：按下方“手动安装”操作，不需要 .NET SDK 或 Git。
- 从 GitHub 下载源码：先按“从源码编译”生成成品包，再手动安装。GitHub 的 **Code → Download ZIP** 和版本标签的源码 ZIP 是源码，不是可以直接放进 Mods 的成品包。

本教程不假定 GitHub 上存在可下载的二进制 Release。仓库不包含游戏 DLL；自行编译需要本机安装正式版游戏。

## 一、找到游戏目录并准备备份

1. 在 Steam 库中右键 Probably Stolen，打开“属性 → 已安装文件 → 浏览”（不同语言的文字可能略有不同）。打开的文件夹就是游戏根目录，应能看到 `Probably Stolen_Data` 文件夹。
2. 完全退出游戏后再复制文件。
3. 如果已有本地安装，将整个 `Mods/BetterHotkeys` 文件夹复制到 **Mods 目录之外**的备份目录，例如自己建立的 `ModBackups/2026-11-02/`。保留整份文件，而不只是 DLL。
4. 如仍有旧版 `Mods/StarterMod`，也先完整备份，再移出 Mods，避免加载两份同一 Mod。不要仅在 Mods 内改名留作备份。
5. 升级前另行备份自己使用的游戏存档；安装脚本只备份 Mod 文件，不备份存档或全局设置。

如果已经订阅工坊版，同时准备安装本地版，应先取消该 Mod 的订阅并确认工坊副本不再参与加载，再安装本地版，避免同一内部 ID 的两个副本同时存在。切回工坊版时也先将本地副本移出 Mods。

## 二、手动安装成品包

1. 找到包含以下四个文件的 `BetterHotkeys` 成品包文件夹；自行编译时位于仓库的 `dist/BetterHotkeys/`。
2. 在游戏根目录下创建 `Mods`（若不存在）。首次安装把成品包文件夹复制进去；升级时完成备份后，将旧 `Mods/BetterHotkeys` 移出 Mods，再复制新包。
3. 核对以下四个**最终文件路径**。不要多套一层 `BetterHotkeys` 或 `dist` 文件夹：

```text
<游戏根目录>/Mods/BetterHotkeys/BetterHotkeys.dll
<游戏根目录>/Mods/BetterHotkeys/manifest.xml
<游戏根目录>/Mods/BetterHotkeys/USER-GUIDE.md
<游戏根目录>/Mods/BetterHotkeys/THIRD-PARTY-NOTICES.txt
```

4. 启动游戏，在 **Mods** 菜单找到 **Better Hotkeys & Storage / 更好的快捷键与收纳**，启用并按提示重启。
5. 打开本 Mod 的设置，确认设置面板可用；使用测试存档检查快捷键和收纳功能，具体操作见[中英文使用说明](user-guide.md)。可通过 F8 日志查看 `Better Hotkeys initialized` 和 `Better Hotkeys enabled`。加载日志只表示初始化／启用，不等于全部功能验证通过。

## 三、从源码编译

### 准备工具与源码

安装 .NET SDK 6.0 或更高版本（只有 Runtime 不够）。在 PowerShell 执行 `dotnet --list-sdks`，确认存在 SDK。

从[仓库](https://github.com/FIELD9z/probably-stolen-better-hotkeys)下载并解压源码；希望复现当前发布版本时使用 [v0.5.3 源码](https://github.com/FIELD9z/probably-stolen-better-hotkeys/tree/v0.5.3)。也可用 Git 克隆仓库。

打开 Windows PowerShell 或 PowerShell 7，用 `Set-Location` 进入解压或克隆后的仓库根目录。该目录应包含 `README.md`、`scripts` 和 `src`：

```powershell
Set-Location 'D:\Projects\probably-stolen-better-hotkeys'
```

把示例路径替换为自己的路径；后续命令都在该目录执行。

### 配置游戏路径

首次配置时执行：

```powershell
New-Item -ItemType Directory -Force .local
Copy-Item config/Game.props.example .local/Game.props
notepad .local/Game.props
```

已有 `.local/Game.props` 时直接编辑，不必重新复制覆盖。将 `GameDir` 改成第一节找到的游戏根目录，例如：

```xml
<Project>
  <PropertyGroup>
    <GameDir>D:\SteamLibrary\steamapps\common\Probably Stolen</GameDir>
  </PropertyGroup>
</Project>
```

路径应指向游戏根目录，不能指向 `Mods` 或 `Managed`。配置后应存在 `<GameDir>/Probably Stolen_Data/Managed/Assembly-CSharp.dll`。本机配置无需提交 Git，游戏程序集也不要复制到源码或发布包中。

### 编译并生成安装包

以下命令适用于本项目已验证的内部游戏版本 `0.46D`：

```powershell
./scripts/Build.ps1 -GameVersion '0.46D'
```

成功后会显示 `Package created:`，安装包位于 `dist/BetterHotkeys/`。然后执行上面的“手动安装成品包”。仅运行 `./scripts/Build.ps1` 会编译 DLL，**不会生成或更新完整安装包**；两种命令都不会自动安装或启动游戏。

`GameVersion` 是游戏内部版本，不是 Steam Build ID，也不是主菜单的 `VERSION 100-Rev3`。如果游戏更新，应核对当前游戏的 `Player.log` 中 `ModLoader audit` 记录的版本，并重新确认 API 兼容；不要仅修改 manifest 版本号来跳过不兼容提示。可从 Windows 用户目录的 `AppData/LocalLow` 下查找该游戏的 `Player.log`。

## 四、可选：使用仓库安装脚本

普通玩家可直接手动安装。以下脚本适用于 **Git 克隆的开发仓库**，需要 Git、PowerShell 7（`pwsh`）、已配置的 `.local/Game.props` 和刚生成的 `dist/BetterHotkeys` 包。脚本使用的路径 API 不适用于 Windows PowerShell 5.1。

```powershell
pwsh -File ./scripts/Install.ps1
```

运行前退出游戏，并完成相关代码、测试、脚本和随包文档的验证与 Git 提交。源码 ZIP 没有 Git 历史，不能直接使用这个安装脚本；使用手动安装即可。

脚本会：

- 检查游戏进程；仍在运行则停止，不强制退出游戏。
- 检查相关文件是否有未提交改动，并核对包版本及主要文件是否过期。
- 将现有安装完整备份到 `.local/deploy-backups/<时间>-<提交短号>/installed/`，验证备份哈希。
- 复制到配置的游戏目录，核对包内文件哈希，并在同一备份目录保存 `deployment.json`，记录版本、提交、路径和结果。

首次安装没有旧文件，因此不会产生 `installed` 子目录，但仍有部署记录。脚本不会自动处理旧 `StarterMod` 或工坊副本，也不会备份存档、启动游戏或上传工坊。请先完成第一节的准备工作。

## 五、升级、回退与卸载

**升级：**退出游戏 → 备份旧 Mod 和存档 → 安装完整新包 → 启用并重启 → 在测试存档确认功能。内部 ID 保持不变，通常会沿用已有配置和规则。

**回退：**退出游戏，把当前 `Mods/BetterHotkeys` 整个移到 Mods 之外保留，再把备份中的完整旧文件夹恢复到 `Mods/BetterHotkeys`。使用脚本备份时，将对应 `installed` 文件夹的内容放回这个目标目录，不要把目录命名为 `installed`。可在 `deployment.json` 中核对安装记录。

Mod 文件备份不包含存档。尤其是在新版保存过规则后降级至不支持该规则格式的旧版本（例如含排除标签的新规则降到 0.4.5 之前），还需要恢复升级前的存档；只换旧 DLL 不够。

**卸载：**在游戏 Mods 中停用并退出游戏，将本地 `Mods/BetterHotkeys` 移出 Mods。工坊版另在 Steam 取消订阅。此操作不主动清理存档或全局设置。

## 六、常见问题

| 现象 | 检查方法 |
| --- | --- |
| Mods 菜单看不到本 Mod | 对照四个最终路径，确认没有多嵌套一层目录，且复制的是成品包而非源码。 |
| 出现重复条目或加载冲突 | 检查旧 `StarterMod`、本地 `BetterHotkeys` 与工坊副本，确保只留一个加载来源。 |
| 提示游戏版本不兼容 | 核对 manifest 与实际内部游戏版本；更新后需要验证兼容性，不要直接强改版本号。 |
| `dotnet` 无法识别或找不到 SDK | 确认安装了 .NET SDK，重新打开终端并执行 `dotnet --list-sdks`。 |
| 找不到 `Assembly-CSharp.dll` | 检查 `.local/Game.props` 的 `GameDir` 是否指向正式版游戏根目录。 |
| PowerShell 提示禁止执行脚本 | 检查错误所述的执行策略或下载文件阻止状态；个人电脑可在确认脚本内容后使用仅当前会话的 `Set-ExecutionPolicy -Scope Process RemoteSigned`，组织管理策略限制则由管理员处理。 |
| 安装脚本提示 Git、未提交改动或包过期 | ZIP 下载者使用手动安装；开发者检查 `git status`，提交已验证改动并重新打包。不要为绕过检查丢弃自己的修改。 |
| 安装脚本提示游戏仍在运行 | 正常退出游戏后重试，不替换游戏正在使用的文件。 |

本教程依据仓库脚本与现有验证记录编写。0.5.3 已通过正式版编译、189 项逻辑检查及程序集绑定检查，用户试用反馈“没有明显问题”；这不代表所有机器、分辨率和 Mod 组合均已测试。
