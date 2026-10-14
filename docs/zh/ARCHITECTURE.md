# 架构与安全

[首页](README.md) · [英文规范](../../ARCHITECTURE.md)

应用使用 Avalonia 界面、MVVM 命令协调以及负责签名、存储、设置和桌面集成的服务。签发方保留私钥，客户端只接收公钥。

[统一规范](../../ARCHITECTURE.md) 定义 JSON 类型、被签名的字节、格式版本和验证规则。[C# 示例](../examples/LicenseVerifier.cs) 与应用一同编译和测试。

- 许可证是经过 Base64 编码的 UTF-8 JSON，不是加密数据。
- 签名算法为 RSA-2048、SHA-256 和 PKCS#1 v1.5。
- UserDefined 为空时保持旧格式，不输出新属性。
- 非空 UserDefined 使用格式 2，需要更新客户端。
- 拒绝未知版本及不一致的字段组合。格式 2 验证失败后不得回退到旧格式。
- 旧格式拒绝文本字段中的 `|`，避免字段边界歧义。
- 到期时间为指定日期**开始时的 UTC 00:00**。每次访问判断都应重新验证。
- HardwareId 为空表示不绑定设备；非空时必须完全匹配。

数据位于 `<LocalApplicationData>/LicenseGenerator/`，包括 `Keys/`、`Licenses/`、`settings.json` 和 `Logs/`。ZIP 备份未加密。导入会添加缺失数据、拒绝冲突，并以原子方式替换有效设置。

离线签名无法阻止客户端代码或系统时钟被修改。远程撤销需要额外机制。请参阅[安全边界](../../ARCHITECTURE.md#security-boundaries)和 [EULA](../../LICENSE)。
