# License Generator

[Español](../es/README.md) · [English](../../README.md) · [Deutsch](../de/README.md) · [Português](../pt/README.md) · **中文**

用于签发、检查和管理离线软件许可证的桌面工具。为每个产品创建独立的 RSA 密钥对，签名许可证，使用可信公钥在本地检查许可证，并在您自己的应用中验证许可证。

[Microsoft Store](https://apps.microsoft.com/detail/9NR38VBCF8JK) · [下载](https://github.com/gonzapersonalsite/license-generator-docs/releases) · [报告问题](https://github.com/gonzapersonalsite/license-generator-docs/issues) · [支持开发](https://ko-fi.com/gonzaloapps)

## 功能

- 使用 SHA-256 和 PKCS#1 v1.5 的 RSA-2048 签名；标准 PEM 密钥。
- 每个已签名的许可证都包含产品、客户名称、硬件 ID 和可选的到期日期。
- 可选的已签名 **UserDefined** 文本，用于功能、套餐或应用专属的元数据。
- 本地应用程序管理、可搜索的历史记录，以及 ZIP 备份和导入。
- 使用明确信任的公钥在本地检查许可证，并分别给出签名、产品、硬件和到期结果。
- 提供英语、西班牙语、德语、巴西葡萄牙语和简体中文界面。
- 独立的 Windows x64 和 Linux x64 版本；最终用户无需安装 .NET。Linux 仍需要 Avalonia 所需的桌面系统库。

## 开始使用

1. 在 **应用程序** 中创建产品。
2. 获取其 `public.pem`，并按照[集成指南](INTEGRATION.md)为您的客户端应用添加许可证验证。
3. 输入客户名称、您的客户端提供的硬件 ID，以及可选的到期日期。
4. 如需使用现有的许可证格式，请将 UserDefined 留空；在客户端更新为支持格式 2 之后，可以输入自定义文本。
5. 生成并交付许可证代码。请妥善保管私钥和备份。

UserDefined 是已签名的文本，不是可执行的规则，也不是内置的功能管理器。其内容的含义由您的应用在验证签名**之后**决定。

## 文档

- [使用指南](OPERATIONS.md)：产品、签发、检查、备份和您的数据。
- [集成指南](INTEGRATION.md)：在您的应用中验证许可证，从激活界面到硬件标识符，附有 C#、Python 和 Node.js 示例。
- [许可证格式与安全契约](ARCHITECTURE.md)：已签名的确切字节、验证规则和安全边界。
- 参考验证器：[C#](../examples/LicenseVerifier.cs) 及其[共享协议](../examples/LicenseProtocol.cs)、[Python](../examples/python/license_verifier.py) 和 [Node.js](../examples/node/license-verifier.mjs)，全部使用相同的[测试向量](../examples/test-vectors.json)检查。
- [更新日志](../CHANGELOG.md)（英文）。

许可证是 Base64 编码的 JSON，不是加密的机密。硬件绑定和到期取决于您的客户端实现的检查；离线许可证无法阻止客户端被修改或系统时钟被篡改。

## 数据与许可

密钥、历史记录、设置和诊断日志位于您的操作系统用户的数据文件夹中，与可执行文件分开；[隐私政策](https://gonzaloapps.es/en/apps/license-generator/legal/privacy-policy/)（英文）说明了每个版本中该文件夹的位置及其内容。移动可执行文件不会移动数据。更换计算机、卸载或修改密钥之前，请先导出备份。

License Generator 是依据 [EULA](../../LICENSE)（英文）授权的专有软件。在这些条款下，生成的许可证和公钥可以随您自己的商业或免费应用分发，`docs/examples` 下的集成示例也可以复制到这些应用中。

由 Gonzalo Martínez García 开发 · gonzalo.dev
