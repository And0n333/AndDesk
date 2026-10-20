# AndDesk — 完整开发者文档

- **项目名称：** AndDesk
- **产品定位：** Windows 11 桌面图标收纳与启动工具
- **文档版本：** 1.0
- **文档状态：** 设计基线 / MVP 开发规范
- **目标平台：** Windows 11 x64（后续可扩展 ARM64）
- **建议技术栈：** C#、.NET LTS、WPF、Windows Shell API
- **核心行为：** 用户登录后可后台驻留系统托盘；桌面图标可选择隐藏；在 AndDesk 窗口中查看、搜索和启动桌面项目。

> 重要声明：隐藏 Explorer 桌面图标与移动真实文件是两种不同操作。AndDesk 默认不得移动、删除、重命名或覆盖用户桌面文件。Windows Shell 的桌面视图窗口实现细节可能随系统更新改变，因此隐藏/恢复功能必须在目标 Windows 版本上进行集成测试，并提供恢复入口。

---

## 1. 项目概述

### 1.1 项目背景

传统 Windows 桌面会逐渐积累快捷方式、文件和文件夹，影响桌面整洁。AndDesk 将桌面项目集中显示在一个可搜索的应用窗口中，并允许用户选择隐藏 Windows 桌面图标，让桌面保持简洁。

### 1.2 项目目标

1. 扫描当前用户桌面与公共桌面中的文件系统项目。
2. 以列表或图标网格方式显示项目。
3. 通过 Windows Shell 打开文件、文件夹和快捷方式。
4. 支持名称搜索、手动刷新。
5. 支持系统托盘常驻和用户登录时启动。
6. 提供隐藏/恢复桌面图标的显式操作。
7. 在异常退出、应用升级和卸载情况下尽可能降低用户桌面状态受损的风险。

### 1.3 非目标（MVP 不实现）

- 不将用户文件移动到隐藏目录。
- 不删除或重命名桌面文件。
- 不将隐藏桌面图标描述为安全、加密或隐私保护。
- 不保证枚举所有虚拟 Shell 项目（例如“此电脑”“回收站”）。
- 不在首版实现云同步、跨设备同步或账号系统。
- 不承诺隐藏图标功能在所有第三方桌面增强软件下都有效。

---

## 2. 术语

| 术语 | 含义 |
|---|---|
| 桌面项目 | 当前用户桌面或公共桌面目录中的文件、快捷方式或文件夹 |
| 桌面图标视图 | Windows Explorer 中负责显示桌面图标的 Shell 视图 |
| 隐藏桌面图标 | 隐藏 Explorer 的桌面图标视图；不移动或删除文件 |
| 托盘 | Windows 通知区域中的 AndDesk 图标及菜单 |
| 恢复模式 | 仅执行恢复桌面图标操作并退出的启动模式 |
| MVP | 最小可行产品 |

---

## 3. 用户场景

### UC-01：首次启动
1. 用户运行 AndDesk。
2. 应用扫描个人桌面和公共桌面。
3. 主窗口展示项目名称、图标、类型和路径。
4. 用户可以双击项目启动目标。

### UC-02：登录后后台驻留
1. 用户在设置中启用“登录 Windows 时启动”。
2. 用户下一次登录后 AndDesk 启动。
3. 应用显示托盘图标；根据用户设置决定是否隐藏桌面图标。
4. 用户双击托盘图标打开主窗口。

### UC-03：隐藏桌面图标
1. 用户点击“隐藏桌面图标”。
2. AndDesk 尝试隐藏 Explorer 桌面图标视图。
3. AndDesk 在状态区显示操作结果。
4. 文件仍留在原位置，可通过资源管理器访问。

### UC-04：恢复桌面图标
1. 用户从主窗口或托盘菜单选择“恢复桌面图标”。
2. AndDesk 尝试重新显示 Explorer 桌面图标视图。
3. 状态区显示恢复结果。
4. 用户也可以通过 `AndDesk.exe --restore` 启动恢复模式。

### UC-05：桌面项目不可用
1. 用户双击一个已被移动或删除的项目。
2. AndDesk 显示友好错误。
3. 应用不崩溃，并允许重新扫描。

