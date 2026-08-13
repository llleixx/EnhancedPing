using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace EnhancedPing;

internal readonly struct PatchCapabilities
{
    public PatchCapabilities(bool input, bool distance, bool deadReticle)
    {
        Input = input;
        Distance = distance;
        DeadReticle = deadReticle;
    }

    public bool Input { get; }
    public bool Distance { get; }
    public bool DeadReticle { get; }
}

internal static class PatchInstaller
{
    public static PatchCapabilities Install(Harmony harmony, ManualLogSource logger)
    {
        bool input = TryPatch(
            harmony,
            AccessTools.Method(typeof(PointPinger), "Update"),
            prefix: AccessTools.Method(typeof(PatchCallbacks), nameof(PatchCallbacks.PointPingerUpdatePrefix)),
            description: "PointPinger.Update",
            logger: logger);

        bool receive = TryPatch(
            harmony,
            AccessTools.Method(typeof(PointPinger), "ReceivePoint_Rpc"),
            prefix: AccessTools.Method(typeof(PatchCallbacks), nameof(PatchCallbacks.ReceivePointPrefix)),
            postfix: AccessTools.Method(typeof(PatchCallbacks), nameof(PatchCallbacks.ReceivePointPostfix)),
            description: "PointPinger.ReceivePoint_Rpc",
            logger: logger);

        TryPatch(
            harmony,
            AccessTools.Method(typeof(GUIManager), "Start"),
            postfix: AccessTools.Method(typeof(PatchCallbacks), nameof(PatchCallbacks.GUIManagerStartPostfix)),
            description: "GUIManager.Start",
            logger: logger);

        TryPatch(
            harmony,
            AccessTools.Method(typeof(GUIManager), "OnDestroy"),
            prefix: AccessTools.Method(typeof(PatchCallbacks), nameof(PatchCallbacks.GUIManagerOnDestroyPrefix)),
            description: "GUIManager.OnDestroy",
            logger: logger);

        bool deadReticle = TryPatch(
            harmony,
            AccessTools.Method(typeof(GUIManager), "LateUpdate"),
            postfix: AccessTools.Method(typeof(PatchCallbacks), nameof(PatchCallbacks.GUIManagerLateUpdatePostfix)),
            description: "GUIManager.LateUpdate",
            logger: logger);

        return new PatchCapabilities(input, receive, deadReticle);
    }

    private static bool TryPatch(
        Harmony harmony,
        MethodInfo? original,
        MethodInfo? prefix = null,
        MethodInfo? postfix = null,
        string? description = null,
        ManualLogSource? logger = null)
    {
        if (original == null || (prefix == null && postfix == null))
        {
            logger?.LogError($"Required patch target is missing: {description}.");
            return false;
        }

        try
        {
            harmony.Patch(
                original,
                prefix == null ? null : new HarmonyMethod(prefix) { priority = Priority.First },
                postfix == null ? null : new HarmonyMethod(postfix) { priority = Priority.Last });
            return true;
        }
        catch (Exception exception)
        {
            logger?.LogError($"Failed to patch {description}: {exception}");
            return false;
        }
    }
}

internal static class PatchCallbacks
{
    public static bool PointPingerUpdatePrefix(PointPinger __instance, ref float ____timeLastPinged)
    {
        EnhancedPingController? controller = Plugin.Instance?.Controller;
        return controller == null || controller.BeforePointPingerUpdate(__instance, ref ____timeLastPinged);
    }

    public static void ReceivePointPrefix(
        ref GameObject ___pingInstance,
        out GameObject __state)
    {
        __state = ___pingInstance;
    }

    public static void ReceivePointPostfix(
        PointPinger __instance,
        GameObject __state,
        ref GameObject ___pingInstance)
    {
        Plugin.Instance?.Controller.ShowDistanceLabel(__instance, __state, ___pingInstance);
    }

    public static void GUIManagerStartPostfix(GUIManager __instance)
    {
        Plugin.Instance?.Controller.InitializeDistanceOverlay(__instance);
    }

    public static void GUIManagerOnDestroyPrefix(GUIManager __instance)
    {
        Plugin.Instance?.Controller.DestroyDistanceOverlay();
    }

    public static void GUIManagerLateUpdatePostfix(GUIManager __instance)
    {
        Plugin.Instance?.Controller.UpdateDeadReticle(__instance);
    }
}
