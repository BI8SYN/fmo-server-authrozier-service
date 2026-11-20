# 历史内置角色模板

本目录保存**曾经随发行版发布过**的默认角色文件内容。

启动时 `AclStore` 只有在服务器上的角色文件命中这里的某个哈希时，才会认为它是
“官方旧默认”并自动升级；否则视为用户自定义，原样保留。

## 规则

- **只增不改不删。** 编辑已有文件会让仍停留在该默认上的部署无法升级。
- 修改 `../*.json` 之前，先把当前内容复制一份进来并起一个新名字，然后：
  1. 把 `../<role>.json` 的 `_version` 加一；
  2. 提交这份历史副本。
- 命名格式：`<role>.<标签>.json`，标签里**不能包含点**（第一个 `.` 之前是角色名，
  之后是任意标签）。文件会被打包成嵌入资源 `acl.history.<角色>.<标签>.json`。
- 历史文件不再需要维护 `_version`：哈希计算会忽略该字段，保留原样即可。
- 如果某一代的内容与**当前**模板完全相同（仅 `_version` 不同），不必单独收录——
  启动时按内容相同直接跳过，规则本身已经一致。

## 当前收录

| 文件 | 来源 |
|---|---|
| `user.v0.json` / `admin.v0.json` / `super.v0.json` | 最初开源版本（`sub` 只有 `FMO/SERVER_INFO`，无 `deny`） |
| `user.v1.json` / `admin.v1.json` / `super.v1.json` | `SERVER_INFO` 改为 `FMO/SERVER_INFO/#` 的版本 |
| `super.v2-nosub.json` | 1.2.0 首次带 `_version: 2` 的 super（`deny: []`、`sub` 未补齐） |
| `super.v2-nodeny.json` | 补齐 super 的 `sub`，但尚无 SAFEPROBE deny |