---

## 4. 功能需求

优先级定义：P0 = MVP 必须；P1 = 重要；P2 = 后续增强。

| ID | 功能需求 | 优先级 | 验收标准 |
|---|---|---|---|
| FR-001 | 扫描个人桌面 | P0 | 显示可枚举项目 |
| FR-002 | 扫描公共桌面 | P0 | 显示公共桌面项目且避免重复 |
| FR-003 | 显示项目名称和路径 | P0 | 列表内容与文件系统一致 |
| FR-004 | 显示文件夹/文件类型 | P0 | 类型标识正确 |
| FR-005 | 双击打开项目 | P0 | 通过 Windows Shell 打开，错误可见 |
| FR-006 | 搜索项目 | P0 | 按名称过滤，忽略大小写 |
| FR-007 | 手动刷新 | P0 | 重新扫描两个桌面目录 |
| FR-008 | 系统托盘 | P0 | 双击托盘可显示主窗口 |
| FR-009 | 隐藏桌面图标 | P0 | 尝试隐藏 Shell 桌面图标视图并反馈结果 |
| FR-010 | 恢复桌面图标 | P0 | 提供窗口、托盘和命令行恢复入口 |
| FR-011 | 登录启动 | P1 | 用户可启用/禁用，不要求管理员权限 |
| FR-012 | 启动时最小化 | P1 | 按设置启动到托盘 |
| FR-013 | 文件变化监听 | P2 | 新增/删除/重命名后自动刷新 |
| FR-014 | 自定义图标网格 | P2 | 支持不同视图和缩放 |
| FR-015 | 全局快捷键 | P2 | 快捷键冲突时有提示并可关闭 |

---

## 5. 非功能需求

### 5.1 稳定性
- 单个文件无法读取时，跳过该文件并继续扫描。
- 不能因为一个桌面目录权限不足而终止整个应用。
- 文件打开失败时显示可理解的错误信息。
- 托盘图标退出流程必须尽最大努力恢复桌面图标。

### 5.2 安全与隐私
- 默认不需要管理员权限。
- 默认不移动、删除、覆盖或上传用户文件。
- 不收集遥测数据；若未来加入，必须明确告知并提供关闭选项。
- 注册表只允许修改 AndDesk 自己拥有的启动项值。
- 不应把桌面图标隐藏宣传为文件保护或隐私屏障。

### 5.3 性能
- 普通桌面项目数量下，扫描应保持界面可响应。
- 文件系统扫描可在后台线程执行，完成后切回 UI 线程更新列表。
- 图标缓存应有上限或可清理策略。
- 文件变化监听应进行防抖，避免连续事件造成频繁全量刷新。

### 5.4 可维护性
- 使用 MVVM 或清晰的分层架构。
- 文件扫描、启动项管理、桌面视图控制、设置存储应分离。
- Windows API 调用集中在独立服务中。
- 所有系统调用必须有错误处理与日志记录。

---

## 6. 技术选型

### 6.1 推荐技术栈

| 层 | 技术 |
|---|---|
| 语言 | C# |
| 目标框架 | `.NET 10 LTS` Windows 目标框架（构建时以官方支持状态为准） |
| UI | WPF |
| 架构 | MVVM |
| 桌面目录定位 | `Environment.GetFolderPath` 或 Windows Known Folder API |
| 文件枚举 | `System.IO` |
| 打开项目 | `ProcessStartInfo.UseShellExecute = true` |
| 托盘 | `System.Windows.Forms.NotifyIcon` |
| 用户登录启动 | 当前用户 Run 注册表项，或经验证的 Windows 启动应用机制 |
| 图标提取 | Windows Shell API（推荐）或初版的关联图标提取 |
| 设置 | `%LocalAppData%\\AndDesk\\settings.json` |
| 日志 | `Microsoft.Extensions.Logging` 或轻量本地日志 |

### 6.2 为什么使用 WPF
WPF 是 Windows 原生桌面应用技术，适合图标列表、搜索、设置窗口、系统托盘集成与 Windows API 互操作。若未来更重视跨平台或 Web 风格 UI，可评估 Avalonia 或 Electron，但本项目主要面向 Windows 11，因此 WPF 是合理的首选。

