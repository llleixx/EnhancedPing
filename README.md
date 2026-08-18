# EnhancedPing

[English](#english) | [中文](#中文)

Recommended companion mod / 推荐搭配使用：[FreeGhost](https://thunderstore.io/c/peak/p/lllei/FreeGhost/)

![EnhancedPing gameplay demo](https://raw.githubusercontent.com/llleixx/EnhancedPing/main/docs/media/enhanced-ping-demo.gif)

## English

EnhancedPing expands PEAK's ping system with several practical features, while remaining fully client-side.

### Highlights

- **Ping distance:** Every visible ping shows its distance from your current camera, including while spectating.
- **Ghost pings:** Dead scouts can ping from the spectator view and optionally keep the center reticle visible.
- **Draw ping paths:** Hold Ping, move your aim to draw a route, then release to play it for the team.
- **No team-wide install required:** Other players can see your normal and path pings without installing EnhancedPing.

### Installation

Install with Thunderstore Mod Manager or r2modman, or place `EnhancedPing.dll` directly in PEAK's `BepInEx/plugins` directory. EnhancedPing requires BepInEx 5.

### Controls

EnhancedPing uses your current in-game Ping binding, including rebound keyboard, mouse, and controller inputs.

To draw a path, hold Ping while moving your aim across the terrain, then release. A local line previews the route as you draw. Small movements are treated as a normal ping; larger movements play the captured path from start to finish.

While path drawing is enabled, normal pings are sent when Ping is released. Disable `Path.Enabled` to restore PEAK's normal press-to-ping behavior.

### Configuration

The config file is generated at `BepInEx/config/com.github.lllei.EnhancedPing.cfg`.

| Key | Default | Allowed range |
|---|---:|---:|
| `General.Enabled` | `true` | boolean |
| `Distance.Enabled` | `true` | boolean |
| `Distance.DecimalPlaces` | `0` | `0` to `1` |
| `Distance.FontSize` | `24` | `10` to `72` |
| `Ghost.Enabled` | `true` | boolean |
| `Ghost.ShowReticleWhenDead` | `true` | boolean |
| `Path.Enabled` | `true` | boolean |
| `Path.MaximumPingPoints` | `10` | `2` to `20` |
| `Path.MinimumCaptureAngleDegrees` | `0.5` | `0.05` to `5` degrees |
| `Path.MinimumPathAngleDegrees` | `1.5` | `0.1` to `30` degrees |
| `Path.PreferredPointDurationSeconds` | `0.20` | `0.05` to `1` second |
| `Path.MaximumSequenceDurationSeconds` | `2.0` | `1` to `10` seconds |
| `Path.ShowPreview` | `true` | boolean |

Invalid numeric values are clamped or replaced with safe defaults at runtime. `MaximumPingPoints` includes the start and endpoint.

### Compatibility notes

Only the player using EnhancedPing needs to install it. Other players can see your pings and paths, including those sent while spectating.

Ghost pings use the currently observed living scout's color and follow PEAK's normal ping visibility rules. They may replace that scout's existing ping, and their next ping may replace yours. If there is no valid living scout to observe, ghost pings are unavailable.

EnhancedPing replaces the use cases of GhostPing and BetterPingDistance. Disable those mods to avoid overlapping Harmony patches or duplicate distance labels.

### Building

Copy `PeakGameDir.props.example` to the gitignored `PeakGameDir.props` and set the PEAK directory, or set `PEAK_GAME_DIR`.

```powershell
dotnet test tests\EnhancedPing.Core.Tests\EnhancedPing.Core.Tests.csproj -c Release
dotnet build EnhancedPing.sln -c Release
dotnet msbuild src\EnhancedPing\EnhancedPing.csproj -t:Deploy -p:Configuration=Debug
dotnet msbuild src\EnhancedPing\EnhancedPing.csproj -t:PackageThunderstore -p:Configuration=Release
```

Normal builds never write to the game directory. `Deploy` is explicit. Packaging writes `artifacts/lllei-EnhancedPing-<version>.zip`.
For a release, update `Version` in `EnhancedPing.csproj` and `version_number` in `manifest.json`. MSBuild generates the BepInEx plugin version from `Version`, and packaging stops if the manifest and compiled DLL versions do not agree.

## 中文

EnhancedPing 为 PEAK 的 Ping 系统加入多项实用功能，并且只需本地安装。

### 功能亮点

- **Ping 距离：** 所有可见 Ping 都会显示相对当前视角的距离，观战时同样有效。
- **幽灵 Ping：** 死亡后仍可从观战视角 Ping，并可选择保留屏幕中心准心。
- **绘制 Ping 路径：** 按住 Ping 并移动视角即可绘制路线，松开后向队友依次播放。
- **无需全队安装：** 其他玩家不安装 EnhancedPing，也能看到普通 Ping 和路径 Ping。

### 安装

可以通过 Thunderstore Mod Manager 或 r2modman 安装，也可以将 `EnhancedPing.dll` 直接放入 PEAK 的 `BepInEx/plugins` 目录。EnhancedPing 需要 BepInEx 5。

### 操作方法

EnhancedPing 使用游戏中当前绑定的 Ping 操作，重新绑定后的键盘、鼠标和手柄输入同样有效。

绘制路径时，按住 Ping 并在地形上移动视角，然后松开。绘制期间会显示仅本地可见的预览线。小幅移动会作为普通 Ping，较明显的移动则会从起点到终点播放整条路径。

启用路径绘制后，普通 Ping 会在松开按键时发出。关闭 `Path.Enabled` 即可恢复 PEAK 原本的按下触发方式。

配置文件生成于 `BepInEx/config/com.github.lllei.EnhancedPing.cfg`，各配置键、默认值和范围见英文表格。`Path.MaximumPingPoints` 包含起点和终点，默认最多 10 个点。非法数值会在运行时钳制或回退。

### 兼容性说明

只有使用者需要安装 EnhancedPing，其他玩家也能看到你的 Ping 和路径，包括观战时发出的内容。

幽灵 Ping 使用当前被观察存活玩家的颜色，并遵循 PEAK 原本的 Ping 可见性规则。它可能替换该玩家已有的 Ping，该玩家之后发出的 Ping 也可能替换你的 Ping。如果当前没有可观察的存活玩家，则无法发送幽灵 Ping。

EnhancedPing 已覆盖 GhostPing 和 BetterPingDistance 的用途。建议禁用这两个模组，避免 Harmony 补丁重叠或重复距离文本。发布时同时更新 `EnhancedPing.csproj` 的 `Version` 和 `manifest.json` 的 `version_number`；MSBuild 会据此生成 BepInEx 插件版本，打包会校验 manifest 与 DLL 版本是否一致。构建、部署和打包命令见英文构建章节。
