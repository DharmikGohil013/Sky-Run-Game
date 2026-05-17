using UnityEngine;
using TMPro;

/// <summary>
/// Complete UI Manager
/// Endless Runner UI System
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public enum Panel
    {
        MainMenu,
        HUD,
        Pause,
        GameOver,
        Shop,
        Settings,
        Leaderboard
    }

    // =========================================
    // PANELS
    // =========================================

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject leaderboardPanel;

    // =========================================
    // MAIN MENU UI
    // =========================================

    [Header("Main Menu UI")]
    [SerializeField] private TMP_Text mainMenuCoinText;
    [SerializeField] private TMP_Text mainMenuPowerText;

    // =========================================
    // HUD UI
    // =========================================

    [Header("HUD UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text distanceText;

    [SerializeField] private TMP_Text powerText;
    [SerializeField] private TMP_Text powerTimerText;

    // =========================================
    // SHOP UI
    // =========================================

    [Header("Shop UI")]
    [SerializeField] private TMP_Text shopCoinText;
    [SerializeField] private TMP_Text shopPowerText;

    // =========================================
    // LEADERBOARD UI
    // =========================================

    [Header("Leaderboard UI")]
    [SerializeField] private TMP_Text totalDistanceText;
    [SerializeField] private TMP_Text maxDistanceText;
    [SerializeField] private TMP_Text totalEarnedCoinsText;
    [SerializeField] private TMP_Text maxEarnedCoinsText;

    // =========================================
    // PLAYER DATA
    // =========================================

    private int totalCoins;
    private int totalPowers;

    // =========================================
    // GAMEPLAY DATA
    // =========================================

    private int currentScore;
    private float currentDistance;

    // =========================================
    // POWER SYSTEM
    // =========================================

    private bool powerActive;
    private float powerTimer;

    // =========================================
    // LEADERBOARD DATA
    // =========================================

    private float totalDistance;
    private float maxDistance;

    private int totalEarnedCoins;
    private int maxEarnedCoins;

    // =========================================
    // PLAYER PREFS KEYS
    // =========================================

    private const string COIN_KEY = "TOTAL_COINS";
    private const string POWER_KEY = "TOTAL_POWERS";

    private const string TOTAL_DISTANCE_KEY = "TOTAL_DISTANCE";
    private const string MAX_DISTANCE_KEY = "MAX_DISTANCE";

    private const string TOTAL_EARNED_COINS_KEY = "TOTAL_EARNED_COINS";
    private const string MAX_EARNED_COINS_KEY = "MAX_EARNED_COINS";

    // =========================================
    // UNITY
    // =========================================

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadData();

        ShowPanel(Panel.MainMenu);

        UpdateAllUI();
    }

    private void Update()
    {
        UpdatePowerTimer();

        // Example distance update
        if (hudPanel != null && hudPanel.activeSelf)
        {
            currentDistance += Time.deltaTime * 5f;

            currentScore = Mathf.FloorToInt(currentDistance);

            UpdateHUDUI();
        }
    }

    // =========================================
    // PANEL SYSTEM
    // =========================================

    public void ShowPanel(Panel panel)
    {
        HideAll();

        GameObject targetPanel = GetPanel(panel);

        if (targetPanel != null)
            targetPanel.SetActive(true);

        UpdateAllUI();
    }

    public void HideAll()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    private GameObject GetPanel(Panel panel)
    {
        return panel switch
        {
            Panel.MainMenu => mainMenuPanel,
            Panel.HUD => hudPanel,
            Panel.Pause => pausePanel,
            Panel.GameOver => gameOverPanel,
            Panel.Shop => shopPanel,
            Panel.Settings => settingsPanel,
            Panel.Leaderboard => leaderboardPanel,
            _ => null
        };
    }

    // =========================================
    // BUTTON FUNCTIONS
    // =========================================

    public void OnPlayButton()
{
    // Resume Game
    Time.timeScale = 1f;

    // Reset Score
    currentScore = 0;
    currentDistance = 0;

    // Open HUD
    ShowPanel(Panel.HUD);

    // Start Player Running
    PlayerRunner player = FindFirstObjectByType<PlayerRunner>();

    if (player != null)
    {
        player.StartRunning();
    }
}

    public void OnShopButton()
    {
        ShowPanel(Panel.Shop);
    }

    public void OnSettingsButton()
    {
        ShowPanel(Panel.Settings);
    }

    public void OnLeaderboardButton()
    {
        ShowPanel(Panel.Leaderboard);
    }

    public void OnCloseButton()
    {
        ShowPanel(Panel.MainMenu);
    }

    public void OnExitButton()
    {
        Debug.Log("Game Closed");

        Application.Quit();
    }

    // =========================================
    // PAUSE SYSTEM
    // =========================================

    private void OnApplicationPause(bool pauseStatus)
    {
        // If the app is sent to the background and the player is currently playing, force pause the game
        if (pauseStatus && hudPanel != null && hudPanel.activeSelf && Time.timeScale > 0)
        {
            OnPauseButton();
        }
    }

    public void OnPauseButton()
    {
        ShowPanel(Panel.Pause);

        Time.timeScale = 0f;
    }

    public void OnResumeButton()
    {
        ShowPanel(Panel.HUD);

        Time.timeScale = 1f;
    }

    public void OnEndGameButton()
    {
        Time.timeScale = 1f;

        // Reload the scene to completely reset the game state
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    // =========================================
    // GAME OVER SYSTEM
    // =========================================

    public void ShowGameOver()
    {
        Time.timeScale = 0f;

        AddRunData(currentDistance, currentScore);

        ShowPanel(Panel.GameOver);
    }

    public void OnGameOverMainMenuButton()
    {
        Time.timeScale = 1f;

        // Reload the scene to completely reset the game state
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    // =========================================
    // POWER SYSTEM
    // =========================================

    public bool IsPowerActive => powerActive;

    public void UsePower()
    {
        if (powerActive)
            return;

        if (totalPowers <= 0)
        {
            Debug.Log("No Power Available");
            return;
        }

        totalPowers--;

        powerActive = true;
        powerTimer = 10f;

        SaveData();
        UpdateAllUI();

        Debug.Log("Power Activated");
    }

    private void UpdatePowerTimer()
    {
        if (!powerActive)
        {
            if (powerTimerText != null)
                powerTimerText.text = "Timer : 0";

            return;
        }

        powerTimer -= Time.deltaTime;

        if (powerTimerText != null)
            powerTimerText.text =
                "Timer : " + Mathf.Ceil(powerTimer);

        if (powerTimer <= 0f)
        {
            powerActive = false;
            powerTimer = 0f;

            Debug.Log("Power Ended");
        }
    }

    // =========================================
    // SHOP SYSTEM
    // =========================================

    public void Buy1Power()
    {
        BuyPower(100, 1);
    }

    public void Buy2Power()
    {
        BuyPower(200, 2);
    }

    public void Buy5Power()
    {
        BuyPower(400, 5);
    }

    private void BuyPower(int coinCost, int powerAmount)
    {
        if (totalCoins >= coinCost)
        {
            totalCoins -= coinCost;
            totalPowers += powerAmount;

            SaveData();
            UpdateAllUI();

            Debug.Log("Purchase Success");
        }
        else
        {
            Debug.Log("Not Enough Coins");
        }
    }

    // =========================================
    // SAVE / LOAD
    // =========================================

    private void SaveData()
    {
        PlayerPrefs.SetInt(COIN_KEY, totalCoins);
        PlayerPrefs.SetInt(POWER_KEY, totalPowers);

        PlayerPrefs.SetFloat(TOTAL_DISTANCE_KEY, totalDistance);
        PlayerPrefs.SetFloat(MAX_DISTANCE_KEY, maxDistance);

        PlayerPrefs.SetInt(TOTAL_EARNED_COINS_KEY, totalEarnedCoins);
        PlayerPrefs.SetInt(MAX_EARNED_COINS_KEY, maxEarnedCoins);

        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        totalCoins = PlayerPrefs.GetInt(COIN_KEY, 1000);
        totalPowers = PlayerPrefs.GetInt(POWER_KEY, 0);

        totalDistance = PlayerPrefs.GetFloat(TOTAL_DISTANCE_KEY, 0);
        maxDistance = PlayerPrefs.GetFloat(MAX_DISTANCE_KEY, 0);

        totalEarnedCoins = PlayerPrefs.GetInt(TOTAL_EARNED_COINS_KEY, 0);
        maxEarnedCoins = PlayerPrefs.GetInt(MAX_EARNED_COINS_KEY, 0);
    }

    // =========================================
    // UI UPDATE
    // =========================================

    private void UpdateAllUI()
    {
        // Main Menu

        if (mainMenuCoinText != null)
            mainMenuCoinText.text = "Coins : " + totalCoins;

        if (mainMenuPowerText != null)
            mainMenuPowerText.text = "Power : " + totalPowers;

        // Shop

        if (shopCoinText != null)
            shopCoinText.text = "Coins : " + totalCoins;

        if (shopPowerText != null)
            shopPowerText.text = "Power : " + totalPowers;

        // HUD

        UpdateHUDUI();

        // Leaderboard

        if (totalDistanceText != null)
            totalDistanceText.text =
                "Total Distance : " + totalDistance.ToString("0");

        if (maxDistanceText != null)
            maxDistanceText.text =
                "Max Distance : " + maxDistance.ToString("0");

        if (totalEarnedCoinsText != null)
            totalEarnedCoinsText.text =
                "Total Coins : " + totalEarnedCoins;

        if (maxEarnedCoinsText != null)
            maxEarnedCoinsText.text =
                "Max Coins : " + maxEarnedCoins;
    }

    private void UpdateHUDUI()
    {
        if (scoreText != null)
            scoreText.text = "Score : " + currentScore;

        if (coinText != null)
            coinText.text = "Coins : " + totalCoins;

        if (distanceText != null)
            distanceText.text =
                "Distance : " +
                currentDistance.ToString("0") + "m";

        if (powerText != null)
            powerText.text = "Power : " + totalPowers;
    }

    // =========================================
    // GAMEPLAY SYSTEM
    // =========================================

    public void AddCoins(int amount)
    {
        totalCoins += amount;

        SaveData();
        UpdateAllUI();
    }

    public void AddPower(int amount)
    {
        totalPowers += amount;

        SaveData();
        UpdateAllUI();
    }

    // =========================================
    // LEADERBOARD SYSTEM
    // =========================================

    public void AddRunData(float runDistance, int earnedCoins)
    {
        totalDistance += runDistance;

        if (runDistance > maxDistance)
            maxDistance = runDistance;

        totalEarnedCoins += earnedCoins;

        if (earnedCoins > maxEarnedCoins)
            maxEarnedCoins = earnedCoins;

        SaveData();
        UpdateAllUI();
    }

    // =========================================
    // RESET DATA
    // =========================================

    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();

        totalCoins = 1000;
        totalPowers = 0;

        totalDistance = 0;
        maxDistance = 0;

        totalEarnedCoins = 0;
        maxEarnedCoins = 0;

        SaveData();
        UpdateAllUI();

        Debug.Log("All Data Reset");
    }
}