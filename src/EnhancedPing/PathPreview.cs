using System.Collections.Generic;
using UnityEngine;

namespace EnhancedPing;

internal sealed class PathPreview
{
    private GameObject? _gameObject;
    private LineRenderer? _line;
    private Material? _material;

    public void Initialize(Color color)
    {
        Dispose();
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            return;

        _gameObject = new GameObject("EnhancedPing.PathPreview");
        _gameObject.hideFlags = HideFlags.HideAndDontSave;
        _line = _gameObject.AddComponent<LineRenderer>();
        _material = new Material(shader);
        _line.material = _material;
        _line.useWorldSpace = true;
        _line.alignment = LineAlignment.View;
        _line.widthMultiplier = 0.045f;
        _line.numCapVertices = 4;
        _line.numCornerVertices = 2;
        color.a = 0.9f;
        _line.startColor = color;
        _line.endColor = color;
        _line.positionCount = 0;
    }

    public void Update(IReadOnlyList<RuntimePingSample> samples)
    {
        if (_line == null)
            return;

        _line.positionCount = samples.Count;
        for (int i = 0; i < samples.Count; i++)
        {
            Vector3 normal = samples[i].Normal.sqrMagnitude > 1e-8f
                ? samples[i].Normal.normalized
                : Vector3.up;
            _line.SetPosition(i, samples[i].Point + normal * 0.035f);
        }
    }

    public void Dispose()
    {
        if (_gameObject != null)
            Object.Destroy(_gameObject);
        if (_material != null)
            Object.Destroy(_material);
        _gameObject = null;
        _line = null;
        _material = null;
    }
}
