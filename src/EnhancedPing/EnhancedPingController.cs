using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using EnhancedPing.Core;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EnhancedPing;

internal sealed class EnhancedPingController
{
    private readonly Plugin _plugin;
    private readonly ModConfig _config;
    private readonly ManualLogSource _logger;
    private readonly PathPreview _preview = new();
    private readonly PingDistanceOverlay _distanceOverlay;

    private PatchCapabilities _capabilities;
    private PathDrawingSession? _drawing;
    private PointPinger? _drawingPinger;
    private PointPinger? _drawingCarrier;
    private Coroutine? _sequence;
    private PointPinger? _sequencePinger;

    public EnhancedPingController(Plugin plugin, ModConfig config, ManualLogSource logger)
    {
        _plugin = plugin;
        _config = config;
        _logger = logger;
        _distanceOverlay = new PingDistanceOverlay(config, logger);
    }

    public void SetCapabilities(PatchCapabilities capabilities)
    {
        _capabilities = capabilities;
    }

    public void TickLifecycle()
    {
        if (!_config.Enabled.Value)
        {
            Reset("feature disabled");
            return;
        }

        if (_drawing != null && (_drawingPinger == null || _drawingCarrier == null ||
            Character.localCharacter == null || !InputAllowed()))
            CancelDrawing("input or character lifecycle ended");

        if (_sequence != null && (_sequencePinger == null || Character.localCharacter == null))
            CancelSequence();
    }

    public bool BeforePointPingerUpdate(PointPinger pinger, ref float timeLastPinged)
    {
        if (!_capabilities.Input || !_config.Enabled.Value || !IsLocalPinger(pinger))
            return true;

        InputAction pingAction = CharacterInput.action_ping;
        if (pingAction == null)
            return true;

        if (_drawing != null)
            return ContinueDrawing(pinger, pingAction, ref timeLastPinged);

        if (!_config.PathEnabled.Value)
        {
            if (!_config.GhostEnabled.Value || pinger.character?.data == null || !pinger.character.data.dead)
                return true;

            if (pingAction.WasPressedThisFrame() && CanStartEnhancedPing(pinger, timeLastPinged) &&
                InputAllowed() && TryResolvePingCarrier(pinger, out PointPinger carrier) &&
                TryGetPingSample(out RuntimePingSample sample))
            {
                timeLastPinged = Time.time;
                SendVanillaPing(carrier, sample);
            }
            return false;
        }

        if (!pingAction.WasPressedThisFrame())
            return true;

        CancelSequence();

        if (CanStartEnhancedPing(pinger, timeLastPinged) && InputAllowed() &&
            TryResolvePingCarrier(pinger, out PointPinger drawingCarrier))
        {
            BeginDrawing(pinger, drawingCarrier);
        }
        return false;
    }

    public void ShowDistanceLabel(PointPinger pinger, GameObject previousMarker, GameObject currentMarker)
    {
        if (!_capabilities.Distance || pinger.character == null || currentMarker == null ||
            ReferenceEquals(previousMarker, currentMarker))
            return;
        _distanceOverlay.Show(pinger.character, currentMarker);
    }

    public void InitializeDistanceOverlay(GUIManager gui)
    {
        if (_config.Enabled.Value && _config.DistanceEnabled.Value)
            _distanceOverlay.Initialize(gui);
    }

    public void DestroyDistanceOverlay()
    {
        _distanceOverlay.Dispose();
    }

    public void UpdateDeadReticle(GUIManager gui)
    {
        Character localCharacter = Character.localCharacter;
        if (!_capabilities.DeadReticle || localCharacter == null || localCharacter.data == null ||
            !localCharacter.data.dead || gui.reticleDefault == null)
        {
            return;
        }

        bool visible = _config.Enabled.Value && _config.ShowReticleWhenDead.Value &&
                       Time.timeScale > 0f && !GUIManager.InPauseMenu &&
                       !gui.windowBlockingInput && !gui.wheelActive;
        if (gui.reticleDefault.activeSelf != visible)
            gui.reticleDefault.SetActive(visible);
    }

    public void Reset(string reason)
    {
        bool hadState = _drawing != null || _sequence != null;
        CancelDrawing(reason);
        CancelSequence();
        _distanceOverlay.Dispose();
        if (hadState)
            _logger.LogDebug($"Enhanced ping state reset: {reason}.");
    }

    private bool ContinueDrawing(PointPinger pinger, InputAction pingAction, ref float timeLastPinged)
    {
        PointPinger? carrier = _drawingCarrier;
        if (pinger != _drawingPinger || carrier == null || !IsLivingCarrier(carrier) ||
            !_config.PathEnabled.Value || !InputAllowed())
        {
            CancelDrawing("path drawing cancelled");
            return false;
        }

        CaptureDrawingSample();
        if (pingAction.IsPressed() && !pingAction.WasReleasedThisFrame())
            return false;

        PathDrawingResult result = _drawing!.Finish(_config.SafeMaximumPingPoints);
        CancelDrawing("path drawing completed");
        if (result.Samples.Count == 0)
            return false;

        timeLastPinged = Time.time;
        if (result.TotalAngleDegrees < _config.SafeMinimumPathAngle || result.Samples.Count < 2)
        {
            SendVanillaPing(carrier, result.Samples[result.Samples.Count - 1]);
            return false;
        }

        _sequencePinger = carrier;
        _sequence = _plugin.StartCoroutine(SendSequence(carrier, result.Samples));
        return false;
    }

