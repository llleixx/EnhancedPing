using BepInEx.Configuration;
using UnityEngine;

namespace EnhancedPing;

internal sealed class ModConfig
{
    private const int DefaultMaximumPingPoints = 10;
    private const float DefaultMinimumCaptureAngle = 0.5f;
    private const float DefaultMinimumPathAngle = 1.5f;
    private const float DefaultPreferredPointDuration = 0.2f;
    private const float DefaultMaximumSequenceDuration = 2f;
    private const int DefaultFontSize = 24;

    public ModConfig(ConfigFile config)
    {
        Enabled = config.Bind("General", "Enabled", true, "Enable EnhancedPing features.");

        DistanceEnabled = config.Bind("Distance", "Enabled", true, "Show distance on visible ping markers.");
        DistanceDecimalPlaces = config.Bind(
            "Distance", "DecimalPlaces", 0,
            new ConfigDescription("Number of decimal places used by distance labels.", new AcceptableValueRange<int>(0, 1)));
        DistanceFontSize = config.Bind(
            "Distance", "FontSize", DefaultFontSize,
            new ConfigDescription("Distance label font size.", new AcceptableValueRange<int>(10, 72)));

        GhostEnabled = config.Bind("Ghost", "Enabled", true, "Allow ping input while the local scout is dead.");
        ShowReticleWhenDead = config.Bind(
            "Ghost", "ShowReticleWhenDead", true,
            "Show PEAK's default center reticle while the local scout is dead.");

        PathEnabled = config.Bind("Path", "Enabled", true, "Hold the rebound Ping action and move the aim to draw a path.");
        MaximumPingPoints = config.Bind(
            "Path", "MaximumPingPoints", DefaultMaximumPingPoints,
            new ConfigDescription("Maximum transmitted points, including the start and end.", new AcceptableValueRange<int>(2, 20)));
        MinimumCaptureAngleDegrees = config.Bind(
            "Path", "MinimumCaptureAngleDegrees", DefaultMinimumCaptureAngle,
            new ConfigDescription("Minimum angular movement between captured aim rays.", new AcceptableValueRange<float>(0.05f, 5f)));
        MinimumPathAngleDegrees = config.Bind(
            "Path", "MinimumPathAngleDegrees", DefaultMinimumPathAngle,
            new ConfigDescription("Minimum cumulative angular movement required to classify a held Ping as a path.", new AcceptableValueRange<float>(0.1f, 30f)));
        PreferredPointDurationSeconds = config.Bind(
            "Path", "PreferredPointDurationSeconds", DefaultPreferredPointDuration,
            new ConfigDescription("Preferred lifetime of each intermediate ping before the next replaces it.", new AcceptableValueRange<float>(0.05f, 1f)));
        MaximumSequenceDurationSeconds = config.Bind(
            "Path", "MaximumSequenceDurationSeconds", DefaultMaximumSequenceDuration,
            new ConfigDescription("Maximum time from the first path ping until the endpoint appears.", new AcceptableValueRange<float>(1f, 10f)));
        ShowPathPreview = config.Bind("Path", "ShowPreview", true, "Show a local line while drawing a path.");
    }

    public ConfigEntry<bool> Enabled { get; }
    public ConfigEntry<bool> DistanceEnabled { get; }
    public ConfigEntry<int> DistanceDecimalPlaces { get; }
    public ConfigEntry<int> DistanceFontSize { get; }
    public ConfigEntry<bool> GhostEnabled { get; }
    public ConfigEntry<bool> ShowReticleWhenDead { get; }
    public ConfigEntry<bool> PathEnabled { get; }
    public ConfigEntry<int> MaximumPingPoints { get; }
    public ConfigEntry<float> MinimumCaptureAngleDegrees { get; }
    public ConfigEntry<float> MinimumPathAngleDegrees { get; }
    public ConfigEntry<float> PreferredPointDurationSeconds { get; }
    public ConfigEntry<float> MaximumSequenceDurationSeconds { get; }
    public ConfigEntry<bool> ShowPathPreview { get; }

    public int SafeDistanceDecimals => Mathf.Clamp(DistanceDecimalPlaces.Value, 0, 1);
    public int SafeDistanceFontSize => Mathf.Clamp(DistanceFontSize.Value, 10, 72);
    public int SafeMaximumPingPoints => Mathf.Clamp(MaximumPingPoints.Value, 2, 20);
    public float SafeMinimumCaptureAngle => SafeRange(MinimumCaptureAngleDegrees.Value, 0.05f, 5f, DefaultMinimumCaptureAngle);
    public float SafeMinimumPathAngle => SafeRange(MinimumPathAngleDegrees.Value, 0.1f, 30f, DefaultMinimumPathAngle);
    public float SafePreferredPointDuration => SafeRange(PreferredPointDurationSeconds.Value, 0.05f, 1f, DefaultPreferredPointDuration);
    public float SafeMaximumSequenceDuration => SafeRange(MaximumSequenceDurationSeconds.Value, 1f, 10f, DefaultMaximumSequenceDuration);

    private static float SafeRange(float value, float minimum, float maximum, float fallback)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            return fallback;
        return Mathf.Clamp(value, minimum, maximum);
    }
}
