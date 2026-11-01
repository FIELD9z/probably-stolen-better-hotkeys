# 0.5.3 — 分类搜索与快捷转移 / Categorized search and quick transfers

- Ctrl 支持容器间互转；快捷操作保持所选目标，从目标容器取出仍返回库存。
- 设备独立成品区支持 Shift / Ctrl / C，保留游戏原生取出和插入限制。
- 已选物品、标签独立显示，支持逐项移除及允许／排除分别清空，保存后生效。
- 新增当前存档的已观察物品目录、原生类别展开和跨分类搜索。
- 物品、标签支持离线拼音首字母／全拼搜索及多音字。
- 默认赃物／违禁品标记键改为双击 V；已有按键设置保留。

- Ctrl now transfers between containers while preserving the selected destination; taking from that destination returns items to inventory.
- Shift / Ctrl / C support dedicated machine output areas while respecting native removal and insertion rules.
- Selected items and tags remain visible independently of filters, with individual removal and separate allow/exclude clearing. Save applies rule changes.
- Added an observed-item catalog per save, expandable native categories and global search.
- Item and tag search supports offline pinyin initials, full pinyin and alternative readings.
- The default stolen/contraband marker shortcut is now double V. Existing bindings are preserved.

Validation: release compilation, 189 logic checks and native API binding checks passed. The user tested installed 0.5.3 and reported no obvious issues; this does not claim exhaustive coverage of every device or UI configuration.

Implementation commits: `19e6d15`, `b30b301`. User playtest record: `5692c77`.
