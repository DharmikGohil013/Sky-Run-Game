using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Toggles")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle hapticsToggle;

    private const string MUSIC_KEY = "MUSIC";
    private const string SFX_KEY = "SFX";
    private const string HAPTICS_KEY = "HAPTICS";

    private void Start()
    {
        musicToggle.isOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        sfxToggle.isOn = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;
        hapticsToggle.isOn = PlayerPrefs.GetInt(HAPTICS_KEY, 1) == 1;

        musicToggle.onValueChanged.AddListener(SetMusic);
        sfxToggle.onValueChanged.AddListener(SetSFX);
        hapticsToggle.onValueChanged.AddListener(SetHaptics);
    }

    public void SetMusic(bool value)
    {
        PlayerPrefs.SetInt(MUSIC_KEY, value ? 1 : 0);
        PlayerPrefs.Save();
        AudioListener.volume = value ? 1f : 0f;
    }

    public void SetSFX(bool value)
    {
        PlayerPrefs.SetInt(SFX_KEY, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetHaptics(bool value)
    {
        PlayerPrefs.SetInt(HAPTICS_KEY, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}