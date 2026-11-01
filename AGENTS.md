# Project guidance

- 本项目面向 Probably Stolen 正式版，先读 README.md 与 docs/research.md。
- 使用本机正式版 Managed DLL 验证 API；不要将 Demo 的 MelonLoader 教程套用到本工程。
- 不把文档中 “in development” 的 API 当作已支持；以编译和游戏内验证为准。
- 本机路径配置放 .local/Game.props；游戏程序集不复制入仓库或发布包。
- 先使用 ModHook，必要时再引入 Harmony。初始化阶段不访问尚未加载的存档。
- 编译命令：./scripts/Build.ps1。编译通过不代表游戏内验证通过。
- 用户已长期授权：每次修复完成并通过相应编译/检查后，默认安装到本机正式版游戏，不再逐次询问安装许可。
- 修复使用 Git 管理：在功能分支保留范围清晰的提交，安装前提交本次已验证修改；不覆盖或丢弃用户未提交的改动。记录安装对应的提交和版本，确保可回退。
- 安装前确认游戏已退出，并将当前 Mod 安装完整备份到 .local/deploy-backups/；安装后核对 DLL、manifest 和随包文档哈希。保留旧安装备份和 Git 历史，不自动清理。
- 如果游戏仍在运行，先报告并请用户退出；不强制结束游戏、不替换正在使用的文件。退出后继续安装，无需重新请求安装许可。
- 默认安装不包含自动启动游戏、修改存档或上传 Workshop；这些仍需用户明确要求。编译通过与安装成功不代表游戏内验证通过。
- 用户已长期授权版本更新时同步推送 GitHub：每次更新版本，须将对应已验证的源码提交推送到 `https://github.com/FIELD9z/probably-stolen-better-hotkeys`，不能把仅本地提交当作已同步。保留功能分支历史；合入远端主分支前核对远端状态，不强推或覆盖他人提交，不夹带用户未提交的改动、本机配置或游戏程序集。
- 对外发布每个版本前，建立并推送对应的 `v<版本号>` 标签，确认其源码与发布包一致且远端可访问；已有发布标签不得静默移动或覆盖。先完成 GitHub 源码同步，再更新 Workshop，避免发布不存在或指向旧版的源码链接。
- Workshop 中英文描述必须始终保留独立的“源码 / Source”栏目，同时包含仓库链接 `https://github.com/FIELD9z/probably-stolen-better-hotkeys` 和当前发布版本源码链接 `https://github.com/FIELD9z/probably-stolen-better-hotkeys/tree/v<版本号>`。精简、翻译或重写描述时不得删除这两个入口；版本更新时必须同步更新版本源码链接。
- 版本发布完成前，回读 GitHub 远端提交和标签，并核对 Workshop 中英文公开页面的源码链接文字及实际目标地址；记录发布版本、源码提交、标签及验证结果。同步或核验失败时明确报告未完成项，不宣称全部发布完成。此同步规则不扩大自动启动游戏、修改存档或上传 Workshop 的授权范围。
