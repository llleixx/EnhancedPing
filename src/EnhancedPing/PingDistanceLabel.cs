using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EnhancedPing;

internal sealed class PingDistanceOverlay
{
    private readonly ModConfig _config;
    private readonly ManualLogSource _logger;
    private readonly Dictionary<Character, GameObject> _instances = new();

    private Canvas? _canvas;
    private GameObject? _template;

    public PingDistanceOverlay(ModConfig config, ManualLogSource logger)
    {
        _config = config;
        _logger = logger;
    }

    public void Initialize(GUIManager gui)
    {
        if (_canvas != null && _template != null && _canvas.transform.parent == gui.transform)
            return;

        Dispose();
        TextMeshProUGUI source = gui.interactNameText;
        if (source == null || source.font == null || source.fontSharedMaterial == null)
        {
            _logger.LogError("Could not initialize distance labels from GUIManager.interactNameText.");
            return;
        }

        try
        {
            GameObject canvasObject = new(
                "Canvas_EnhancedPingDistance",
                typeof(Canvas),
                typeof(CanvasScaler));
            canvasObject.transform.SetParent(gui.transform, false);
            canvasObject.layer = LayerMask.NameToLayer("UI");

            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            _template = new GameObject(
                "EnhancedPingDistanceIndicator",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(PingDistanceLabel));
            _template.transform.SetParent(_canvas.transform, false);
            CreateText(_template.transform, source.font, source.fontSharedMaterial);
            _template.SetActive(false);

            _logger.LogInfo("Distance label overlay initialized from GUIManager.interactNameText.");
        }
        catch (Exception exception)
        {
            _logger.LogWarning($"Could not initialize the ping distance overlay: {exception}");
            Dispose();
        }
    }

    public void Show(Character character, GameObject marker)
    {
        if (!_config.Enabled.Value || !_config.DistanceEnabled.Value || character == null || marker == null)
            return;

        if (_canvas == null || _template == null)
        {
            GUIManager gui = GUIManager.instance;
            if (gui != null)
                Initialize(gui);
        }
        if (_canvas == null || _template == null)
            return;

        if (_instances.TryGetValue(character, out GameObject oldInstance) && oldInstance != null)
            Object.Destroy(oldInstance);

        try
        {
            PointPing pointPing = marker.GetComponent<PointPing>();
            Vector3 normal = pointPing != null && pointPing.hitNormal.sqrMagnitude > 1e-8f
                ? pointPing.hitNormal.normalized
                : Vector3.up;

            GameObject instance = Object.Instantiate(_template, _canvas.transform);
            PingDistanceLabel label = instance.GetComponent<PingDistanceLabel>();
            label.Initialize(marker, normal, GetPlayerColor(character), _config);
            instance.SetActive(true);
            _instances[character] = instance;
        }
        catch (Exception exception)
        {
            _logger.LogWarning($"Could not create a ping distance label: {exception}");
        }
    }

    public void Dispose()
    {
        if (_canvas != null)
            Object.Destroy(_canvas.gameObject);
        _canvas = null;
        _template = null;
        _instances.Clear();
    }

    private static void CreateText(Transform parent, TMP_FontAsset font, Material material)
    {
        GameObject textObject = new(
            "DistanceText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI),
            typeof(Shadow));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(160f, 50f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontMaterial = material;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = false;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.outlineWidth = 0.1f;
        text.outlineColor = Color.black;

        Shadow shadow = textObject.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
        shadow.effectDistance = new Vector2(2f, -2f);
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
}

internal sealed class PingDistanceLabel : MonoBehaviour
{
    private GameObject? _marker;
    private Vector3 _normal;
    private Color _color;
    private ModConfig? _config;
    private TextMeshProUGUI? _text;

    public void Initialize(GameObject marker, Vector3 normal, Color color, ModConfig config)
    {
        _marker = marker;
        _normal = normal;
        _color = color;
        _config = config;
        _text = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void LateUpdate()
    {
        if (_marker == null)
        {
            Object.Destroy(gameObject);
            return;
        }

        Camera camera = Camera.main;
        if (_config == null || !_config.Enabled.Value || !_config.DistanceEnabled.Value ||
            camera == null || _text == null)
        {
            SetTextVisible(false);
            return;
        }

        Vector3 markerPosition = _marker.transform.position;
        Vector3 screenPoint = camera.WorldToScreenPoint(markerPosition + _normal);
        if (screenPoint.z <= 0f)
        {
            SetTextVisible(false);
            return;
        }

        float distance = Vector3.Distance(markerPosition, camera.transform.position);
        float scale = Mathf.Clamp(20f / Mathf.Max(distance, 0.01f), 0.5f, 1.5f);
        transform.position = screenPoint;
        transform.localScale = new Vector3(scale, scale, scale);

        int decimals = _config.SafeDistanceDecimals;
        _text.fontSize = _config.SafeDistanceFontSize;
        _text.color = _color;
        _text.text = distance.ToString(decimals == 0 ? "0" : "0.0", CultureInfo.InvariantCulture) + "m";
        SetTextVisible(true);
    }

    private void SetTextVisible(bool visible)
    {
        if (_text != null && _text.gameObject.activeSelf != visible)
            _text.gameObject.SetActive(visible);
    }
}
