# LifeCutdown

一个原生 `WPF/.NET 8` 的 Windows 小组件，用来显示：

- 本天进度
- 本周进度
- 本月进度
- 本年进度
- 一生进度
- 自定义倒计时

它不是 Electron，而是托盘常驻 + 无边框悬浮窗的原生实现，比较适合在 Windows 11 上做接近“状态栏 / 小组件”体验的轻量工具。

## 功能

- 托盘常驻，双击托盘图标展开 / 隐藏
- 主界面可展开 / 收起系统隐藏托盘图标面板
- 托盘图标可显示所选目标的迷你进度条（支持一生 / 本年 / 本月 / 本周 / 本天 / 自定义目标）
- 可选在任务栏最右侧、托盘图标左侧显示一个小窗口：展示“名称 + 百分比”以及迷你进度条
- 实时刷新本周 / 本月 / 本年 / 一生进度
- 实时刷新本天 / 本周 / 本月 / 本年 / 一生进度
- 可设置出生日期、预期寿命、一周起始日
- 可配置一个带标题、开始日期和目标日期的自定义倒计时
- 可切换悬浮窗固定在右上角或右下角
- 可一键打开 Windows 的系统托盘设置页，管理其他应用的托盘图标显示策略

## 运行

```powershell
dotnet run --project .\LifeCutdown.App\LifeCutdown.App.csproj
```

## 发布为单文件 exe

```powershell
dotnet publish .\LifeCutdown.App\LifeCutdown.App.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true
```

发布结果会在 `LifeCutdown.App\bin\Release\net8.0-windows\win-x64\publish\`。

## 说明

Windows 11 已经不太支持旧式的任务栏 DeskBand / Toolbar 扩展，所以这里采用了更稳妥的原生方案：

- 托盘图标负责常驻和呼出
- 托盘图标本身也可以被渲染成一个微型进度条
- 可选启用任务栏右侧小窗：直接贴在托盘左侧显示当前指标，更接近 TrafficMonitor 的任务栏窗口体验
- 无边框小窗负责展示进度条
- 系统托盘图标的长期显示 / 隐藏仍交给 Windows 自身设置页管理

这样既保持了“显示在状态栏附近”的使用方式，也更接近常驻型任务栏监控工具的观感，并避免了 Electron 带来的体积和内存开销。
