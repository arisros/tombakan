using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Toggle button that mutes/unmutes game audio.
/// Attach to the mute button; wire the optional label/icon in Inspector.
/// </summary>
[RequireComponent(typeof(Button))]
public class MuteButtonUI : MonoBehaviour
{
    public TMP_Text label;          // optional — shows "🔊" / "🔇"
    public string soundOnText  = "Suara: On";
    public string soundOffText = "Suara: Off";

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        Refresh();
    }

    void OnClick()
    {
        if (AudioManager.I != null)
            AudioManager.I.ToggleMute();
        Refresh();
    }

    void Refresh()
    {
        if (label == null || AudioManager.I == null) return;
        label.text = AudioManager.I.IsMuted ? soundOffText : soundOnText;
    }
}
