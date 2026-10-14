# License Generator

[Español](../es/README.md) · [English](../../README.md) · [Deutsch](../de/README.md) · [Português](../pt/README.md) · **中文**

用于离线签发、检查和管理软件许可证的桌面工具。每个产品拥有独立的 RSA 密钥对，您可以使用可信公钥在本地检查许可证，并在自己的应用中验证许可证。

[Microsoft Store](https://apps.microsoft.com/detail/9NR38VBCF8JK) · [报告问题](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [支持开发](https://ko-fi.com/gonzaloapps)

- RSA-2048、SHA-256 和 PKCS#1 v1.5 签名；标准 PEM 密钥。
- 产品标识、客户名称、可选硬件绑定及可选到期日期。
- 签名保护的 **UserDefined** 文本，可用于功能、套餐或自定义元数据。
- 产品管理、历史记录和 ZIP 备份。
- 使用明确选择的可信公钥在本地检查许可证，分别显示签名、产品、硬件和有效期的检查结果。
- 简体中文、英语、西班牙语、德语及巴西葡萄牙语界面。
- 独立的 Windows x64 和 Linux x64 版本，无需单独安装 .NET。Linux 仍需要相应的桌面系统库。

创建产品并将 `public.pem` 集成到您的应用中。切勿分发 `private.pem`。UserDefined 留空时保持旧格式；填写自定义文本需要客户端支持格式 2。

[使用与集成](OPERATIONS.md) · [架构与安全](ARCHITECTURE.md) · [C# 验证示例](../examples/LicenseVerifier.cs)

数据保存在当前用户的配置目录中，与程序文件分离。将程序复制到 U 盘不会转移密钥或历史记录。ZIP 备份包含未加密的私钥。

本软件为专有软件，受 [EULA](../../LICENSE) 约束。根据协议，您可随自己的产品分发生成的许可证和公钥。

开发者：Gonzalo Martínez García · gonzalo.dev
