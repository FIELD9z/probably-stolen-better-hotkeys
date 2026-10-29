# Project guidance

- 本项目面向 Probably Stolen 正式版，先读 README.md 与 docs/research.md。
- 使用本机正式版 Managed DLL 验证 API；不要将 Demo 的 MelonLoader 教程套用到本工程。
- 不把文档中 “in development” 的 API 当作已支持；以编译和游戏内验证为准。
- 本机路径配置放 .local/Game.props；游戏程序集不复制入仓库或发布包。
- 先使用 ModHook，必要时再引入 Harmony。初始化阶段不访问尚未加载的存档。
- 编译命令：./scripts/Build.ps1。编译通过不代表游戏内验证通过。
- 未明确要求时不自动部署、启动游戏或上传 Workshop。