### 6.3 重要 API 边界
桌面图标隐藏功能经常依赖 Explorer 的 `SHELLDLL_DefView` 窗口。该窗口类名和宿主结构属于 Windows Shell 实现细节，并非稳定的公开桌面图标控制契约。实现前必须：
- 在目标 Windows 11 版本测试；
- 检查调用后是否真的改变可见状态；
- 处理 Explorer 重启、多个显示器和第三方桌面工具；
- 提供显式恢复命令；
- 不把隐藏状态当作永久有效的设置。

---

## 7. 系统架构

```text
┌────────────────────────────────────────────┐
│                 WPF Views                  │
│ MainWindow / SettingsWindow / Status UI    │
└──────────────────────┬─────────────────────┘
                       │
┌──────────────────────▼─────────────────────┐
│              ViewModels / Commands         │
│ Search / Refresh / Open / Hide / Restore   │
└──────────────────────┬─────────────────────┘
                       │
┌──────────────────────▼─────────────────────┐
│                 Application Services       │
│ DesktopScanner / ShellService / Settings   │
│ StartupService / TrayService / Logger      │
└───────────────┬──────────────────┬─────────┘
                │                  │
┌───────────────▼──────────┐ ┌─────▼────────────────┐
│ Windows File System      │ │ Windows Shell / APIs │
│ User Desktop / Public    │ │ Tray / Explorer View │
└──────────────────────────┘ └──────────────────────┘
```

### 7.1 模块职责

- `DesktopScanner`：定位并扫描桌面目录；不修改文件。
- `ShellService`：打开项目、解析快捷方式、获取图标。
- `DesktopVisibilityService`：隐藏/恢复桌面图标视图；所有系统窗口操作集中于此。
- `StartupService`：管理 AndDesk 当前用户登录启动项。
- `TrayService`：创建通知区域图标和上下文菜单。
- `SettingsService`：读取和保存用户设置。
- `RecoveryService`：恢复模式与启动时状态检查。
- `Logger`：记录错误、操作结果与诊断信息，避免记录敏感文件内容。

---

## 8. 项目目录结构

```text
AndDesk/
├── AndDesk.sln
├── README.md
├── docs/
│   ├── DeveloperGuide.md
│   ├── Architecture.md
│   ├── TestPlan.md
│   └── ReleaseChecklist.md
├── src/
│   └── AndDesk/
│       ├── AndDesk.csproj
│       ├── App.xaml
│       ├── App.xaml.cs
│       ├── Models/
│       │   ├── DesktopItem.cs
│       │   └── AppSettings.cs
│       ├── Views/
│       │   ├── MainWindow.xaml
│       │   └── SettingsWindow.xaml
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   └── SettingsViewModel.cs
│       ├── Services/
│       │   ├── DesktopScanner.cs
│       │   ├── ShellService.cs
│       │   ├── DesktopVisibilityService.cs
│       │   ├── StartupService.cs
│       │   ├── TrayService.cs
│       │   ├── SettingsService.cs
│       │   └── RecoveryService.cs
│       └── Infrastructure/
│           ├── RelayCommand.cs
│           └── SingleInstance.cs
├── tests/
│   └── AndDesk.Tests/
└── installer/
    └── README.md
```

---

## 9. 数据模型

### 9.1 DesktopItem

建议字段：
- `Name`：用于显示的名称。
- `FullPath`：原始文件路径。
- `IsDirectory`：是否文件夹。
- `ItemKind`：File、Directory、Shortcut、Unknown。
- `Icon`：缓存的图标资源。
- `Exists`：最近一次检查时是否仍然存在。
- `SourceFolder`：个人桌面或公共桌面来源。

不要将快捷方式的显示路径与解析后的目标路径混为一谈。用户点击快捷方式时，应按 Shell 的快捷方式语义打开原始 `.lnk`，而不是总是直接启动其解析目标，因为快捷方式可能包含启动参数、工作目录或特殊 Shell 目标。

### 9.2 AppSettings

