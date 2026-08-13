using BepInEx;
using HarmonyLib;

namespace EnhancedPing;

[BepInPlugin(PluginGuid, PluginName, BuildInfo.Version)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.github.lllei.EnhancedPing";
    public const string PluginName = "EnhancedPing";

    internal static Plugin? Instance { get; private set; }
    internal EnhancedPingController Controller { get; private set; } = null!;

    private Harmony? _harmony;

    private void Awake()
    {
        Instance = this;
        ModConfig settings = new(Config);
        Controller = new EnhancedPingController(this, settings, Logger);
        _harmony = new Harmony(PluginGuid);

        PatchCapabilities capabilities = PatchInstaller.Install(_harmony, Logger);
        Controller.SetCapabilities(capabilities);
        Logger.LogInfo($"{PluginName} {BuildInfo.Version} loaded for PEAK 2.0.a baseline. " +
                       $"Input: {capabilities.Input}; distance: {capabilities.Distance}; " +
                       $"dead reticle: {capabilities.DeadReticle}.");
    }

    private void Update()
    {
        Controller?.TickLifecycle();
    }

    private void OnDestroy()
    {
        Controller?.Reset("plugin destroyed");
        _harmony?.UnpatchSelf();
        Instance = null;
    }
}
