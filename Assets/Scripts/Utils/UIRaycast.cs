using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRaycast
{
    readonly List<Graphic> _raycastTargets = new(32);
    readonly Canvas _canvas;

    public UIRaycast(Canvas canvas)
    {
        _canvas = canvas;
        RebuildCache();
    }

    public void RebuildCache()
    {
        _raycastTargets.Clear();

        var graphics = GraphicRegistry.GetGraphicsForCanvas(_canvas);

        for (int i = 0; i < graphics.Count; i++)
        {
            var g = graphics[i];

            if (!g.raycastTarget)
                continue;

            if (!g.gameObject.activeInHierarchy)
                continue;

            _raycastTargets.Add(g);
        }
    }

    public bool PointerOverUI(Vector2 screenPos)
    {
        for (int i = 0; i < _raycastTargets.Count; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                     _raycastTargets[i].rectTransform,
                    screenPos,
                    Camera.main))
            {
                return true; // EARLY OUT
            }
        }

        return false;
    }

}