```json
{
  "schemaVersion": 1,
  "launchAtLogin": false,
  "startMinimized": true,
  "hideDesktopIconsOnLaunch": false,
  "restoreIconsOnExit": true,
  "viewMode": "grid",
  "sortMode": "name"
}
```

以上为建议默认值。尤其是 `launchAtLogin` 与 `hideDesktopIconsOnLaunch` 应由用户明确选择，不应在首次运行时无提示地强制开启。

### 9.3 配置存储
- 路径：`%LocalAppData%\\AndDesk\\settings.json`
- 配置目录缺失时自动创建。
- 解析失败时保留损坏文件备份并恢复默认配置。
- 写入时先写临时文件，再替换目标文件。
- 通过 `schemaVersion` 处理未来配置升级。

---

## 10. 核心流程

### 10.1 应用启动流程
1. 检查命令行参数。
2. 若参数为 `--restore`，执行恢复桌面图标并退出。
3. 检查单实例；若已有实例，将已有窗口唤起后退出。
4. 读取设置。
5. 初始化日志和托盘图标。
6. 根据 `startMinimized` 决定显示主窗口还是驻留托盘。
7. 根据 `hideDesktopIconsOnLaunch` 决定是否请求隐藏桌面图标。
8. 扫描桌面项目。
9. 将扫描结果交给 UI 线程更新列表。

### 10.2 退出流程
1. 保存设置。
2. 尝试恢复桌面图标（取决于用户设置和本应用实际控制状态）。
3. 取消文件系统监听器。
4. 释放托盘图标、图标句柄和其他非托管资源。
5. 写入退出日志。
6. 正常关闭应用。

不能只依赖退出事件恢复桌面：任务管理器强制结束、系统崩溃、断电等情况不会保证执行正常退出流程。必须提供 `--restore` 恢复入口，并在启动时对状态进行谨慎核对。

### 10.3 扫描流程
1. 使用 Known Folder API 或 `Environment.GetFolderPath` 定位个人桌面与公共桌面。
2. 去重目录路径。
3. 枚举目录中的文件系统项目。
4. 单个项目异常时跳过并记录。
5. 对快捷方式保留原始路径。
6. 获取名称、类型、图标和来源。
7. 通过名称或路径排序。
8. 更新界面。

### 10.4 打开项目流程
1. 检查项目是否仍存在。
2. 使用 `ProcessStartInfo` 和 `UseShellExecute = true` 请求 Windows Shell 打开项目。
3. 捕获 `Win32Exception`、`IOException` 等错误。
4. 显示可操作的提示。
5. 不因单个项目打开失败而终止应用。

---

## 11. 桌面图标隐藏与恢复规范

### 11.1 原则
- 只控制 Explorer 桌面图标视图的可见性，不操作用户文件。
- 每次操作后检查结果，并在 UI 显示成功、失败或状态未知。
- 如果找不到预期的 Shell 视图，返回失败，不得谎报成功。
- 不要无条件在每次启动时先显示、再隐藏，否则可能造成桌面闪烁并覆盖用户手动选择。
- 只有在应用确实请求过隐藏、且设置明确允许时，才尝试恢复本应用管理的状态。
- 当用户手动改变状态时，应尽量尊重用户操作。

### 11.2 已知风险
- Explorer 重启后窗口句柄可能变化。
- Windows 更新可能改变窗口层级。
- 多显示器、虚拟桌面、第三方桌面增强工具可能影响行为。
- `ShowWindow` 的返回值并不直接代表最终可见状态；应重新查询状态并验证。
- 隐藏图标不是安全功能，文件仍可通过文件资源管理器访问。

### 11.3 恢复入口
至少实现：
1. 主窗口按钮：恢复桌面图标。
2. 托盘菜单：恢复桌面图标。
3. 命令行：`AndDesk.exe --restore`。
4. 文档说明：如恢复入口失效，用户可在 Windows 桌面右键菜单中重新启用“显示桌面图标”；必要时重启 Windows Explorer 或重新登录。

### 11.4 恢复程序建议
正式发布版可附带独立的 `AndDeskRecovery.exe`，它只尝试恢复桌面图标并退出。这样即使主程序无法正常启动，仍能提供恢复路径。恢复工具也必须验证目标 Shell 窗口存在，并报告结果。

