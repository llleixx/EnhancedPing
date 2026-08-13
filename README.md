# EnhancedPing

[English](#english) | [中文](#中文)

## English

EnhancedPing is a client-side BepInEx mod for PEAK that adds distance labels, dead-scout ping input, and path drawing while preserving PEAK's vanilla ping RPC. Other players do not need the mod to see the path sequence.

Baseline: PEAK `2.0.a` (Steam build `24676019`), BepInEx 5, .NET Standard 2.1.

The Thunderstore archive contains one production binary at `plugins/EnhancedPing.dll`. For manual installation, place that DLL directly in PEAK's `BepInEx/plugins` directory.

### Features and controls

- Every visible ping shows its distance from the current camera. While spectating, this is the distance from the ghost/spectator view.
- Dead scouts can use the normal rebound Ping action from the current spectator camera. Ghost pings borrow the currently observed living scout's vanilla identity and color so unmodded peers see them reliably.
- Dead scouts can optionally keep PEAK's default center reticle visible.
- Hold the current rebound Ping action, move the aim to draw, then release Ping to play the path from its start to its endpoint.
- A local line previews the captured terrain path while drawing.

EnhancedPing decides on release whether the gesture was a normal ping or a path. Cumulative aim movement below `MinimumPathAngleDegrees` sends one normal ping at the release position. A larger gesture plays the captured route. Normal pings therefore trigger on release instead of press while the path feature is enabled; disabling `Path.Enabled` restores the untouched vanilla press behavior.

Path capture is based on aim angle rather than terrain distance. Each valid terrain hit stores its normalized camera-ray direction. New samples are accepted after the aim moves by the configured angular threshold, and the final points are distributed along cumulative spherical angle. Near and distant strokes therefore have consistent on-screen density, while every transmitted point remains a real terrain raycast hit.

Path points use PEAK's existing `ReceivePoint_Rpc` one after another. A vanilla client sees the marker travel along the route because each new ping replaces the previous ping. No custom network event, host authority, or required peer installation is involved.

The effective intermediate duration is:

```text
min(PreferredPointDurationSeconds, MaximumSequenceDurationSeconds / selected point count)
```

The configured maximum covers the path animation from its first point until the endpoint appears. The endpoint then remains for PEAK's normal ping lifetime, because a sender cannot remotely clear it on an unmodded client.

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

Only the player operating EnhancedPing needs to install it. Vanilla peers see both live and ghost path sequences through the original RPC.

PEAK `2.0.a` added explicit `PointPinger` eligibility and send helpers. EnhancedPing leaves the native live-scout flow intact except while a path gesture is being drawn. Dead-scout input is handled separately through the observed scout.

Ghost pings are sent exclusively through the currently observed living scout's `PointPinger`. Vanilla peers therefore use that scout's color and apply the normal visibility check from that scout's position. A ghost ping replaces the observed scout's existing marker, and their next real ping replaces the ghost marker. If there is no valid living observed scout, EnhancedPing does not send a ghost ping; it does not fall back to the dead scout's identity.

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

EnhancedPing 是 PEAK 的客户端 BepInEx Mod，提供 Ping 距离、死亡后的 Ping 输入和路径绘制，并继续使用 PEAK 原版 Ping RPC。其他玩家不安装 Mod 也能看到依次播放的路径。

开发基线：PEAK `2.0.a`（Steam build `24676019`）、BepInEx 5、.NET Standard 2.1。

Thunderstore 压缩包只包含一个生产 DLL，路径为 `plugins/EnhancedPing.dll`。手动安装时，将该 DLL 直接放入 PEAK 的 `BepInEx/plugins` 目录。

### 功能与操作

- 所有可见 Ping 都显示相对当前相机的距离；观战时即为相对幽灵/观战视角的距离。
- 死亡后可从当前观战相机使用游戏中已绑定的 Ping 操作。幽灵 Ping 借用当前被观察存活玩家的原版身份和颜色，使未安装 Mod 的队友也能可靠看到。
- 可选择在死亡和观战期间继续显示 PEAK 原版屏幕中心准心。
- 按住当前绑定的 Ping 键，移动视角绘制路径，松开 Ping 后从起点向终点依次播放。
- 绘制期间会显示仅本地可见的地形路径预览线。

EnhancedPing 会在松开时判断这是普通 Ping 还是路径。累计瞄准角度不足 `Path.MinimumPathAngleDegrees` 时，只在松开位置发送一个普通 Ping；超过阈值才播放路径。启用路径功能后普通 Ping 因而改为松开触发，关闭 `Path.Enabled` 后恢复完全原版的按下触发行为。

采样依据瞄准射线之间的球面夹角，而不是地面世界距离。近处和远处的绘制因而具有一致的屏幕视觉密度；最终发送的坐标始终来自真实 TerrainMap 射线命中，不会通过坐标插值产生空中点。

路径通过原版 `ReceivePoint_Rpc` 逐点发送。未安装 Mod 的客户端会看到原版标记沿路径移动，因为新点会替换前一个点。此功能不需要自定义网络消息、房主权限或队友安装。

中间点的实际持续时间为：

```text
min(首选单点持续时间, 路径播放上限 / 最终点数)
```

播放上限只约束首点到终点出现的阶段。终点出现后仍按 PEAK 原版时长保留，因为发送端无法要求未安装 Mod 的客户端提前清除它。

配置文件生成于 `BepInEx/config/com.github.lllei.EnhancedPing.cfg`，各配置键、默认值和范围见英文表格。`Path.MaximumPingPoints` 包含起点和终点，默认最多 10 个点。非法数值会在运行时钳制或回退。

### 兼容性说明

只有操作者需要安装 EnhancedPing；未安装 Mod 的客户端也能通过原版 RPC 看到存活和幽灵路径播放。

PEAK `2.0.a` 新增了明确的 `PointPinger` 可用性判断和发送辅助方法。EnhancedPing 除绘制路径手势期间外，不改变存活玩家的原版 Ping 流程；死亡玩家的输入则单独通过当前被观察玩家处理。

幽灵 Ping 只通过当前被观察存活玩家的 `PointPinger` 发送。未安装 Mod 的客户端因此使用该玩家的颜色，并从该玩家位置执行原版可见性判断。幽灵 Ping 会替换该玩家已有的标记，该玩家下一次真实 Ping 也会替换幽灵标记。如果当前没有有效的被观察存活玩家，EnhancedPing 不会发送幽灵 Ping，也不会回退到死者自己的身份。

EnhancedPing 已覆盖 GhostPing 和 BetterPingDistance 的用途。建议禁用这两个模组，避免 Harmony 补丁重叠或重复距离文本。发布时同时更新 `EnhancedPing.csproj` 的 `Version` 和 `manifest.json` 的 `version_number`；MSBuild 会据此生成 BepInEx 插件版本，打包会校验 manifest 与 DLL 版本是否一致。构建、部署和打包命令见英文构建章节。
