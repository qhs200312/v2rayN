# v2rayN WinUI 3

基于 [v2rayN](https://github.com/2dust/v2rayN) 使用 WinUI 3 重构的 Windows 代理客户端。

本项目专注于 Windows 桌面体验，支持 Xray、sing-box、Mihomo 等代理核心，并修复了一些安装、更新、TUN 和系统代理相关问题。

## 主要功能

- WinUI 3 原生 Windows 界面
- 支持系统代理和 TUN 模式
- 支持路由规则、订阅和节点管理
- 支持通过系统代理检查和下载更新
- 支持自动退出应用并覆盖安装新版本
- 安装升级时保留原有节点、设置、日志和数据库

## 下载

请从 [GitHub Releases](https://github.com/qhs200312/v2rayN/releases/latest) 下载最新版本。

| 文件 | 适用设备 | 说明 |
| --- | --- | --- |
| `v2rayN-windows-64-setup.exe` | Windows x64 | 包含运行环境和代理核心，推荐使用 |
| `v2rayN-windows-64.zip` | Windows x64 | 便携版和应用内更新包 |
| `v2rayN-windows-arm64.zip` | Windows ARM64 | ARM64 便携版和应用内更新包 |

本项目仅发布 Windows 版本，不提供 Linux 或 macOS 版本。

## 系统要求

- Windows 10 2004（版本 19041）或更高版本
- x64 或 ARM64 处理器

## 升级说明

安装器可以识别已有的 v2rayN 便携版或安装目录并直接覆盖升级。升级过程中会保留 `guiConfigs`、日志和数据库，建议重要配置仍定期自行备份。

## 上游项目

本项目基于 [2dust/v2rayN](https://github.com/2dust/v2rayN) 开发。协议、核心支持范围及基础使用文档可参考[上游 Wiki](https://github.com/2dust/v2rayN/wiki)。