---

## 12. 开机自启动规范

### 12.1 推荐实现
- 使用当前用户的 Windows 登录启动机制。
- 不要求管理员权限。
- 启动项名称固定为 `AndDesk`。
- 启动命令使用带引号的完整可执行文件路径。
- 支持用户设置中的启用/禁用。
- 卸载时只删除 AndDesk 自己创建的启动项。
- 程序更新后检查启动项是否仍指向有效路径。

### 12.2 注册表 Run 项注意事项
当前用户 Run 项通常位于：
`HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run`

只应写入 `AndDesk` 自己的值。不要更改系统级启动项，也不要修改其他应用的值。发布前要验证安装路径包含空格、路径更新、用户禁用和卸载后的行为。

---

## 13. 日志与错误处理

### 13.1 日志位置
`%LocalAppData%\\AndDesk\\Logs\\`

### 13.2 应记录
- 应用启动、退出和版本号。
- 扫描开始/完成及项目数量。
- 隐藏/恢复请求是否成功。
- 自启动配置失败。
- 项目打开失败的错误类型。
- 恢复模式执行结果。

### 13.3 不应记录
- 文件内容。
- 用户文档内容。
- 不必要的完整路径日志（如非诊断必需）。
- 账号、令牌或其他凭据。

### 13.4 错误分类
- `Recoverable`：单个项目读取失败，可继续。
- `Configuration`：设置损坏，备份后使用默认值。
- `ShellInterop`：桌面视图找不到或 API 调用失败。
- `StartupRegistration`：启动项无法更新。
- `Fatal`：应用初始化失败，需要错误提示和恢复建议。

---

## 14. 安全与隐私设计

1. MVP 不需要管理员权限。
2. 不移动、删除、覆盖或上传桌面项目。
3. 不自动修改 Windows 以外的启动项。
4. 所有用户可见的系统级行为必须由用户设置控制。
5. 退出时恢复图标是尽力而为机制，不得宣称绝对保证。
6. 如果未来添加云同步、遥测或账户功能，必须重新进行隐私评估。
7. 安装程序应有清晰的发布者信息、卸载入口和版本信息。
8. 对命令行参数进行白名单解析，只支持文档化参数。

---

## 15. 测试计划

### 15.1 单元测试
- 桌面目录路径为空或不存在时扫描不崩溃。
- 重复路径去重正确。
- 单个项目访问失败不会中止整体扫描。
- 搜索不区分大小写。
- 设置文件损坏后回退到默认设置。
- 命令行 `--restore` 路由正确。

### 15.2 Windows 集成测试
- Windows 11 x64 普通用户启动。
- 当前用户桌面与公共桌面均有项目。
- 桌面位于 OneDrive 重定向目录。
- 文件名含中文、空格、括号和 Unicode 字符。
- 快捷方式目标不存在。
- Explorer 重启后恢复入口仍可用。
- 双显示器与不同 DPI 缩放。
- 从托盘退出时恢复图标。
- 通过任务管理器结束进程后执行恢复命令。
- 用户禁用启动项后再次登录。
- 安装、升级、卸载后没有残留无效启动项。

### 15.3 验收标准
- 不发生任何用户文件移动或删除。
- 单个文件读取失败不会使应用崩溃。
- 托盘菜单能打开主窗口和恢复桌面图标。
- `--restore` 可独立执行恢复路径。
- 自启动行为符合设置，且可被用户关闭。
- 发布包可在干净的 Windows 11 测试机上运行。

---

## 16. 构建与运行

### 16.1 环境要求
- Windows 11。
- Visual Studio 2022（安装 .NET 桌面开发工作负载）或 .NET SDK。
- Git（可选）。
- 测试阶段使用普通用户账户。

### 16.2 创建项目

在 PowerShell 中：

```powershell
mkdir AndDesk
cd AndDesk

dotnet new sln -n AndDesk
dotnet new wpf -n AndDesk -o src/AndDesk
dotnet sln AndDesk.sln add src/AndDesk/AndDesk.csproj
```

