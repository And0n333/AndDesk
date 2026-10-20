# AndDesk 🖥️✨

<div align="center">

**专为 Windows 11 设计的现代化桌面图标收纳、启动与极简专注工具**

[![.NET 8](https://img.shields.io/badge/.NET-8.0%20LTS-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%2011%20%7C%2010-0078D6?logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![Architecture](https://img.shields.io/badge/Architecture-WPF%20%7C%20MVVM-00599C)](https://github.com/CommunityToolkit/dotnet)
[![Memory](https://img.shields.io/badge/Tray%20Memory-3~8%20MB-brightgreen)](#-超低资源占用与内存裁剪)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

[功能特性](#-功能特性) • [技术亮点](#-核心技术亮点) • [快速上手](#-快速上手与构建) • [项目结构](#-项目结构) • [安全与恢复机制](#-安全原则与恢复机制)

</div>

---

## 📖 项目简介

日常使用 Windows 过程中，桌面往往逐渐堆满快捷方式、临时文档与各类工程文件，造成视觉杂乱并分散注意力。

**AndDesk** 致力于在**绝对不改动用户物理文件**的前提下，为用户带来干净纯粹的桌面体验：
1. **一键隐匿**：通过 Windows Shell 原生指令，让物理桌面上所有图标瞬间隐匿，还你干净桌面与高清壁纸；
2. **中心聚合**：在优雅轻量的 Fluent 窗口中，集中浏览、检索并秒级双击启动桌面项目；
3. **极低常驻**：后台托盘待机时物理内存暴降至 **3MB ~ 8MB**，静默无感常驻。

---

## 🌟 功能特性

- 👁️ **非侵入式图标显隐**：仅控制 Windows Explorer 桌面视图可见性，**绝不移动、重命名、加密或删除**用户任何文件。
- 📂 **双桌面智能聚合**：自动聚合**当前用户个人桌面**、**公共桌面（Public Desktop）**以及 **OneDrive 重定向桌面**。
- 🚫 **智能系统杂项过滤**：自动识别并过滤 Windows 隐藏的 `desktop.ini`、缩略图缓存 `Thumbs.db` 以及 Office 临时锁定文件。
- 🖼️ **双模视图与高分屏适配**：
  - **网格卡片视图**：自适应居中卡片，提取并渲染 48x48 高清矢量图标。
  - **详细列表视图**：展示文件名称、格式类型、来源（个人/公共）、修改日期及完整路径。
- 🔍 **毫秒级防抖搜索**：支持在顶部检索框即时按名称与类型进行拼音与字符串模糊过滤。
- 🔄 **文件系统实时感知（FSW）**：内置 `FileSystemWatcher`，桌面一旦有文件新增、删除或重命名，经 400ms 智能防抖后自动无感刷新。
- 🪟 **系统托盘深度集成**：支持点击右上角 `[X]` 最小化收起到通知区域，右键托盘提供快捷显隐、主窗口唤起与设置入口。
- ⚙️ **丰富偏好设置**：
  - 开机自动启动（读写当前用户注册表，无需管理员权限）
  - 启动时最小化到托盘
  - 启动时自动隐藏物理桌面图标
  - 退出程序时尽力恢复物理桌面图标
  - 显示/隐藏系统文件过滤开关
- 🛡️ **立体安全恢复链路**：
  - 窗口内显隐控制
  - 托盘右键恢复
  - 命令行一键恢复：`AndDesk.exe --restore`
  - 附带独立超轻量恢复工具：`AndDeskRecovery.exe`（仅约 160 KB）

---

## ⚡ 核心技术亮点

### 1. 深入 Windows 11 Shell 层级的 DefView 探测
Windows 11 在启用壁纸幻灯片或动态壁纸时，`SHELLDLL_DefView` 句柄会从 `Progman` 转移到 `WorkerW` 顶级窗口下。AndDesk 实现了基于 `EnumWindows` 的双重遍历探测算法，并优先分发系统原生菜单消息：
```csharp
// 触发系统原生“查看 -> 显示桌面图标”操作，状态无损同步
SendMessage(hDefView, WM_COMMAND, (IntPtr)0x7402, IntPtr.Zero);
```

### 2. 超低资源占用与内存裁剪 (`EmptyWorkingSet`)
桌面收纳工具 95% 的时间都在后台静默待命。AndDesk 引入了进程工作集智能释放技术：
- 当窗口隐藏至系统托盘时，后台自动触发托管内存压缩并调用 Win32 API：
  ```csharp
  EmptyWorkingSet(Process.GetCurrentProcess().Handle);
  ```
- 任务管理器中的物理工作集内存占用瞬间从 **55MB 暴降至 3MB ~ 8MB**！
- 用户再次点开时毫秒级按需恢复，兼顾前台渲染丝滑与后台极简常驻。

### 3. 单实例互斥与具名管道唤醒（IPC）
通过 `Global\AndDesk_SingleInstance_Mutex` 防止多开冲突，并通过底层异步 `NamedPipeServerStream` 实现：当用户二次双击运行主程序时，第二个实例自动通知主进程唤醒前置窗口并优雅退出。

---

## 🛠️ 项目结构

```text
AndDesk/
├── AndDesk.sln                      # Visual Studio / Rider 解决方案
├── .gitignore                       # 完整 .NET 8 忽略配置
├── README.md                        # 项目说明文档
├── backups/                         # 关键里程碑源码归档
├── src/
│   ├── AndDesk/                     # 主应用程序 (WPF + .NET 8)
│   │   ├── Models/                  # 数据实体 (DesktopItem, AppSettings)
│   │   ├── ViewModels/              # MVVM 视图模型 (MainViewModel, SettingsViewModel)
│   │   ├── Views/                   # 界面视图 (MainWindow, SettingsWindow)
│   │   ├── Services/                # 核心业务服务
│   │   │   ├── DesktopScanner.cs    # 桌面多源扫描与过滤
│   │   │   ├── DesktopVisibility.cs # Shell 桌面图标显隐控制
│   │   │   ├── DesktopWatcher.cs    # 桌面变动实时监听与防抖
│   │   │   ├── ShellService.cs      # Win32 高清图标提取与项目启动
│   │   │   ├── StartupService.cs    # HKCU 登录自启管理
│   │   │   ├── SettingsService.cs   # JSON 配置原子读写
│   │   │   └── RecoveryService.cs   # --restore 命令行兜底处理
│   │   ├── Infrastructure/          # 单实例 IPC、内存裁剪器
│   │   └── App.ico                  # 应用程序图标资源
│   └── AndDeskRecovery/             # 独立无依赖恢复程序 (160 KB)
│       └── Program.cs               # 紧急桌面图标一键恢复
└── tests/
    └── AndDesk.Tests/               # xUnit 自动化单元测试
```

---

## 🚀 快速上手与构建

### 环境要求
- Windows 11 / Windows 10 (x64)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更高版本

### 1. 克隆仓库
```bash
git clone https://github.com/And0n333/AndDesk.git
cd AndDesk
```

### 2. 本地调试运行
```powershell
# 还原依赖并启动主程序
dotnet run --project .\src\AndDesk\AndDesk.csproj
```

### 3. 执行自动化测试
```powershell
dotnet test
```

### 4. 发布单文件便携绿色版 (Self-contained / R2R)
```powershell
# 发布主程序 (开启 ReadyToRun 优化)
dotnet publish .\src\AndDesk\AndDesk.csproj `
  -c Release `
  -r win-x64 `
  --self-contained false `
  -p:PublishSingleFile=true `
  -p:PublishReadyToRun=true

# 发布独立恢复工具
dotnet publish .\src\AndDeskRecovery\AndDeskRecovery.csproj `
  -c Release `
  -r win-x64 `
  --self-contained false `
  -p:PublishSingleFile=true
```
编译产物位于：
- `src\AndDesk\bin\Release\net8.0-windows\win-x64\publish\AndDesk.exe`
- `src\AndDeskRecovery\bin\Release\net8.0-windows\win-x64\publish\AndDeskRecovery.exe`

---

## 🔒 安全原则与恢复机制

> **重要声明：** 隐藏桌面图标不等于文件加密或隐私隔离。AndDesk 不会在未经用户知情的情况下上传或修改任何用户数据。

为确保用户桌面的绝对安全，AndDesk 设计了多重恢复兜底通道：
1. **主界面恢复**：顶部操作栏提供明显的“恢复桌面图标”按钮；
2. **托盘菜单**：在任务栏托盘图标右键菜单中，随时可点击“恢复桌面图标”；
3. **命令行恢复**：任何时候通过终端执行 `AndDesk.exe --restore` 均可静默唤醒桌面；
4. **独立恢复工具**：随发布包附带 `AndDeskRecovery.exe`，无需主程序存活即可直接恢复；
5. **系统原生入口**：在物理桌面空白处点击鼠标右键 ->【查看】-> 勾选【显示桌面图标】。

---

## 📄 开源许可证

本项目基于 [MIT License](LICENSE) 开源。欢迎提交 Issue 与 Pull Request！
