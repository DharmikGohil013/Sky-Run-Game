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
        // Load Saved Settings
        bool music = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        bool sfx = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;
        bool haptics = PlayerPrefs.GetInt(HAPTICS_KEY, 1) == 1;

        // Set Toggle Values
        musicToggle.isOn = music;
        sfxToggle.isOn = sfx;
        hapticsToggle.isOn = haptics;

        // Add Listener
        musicToggle.onValueChanged.AddListener(SetMusic);
        sfxToggle.onValueChanged.AddListener(SetSFX);
        hapticsToggle.onValueChanged.AddListener(SetHaptics);
    }

    // =========================
    // MUSIC
    // =========================

    public void SetMusic(bool value)
    {
        PlayerPrefs.SetInt(MUSIC_KEY, value ? 1 : 0);
        PlayerPrefs.Save();

        AudioListener.volume = value ? 1f : 0f;

        Debug.Log("Music: " + value);
    }

    // =========================
    // SFX
    // =========================

    public void SetSFX(bool value)
    {
        PlayerPrefs.SetInt(SFX_KEY, value ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("SFX: " + value);
    }

    // =========================
    // HAPTICS
    // =========================

    public void SetHaptics(bool value)
    {
        PlayerPrefs.SetInt(HAPTICS_KEY, value ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Haptics: " + value);
    }
}