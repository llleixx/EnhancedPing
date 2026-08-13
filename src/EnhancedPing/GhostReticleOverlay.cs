using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EnhancedPing;

internal sealed class GhostReticleOverlay
{
    private Canvas? _canvas;
    private Image? _image;
    private GUIManager? _gui;

    public void Update(GUIManager gui, bool visible)
    {
        if (!visible)
        {
            if (_canvas != null && _canvas.gameObject.activeSelf)
                _canvas.gameObject.SetActive(false);
            return;
        }

        if (_canvas == null || _image == null || _gui != gui)
            Initialize(gui);

        if (_canvas != null && !_canvas.gameObject.activeSelf)
            _canvas.gameObject.SetActive(true);
    }

    public void Dispose()
    {
        if (_canvas != null)
            Object.Destroy(_canvas.gameObject);
        _canvas = null;
        _image = null;
        _gui = null;
    }

    private void Initialize(GUIManager gui)
    {
        Dispose();
        Image source = gui.reticleDefaultImage;
        if (source == null || source.sprite == null)
            return;

        GameObject canvasObject = new(
            "Canvas_EnhancedPingGhostReticle",
            typeof(Canvas),
            typeof(CanvasScaler));
        canvasObject.transform.SetParent(gui.transform, false);
        canvasObject.layer = LayerMask.NameToLayer("UI");

        _canvas = canvasObject.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 101;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        GameObject imageObject = new(
            "EnhancedPingGhostReticle",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.transform.SetParent(_canvas.transform, false);

        RectTransform sourceRect = source.rectTransform;
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = sourceRect.pivot;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = sourceRect.rect.size;
        rect.localScale = sourceRect.localScale;

        _image = imageObject.GetComponent<Image>();
        _image.sprite = source.sprite;
        _image.material = source.material;
        _image.color = source.color;
        _image.type = source.type;
        _image.preserveAspect = source.preserveAspect;
        _image.raycastTarget = false;
        _gui = gui;
    }
}