    private void BeginDrawing(PointPinger pinger, PointPinger carrier)
    {
        CancelDrawing("new path started");
        _drawing = new PathDrawingSession();
        _drawingPinger = pinger;
        _drawingCarrier = carrier;
        if (_config.ShowPathPreview.Value)
            _preview.Initialize(GetPlayerColor(carrier.character));
        CaptureDrawingSample();
    }

    private void CaptureDrawingSample()
    {
        if (_drawing == null || !TryGetPingSample(out RuntimePingSample sample))
            return;
        _drawing.Capture(sample, _config.SafeMinimumCaptureAngle);
        if (_config.ShowPathPreview.Value)
            _preview.Update(_drawing.PreviewSamples);
    }

    private IEnumerator SendSequence(
        PointPinger pinger,
        IReadOnlyList<RuntimePingSample> samples)
    {
        float duration = SequenceTiming.EffectivePointDuration(
            samples.Count,
            _config.SafePreferredPointDuration,
            _config.SafeMaximumSequenceDuration);
        WaitForSeconds wait = new(duration);

        for (int i = 0; i < samples.Count; i++)
        {
            if (pinger == null || !IsLivingCarrier(pinger) || !_config.Enabled.Value)
                break;
            SendVanillaPing(pinger, samples[i]);
            if (i < samples.Count - 1)
                yield return wait;
        }

        _sequence = null;
        _sequencePinger = null;
    }

    private void SendVanillaPing(PointPinger pinger, RuntimePingSample sample)
    {
        PhotonView view = pinger.GetComponent<PhotonView>();
        if (view != null)
            view.RPC("ReceivePoint_Rpc", RpcTarget.All, sample.Point, sample.Normal);
    }

    private bool CanStartEnhancedPing(PointPinger pinger, float timeLastPinged)
    {
        if (Time.time - timeLastPinged < pinger.coolDown || pinger.character?.data == null)
            return false;
        return pinger.character.data.fullyConscious ||
               (_config.GhostEnabled.Value && pinger.character.data.dead);
    }

    private static bool TryResolvePingCarrier(PointPinger source, out PointPinger carrier)
    {
        carrier = null!;
        if (source.character?.data == null)
            return false;

        if (!source.character.data.dead)
        {
            carrier = source;
            return IsLivingCarrier(carrier);
        }

        Character observed = MainCameraMovement.specCharacter;
        if (observed == null || ReferenceEquals(observed, source.character) ||
            observed.data == null || observed.data.dead)
        {
            return false;
        }

        PointPinger observedPinger = observed.GetComponent<PointPinger>();
        if (!IsLivingCarrier(observedPinger))
            return false;

        carrier = observedPinger;
        return true;
    }

    private static bool IsLivingCarrier(PointPinger pinger)
    {
        if (pinger == null || pinger.character?.data == null || pinger.character.data.dead)
            return false;
        PhotonView view = pinger.GetComponent<PhotonView>();
        return view != null && view.ViewID > 0;
    }

    private static bool IsLocalPinger(PointPinger pinger)
    {
        if (pinger.character == null || pinger.character != Character.localCharacter)
            return false;
        PhotonView view = pinger.GetComponent<PhotonView>();
        return view != null && view.IsMine;
    }

    private static bool InputAllowed()
    {
        if (Time.timeScale <= 0f || GUIManager.InPauseMenu)
            return false;
        GUIManager gui = GUIManager.instance;
        return gui == null || (!gui.windowBlockingInput && !gui.wheelActive);
    }

    private static bool TryGetPingSample(out RuntimePingSample sample)
    {
        sample = default;
        Camera camera = Camera.main;
        if (camera == null)
            return false;

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        int terrainMask = HelperFunctions.GetMask(HelperFunctions.LayerType.TerrainMap);
        if (terrainMask == 0 || !Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, terrainMask))
            return false;
        if (!IsFinite(ray.direction) || !IsFinite(hit.point) || !IsFinite(hit.normal))
            return false;

        sample = new RuntimePingSample(ray.direction.normalized, hit.point, hit.normal);
        return true;
    }

    private void CancelDrawing(string reason)
    {
        if (_drawing != null)
            _logger.LogDebug($"Path drawing ended: {reason}.");
        _drawing = null;
        _drawingPinger = null;
        _drawingCarrier = null;
        _preview.Dispose();
    }

    private void CancelSequence()
    {
        if (_sequence != null)
            _plugin.StopCoroutine(_sequence);
        _sequence = null;
        _sequencePinger = null;
    }

    private static Color GetPlayerColor(Character character)
    {
        try
        {
            return character.refs.customization.PlayerColor;
        }
        catch
        {
            return Color.white;
        }
    }

    private static bool IsFinite(Vector3 value) =>
        IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);

    private static bool IsFinite(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value);
}
