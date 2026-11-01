# README 历史记录

以下段落迁自旧 README，保留原有措辞。其中尚未验证／未安装等是当时状态，不表示当前 0.5.3 的状态。当前状态见 [README](../README.md)，详细记录见 [research.md](research.md)。

0.3.0 新增 D 需求高亮、C 按标签收纳，以及容器标题栏的收纳设置。储藏升级后每页独立规则，支持当前可见页 / 全部页模式。代码与自动检查已完成，尚待游戏内验证，未自动安装。详见 [新功能规格与验收](demand-storage-spec.md)。

0.4.0 增加同种物品白／黑名单、连续点选、非占位虚影预览和手动目标冲突确认。自动收纳按“白名单 → 标签 → 可选空规则兜底”执行，黑名单全程排除。仍需游戏内验证，未自动部署。

之前版本已由用户进游戏验证加载；0.2.1 配置面板、语言切换、重启持久化及兼容模式仍待游戏内验证。构建脚本不自动部署或启动游戏；用户已授权助手在游戏退出后安装更新包。

当前 0.4.0 通过正式版编译、139 项逻辑检查及程序集绑定检查，但新增功能仍在游戏内测试阶段。公开源码供玩家审查与自行编译；它不是对任意来源二进制文件的认证。请核对下载来源和对应源码版本。

0.4.1 preserves native shortcut hints. Open the independent shortcut guide from this mod’s settings; apply binding changes and reopen to refresh. Assembly/package name: BetterHotkeys; the internal mod ID is unchanged. Remove the old StarterMod installation before installing the renamed package.
