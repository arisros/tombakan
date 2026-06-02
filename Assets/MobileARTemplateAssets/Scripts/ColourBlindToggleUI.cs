using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Toggle button for colour-blind mode.
/// Broadcasts the change to all active FishShapeOverlay instances in the scene.
/// </summary>
[RequireComponent(typeof(Button))]
public class ColourBlindToggleUI : MonoBehaviour
{
    public TMP_Text label;
    public string enabledText  = "Buta Warna: On";
    public string disabledText = "Buta Warna: Off";

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        Refresh();
    }

    void OnClick()
    {
        ColourBlindSettings.Toggle();
        Refresh();
        // Notify all active fish overlays so they update immediately
        foreach (var overlay in FindObjectsOfType<FishShapeOverlay>())
            overlay.OnSettingChanged();
    }

    void Refresh()
    {
        if (label != null)
            label.text = ColourBlindSettings.IsEnabled() ? enabledText : disabledText;
    }
}
