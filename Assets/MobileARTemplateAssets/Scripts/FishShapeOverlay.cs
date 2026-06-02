using TMPro;
using UnityEngine;

/// <summary>
/// Adds a shape symbol label on the fish when colour-blind mode is active.
/// Attach to the fish prefab root or a child GameObject with a TMP_Text.
/// The text is auto-populated from the fish's FishTarget.fishColor when colour-blind mode is on.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class FishShapeOverlay : MonoBehaviour
{
    TMP_Text label;
    FishTarget fishTarget;

    void Start()
    {
        label      = GetComponent<TMP_Text>();
        fishTarget = GetComponentInParent<FishTarget>();
        Refresh();
    }

    void Refresh()
    {
        if (label == null) return;

        bool show = ColourBlindSettings.IsEnabled() && fishTarget != null;
        label.enabled = show;
        if (show)
            label.text = ColourBlindSettings.ShapeForColor(fishTarget.fishColor);
    }

    // Called by ColourBlindToggleUI when the setting changes at runtime
    public void OnSettingChanged() => Refresh();
}
