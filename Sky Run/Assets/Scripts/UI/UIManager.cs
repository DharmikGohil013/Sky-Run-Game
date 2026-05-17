using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public enum Panel { MainMenu, HUD, Pause, GameOver, Shop, Settings, Leaderboard }

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject leaderboardPanel;

    [Header("Main Menu UI")]
    [SerializeField] private TMP_Text mainMenuCoinText;
    [SerializeField] private TMP_Text mainMenuPowerText;

    [Header("HUD UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text powerText;
    [SerializeField] private TMP_Text powerTimerText;

    [Header("Shop UI")]
    [SerializeField] private TMP_Text shopCoinText;
    [SerializeField] private TMP_Text shopPowerText;

    [Header("Leaderboard UI")]
    [SerializeField] private TMP_Text totalDistanceText;
    [SerializeField] private TMP_Text maxDistanceText;
    [SerializeField] private TMP_Text totalEarnedCoinsText;
    [SerializeField] private TMP_Text maxEarnedCoinsText;

    private int totalCoins;
    private int totalPowers;

    private int currentScore;
    private float currentDistance;

    private bool powerActive;
    private float powerTimer;

    private float totalDistance;
    private float maxDistance;
    private int totalEarnedCoins;
    private int maxEarnedCoins;

    private const string COIN_KEY = "TOTAL_COINS";
    private const string POWER_KEY = "TOTAL_POWERS";
    private const string TOTAL_DISTANCE_KEY = "TOTAL_DISTANCE";
    private const string MAX_DISTANCE_KEY = "MAX_DISTANCE";
    private const string TOTAL_EARNED_COINS_KEY = "TOTAL_EARNED_COINS";
    private const string MAX_EARNED_COINS_KEY = "MAX_EARNED_COINS";

    private void Awake()
    {
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

        if (hudPanel != null && hudPanel.activeSelf)
        {
            currentDistance += Time.deltaTime * 5f;
            currentScore = Mathf.FloorToInt(currentDistance);
            UpdateHUDUI();
        }
    }

    public void ShowPanel(Panel panel)
    {
        HideAll();
        GetPanel(panel)?.SetActive(true);
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

    private GameObject GetPanel(Panel panel) => panel switch
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

    public void OnPlayButton()
    {
        Time.timeScale = 1f;
        currentScore = 0;
        currentDistance = 0;
        ShowPanel(Panel.HUD);

        FindFirstObjectByType<PlayerRunner>()?.StartRunning();
    }

    public void OnShopButton() => ShowPanel(Panel.Shop);
    public void OnSettingsButton() => ShowPanel(Panel.Settings);
    public void OnLeaderboardButton() => ShowPanel(Panel.Leaderboard);
    public void OnCloseButton() => ShowPanel(Panel.MainMenu);

    public void OnExitButton() => Application.Quit();

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && hudPanel != null && hudPanel.activeSelf && Time.timeScale > 0)
            OnPauseButton();
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        AddRunData(currentDistance, currentScore);
        ShowPanel(Panel.GameOver);
    }

    public void OnGameOverMainMenuButton()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsPowerActive => powerActive;

    public void UsePower()
    {
        if (powerActive || totalPowers <= 0) return;

        totalPowers--;
        powerActive = true;
        powerTimer = 10f;

        SaveData();
        UpdateAllUI();
    }

    private void UpdatePowerTimer()
    {
        if (!powerActive)
        {
            if (powerTimerText != null) powerTimerText.text = "Timer : 0";
            return;
        }

        powerTimer -= Time.deltaTime;

        if (powerTimerText != null)
            powerTimerText.text = "Timer : " + Mathf.Ceil(powerTimer);

        if (powerTimer <= 0f)
        {
            powerActive = false;
            powerTimer = 0f;
        }
    }

    public void Buy1Power() => BuyPower(100, 1);
    public void Buy2Power() => BuyPower(200, 2);
    public void Buy5Power() => BuyPower(400, 5);

    private void BuyPower(int coinCost, int powerAmount)
    {
        if (totalCoins < coinCost) return;

        totalCoins -= coinCost;
        totalPowers += powerAmount;

        SaveData();
        UpdateAllUI();
    }

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

    private void UpdateAllUI()
    {
        if (mainMenuCoinText != null) mainMenuCoinText.text = "Coins : " + totalCoins;
        if (mainMenuPowerText != null) mainMenuPowerText.text = "Power : " + totalPowers;

        if (shopCoinText != null) shopCoinText.text = "Coins : " + totalCoins;
        if (shopPowerText != null) shopPowerText.text = "Power : " + totalPowers;

        if (totalDistanceText != null) totalDistanceText.text = "Total Distance : " + totalDistance.ToString("0");
        if (maxDistanceText != null) maxDistanceText.text = "Max Distance : " + maxDistance.ToString("0");
        if (totalEarnedCoinsText != null) totalEarnedCoinsText.text = "Total Coins : " + totalEarnedCoins;
        if (maxEarnedCoinsText != null) maxEarnedCoinsText.text = "Max Coins : " + maxEarnedCoins;

        UpdateHUDUI();
    }

    private void UpdateHUDUI()
    {
        if (scoreText != null) scoreText.text = "Score : " + currentScore;
        if (coinText != null) coinText.text = "Coins : " + totalCoins;
        if (distanceText != null) distanceText.text = "Distance : " + currentDistance.ToString("0") + "m";
        if (powerText != null) powerText.text = "Power : " + totalPowers;
    }

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

    public void AddRunData(float runDistance, int earnedCoins)
    {
        totalDistance += runDistance;
        if (runDistance > maxDistance) maxDistance = runDistance;

        totalEarnedCoins += earnedCoins;
        if (earnedCoins > maxEarnedCoins) maxEarnedCoins = earnedCoins;

        SaveData();
        UpdateAllUI();
    }

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
    }
}