若要使用系统托盘 `NotifyIcon`，在项目文件中启用 Windows Forms 支持：

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net10.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
  <UseWindowsForms>true</UseWindowsForms>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

请根据开发时官方支持的 .NET LTS 版本调整 `TargetFramework`；如果环境尚未安装 .NET 10 SDK，可以选择当前受支持的 Windows 目标框架。

### 16.3 构建与运行

```powershell
dotnet restore
dotnet build -c Release
dotnet run --project .\src\AndDesk\AndDesk.csproj
```

### 16.4 发布为独立 EXE

```powershell
dotnet publish .\src\AndDesk\AndDesk.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true
```

发布目录通常在：
`src\\AndDesk\\bin\\Release\\net10.0-windows\\win-x64\\publish\\`

发布前需验证具体框架版本与输出路径。

---

## 17. 安装、升级与卸载

### 17.1 安装
- 安装到用户可写目录或由安装器管理的应用目录。
- 创建开始菜单快捷方式。
- 自启动默认关闭或由安装流程明确询问。
- 隐藏桌面图标默认关闭或由首次运行设置明确询问。
- 不要求管理员权限，除非采用的安装方案确有必要。

### 17.2 升级
- 保留用户设置。
- 关闭旧进程后升级。
- 更新启动项路径。
- 不删除用户桌面项目。
- 升级失败时保留可运行的旧版本或给出恢复指引。

### 17.3 卸载
- 关闭 AndDesk 进程。
- 尝试恢复桌面图标。
- 删除 AndDesk 自己的启动项。
- 删除程序文件和快捷方式。
- 默认保留用户设置和日志，或在卸载流程中明确询问是否删除。
- 不触碰用户桌面文件。

---

## 18. 发布检查清单

- [ ] 在干净 Windows 11 x64 环境构建成功。
- [ ] 桌面扫描不修改文件。
- [ ] 托盘图标和菜单正常。
- [ ] 隐藏和恢复功能经过目标系统版本测试。
- [ ] `--restore` 命令行恢复已验证。
- [ ] Explorer 重启后的恢复已验证。
- [ ] 自启动可启用、禁用和清理。
- [ ] 设置损坏时能恢复默认值。
- [ ] 安装、升级、卸载流程已测试。
- [ ] 发布说明列出已知限制。
- [ ] 安装包具有清晰的应用名称、版本和发布者信息。
- [ ] 如公开分发，考虑代码签名和安全软件误报排查。

---

## 19. 版本路线图

### v0.1 — 技术验证
- WPF 主窗口。
- 扫描个人桌面和公共桌面。
- 显示名称、类型、路径。
- 双击打开。
- 托盘图标。
- 手动隐藏/恢复。
- `--restore` 恢复入口。

### v0.2 — MVP
- 设置文件。
- 自启动开关。
- 启动时最小化。
- 更可靠的图标提取。
- 搜索和排序。
- 日志和单实例。

### v0.3 — 稳定性
- 文件变化监听和防抖。
- Explorer 重启检测。
- 独立恢复工具。
- 自动化测试。
- 设置迁移。

### v1.0 — 正式发布
- 安装包、升级和卸载。
- 用户帮助与诊断页面。
- 完整 Windows 11 集成测试。
- 代码签名（如适用）。
- 发布与回滚流程。

---

## 20. 官方参考资料

- WPF 桌面开发：<https://learn.microsoft.com/en-us/dotnet/desktop/wpf/>
- Windows Shell：<https://learn.microsoft.com/en-us/windows/win32/shell/>
- Windows Known Folders：<https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid>
- Windows 启动应用管理：<https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/desktop-to-uwp-extensions>
- Windows 桌面图标设置：<https://support.microsoft.com/en-us/windows/customize-the-desktop-icons-in-windows>

---

## 21. 交付说明

本文件是 AndDesk 的完整产品与工程开发规范。它定义了功能边界、模块职责、系统风险、恢复策略、测试标准和发布流程。实际代码应以这些规范为基线逐步实现；尤其桌面图标隐藏部分必须经过 Windows 11 集成测试，不应只凭编译成功就视为可靠。
