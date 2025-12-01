using UnityEngine;
using UnityEngine.UI;

using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// UIManager - Centralised UI system for the game
/// 
/// This class demonstrates several important game development concepts:
/// - Event-Driven UI Updates: UI automatically updates when game state changes
/// - Singleton Pattern Usage: Works with GameManager singleton
/// - UI State Management: Shows/hides different panels based on game state
/// - Button Event Handling: Connects UI buttons to game actions
/// - Debug Tools: Built-in debugging features for development
/// - Input System Integration: Uses Unity's new Input System for keyboard input
/// </summary>
public class UIManager : MonoBehaviour
{
    // ===== UI ELEMENTS =====
    // These are references to UI components that will be set in the Unity Inspector
    // The [SerializeField] attribute makes private fields visible in the Inspector
    
    [Header("HUD Elements")]
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject title;
    [SerializeField] private LevelSelect levelSelect;
    [SerializeField] private TextMeshProUGUI scoreText;      // Shows current score
    [SerializeField] private TextMeshProUGUI timerText;      // Shows time remaining
    [SerializeField] private TextMeshProUGUI livesText;      // Shows remaining lives
    [SerializeField] private TextMeshProUGUI levelText;      // Shows current level number
    [SerializeField] private TextMeshProUGUI progressText;   // Shows progress (ducks clicked/required)
    [SerializeField] private GameObject wantedPosterPrefab;
    [SerializeField] private GameObject wantedPosterContainer;
    [SerializeField] private GameObject tutorial;
    
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;       // Container for game over UI
    [SerializeField] private TextMeshProUGUI gameOverTitle;  // "Level Complete!" or "Level Failed!"
    [SerializeField] private TextMeshProUGUI finalScoreText; // Shows final score or restart message
    [SerializeField] private Button restartButton;           // Button to restart current level
    [SerializeField] private Button nextLevelButton;         // Button to go to next level

    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;          // Container for pause menu
    [SerializeField] private Button resumeButton;            // Button to resume game
    [SerializeField] private Button pauseRestartButton;      // Button to restart from pause menu
    
    [Header("Instructions Panel")]
    [SerializeField] private GameObject instructionsPanel;   // Container for game instructions
    [SerializeField] private Button startGameButton;         // Button to start the game
    [SerializeField] private Button testLevel12Button;       // Button to jump to test level
    
    // ===== DEVELOPMENT TOOLS =====
    // These settings help during development and testing
    
    [Header("Testing Tools")]
    [SerializeField] private bool showTestButton = true;     // Toggle to show/hide test button in build
    [SerializeField] private int testButtonLevel = 12;       // Which level to jump to when test button is clicked
    
    [Header("Settings")]
    [SerializeField] private bool showDebugInfo = true;      // Toggle to show debug information on screen
    [SerializeField] private Color timerWarningColor = Color.red;  // Colour when time is running low
    [SerializeField] private float timerWarningThreshold = 10f;    // Time remaining when warning starts
    
    // ===== PRIVATE VARIABLES =====
    private Color originalTimerColor;  // Stores the original timer colour to restore it later
    
    #region Unity Lifecycle
    // Unity automatically calls these methods at specific times during the game's lifecycle
    
    /// <summary>
    /// Called when the script instance is being loaded
    /// This happens before Start() and is used for initialisation
    /// </summary>
    void Awake()
    {
        // Store the original timer colour so we can restore it later
        if (timerText != null)
            originalTimerColor = timerText.color;
        
        // Set up all button click listeners
        SetupButtonListeners();
    }
    
    /// <summary>
    /// Called on the frame when the script is enabled, just before Update()
    /// This is where we connect to the GameManager's events
    /// </summary>
    void Start()
    {
        // Subscribe to GameManager events so UI updates automatically
        // This is an example of the Observer pattern - UI "listens" for game changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;        // When score changes
            GameManager.Instance.OnLivesChanged += UpdateLives;        // When lives change
            GameManager.Instance.OnGameStateChanged += UpdateGameState; // When game state changes
            GameManager.Instance.OnLevelLoaded += UpdateLevelInfo;     // When new level loads
            GameManager.Instance.OnGetWantedPosters += UpdateWantedPosters;
            GameManager.Instance.OnClearWantedPosters += ClearWantedPosters;
        }
        
        // Start with a clean UI state
        HideHUDElements();
        MainMenu();
    }
    
    /// <summary>
    /// Called when the script is being destroyed
    /// Important to unsubscribe from events to prevent memory leaks
    /// </summary>
    void OnDestroy()
    {
        // Unsubscribe from all events to prevent errors when object is destroyed
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnGameStateChanged -= UpdateGameState;
            GameManager.Instance.OnLevelLoaded -= UpdateLevelInfo;
        }
    }
    
    #endregion
    
    #region Setup
    
    /// <summary>
    /// Connects all UI buttons to their corresponding action methods
    /// This uses Unity's event system - when button is clicked, method is called
    /// </summary>
    private void SetupButtonListeners()
    {
        // Each button.onClick.AddListener() connects a button to a method
        // When the button is clicked, the method will be called automatically
        
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseRestartButton != null)
            pauseRestartButton.onClick.AddListener(OnRestartClicked);
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);
    }
    
    #endregion
    
    #region HUD Updates
    // These methods update the Heads-Up Display (HUD) elements
    // They are called automatically when game data changes
    
    /// <summary>
    /// Updates the score display with the new score value
    /// The {score:N0} format adds commas for thousands (e.g., "1,234")
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score:N0}";
    }
    
    /// <summary>
    /// Updates the lives display
    /// </summary>
    public void UpdateLives(int lives)
    {
        livesText.text = $"Lives: {lives}";
        livesText.color = Color.red;
    }
    
    /// <summary>
    /// Updates level information when a new level is loaded
    /// </summary>
    public void UpdateLevelInfo()
    {
        if (levelText != null)
            levelText.text = $"Level: {GameManager.Instance.currentLevel + 1}";
        
        UpdateProgress();
    }
    
    /// <summary>
    /// Updates the progress display showing how many good ducks have been clicked
    /// </summary>
    public void UpdateProgress()
    {
        if (progressText != null && GameManager.Instance != null)
        {
            int clicked = GameManager.Instance.GeeseClicked;
            int required = GameManager.Instance.GeeseRequired;
            progressText.text = $"Progress: {clicked}/{required}";
        }
    }

    public void UpdateWantedPosters()
    {
        List<Goose> existingGeese = new();

        wantedPosterContainer.transform.localScale = new Vector3(2, 2, 2);

        foreach (Goose goose in LevelLoader.GetCurrentLevel().GetGeese())
        {
            if (existingGeese.Contains(goose)) continue;

            existingGeese.Add(goose);

            WantedPoster wantedPoster = Instantiate(wantedPosterPrefab, wantedPosterContainer.transform).GetComponent<WantedPoster>();

            Texture texture = goose.idleAnimation?.frames[0].texture;

            if (texture == null) return;

            wantedPoster.gooseImage.texture = texture;
        }
    }

    public void ClearWantedPosters()
    {
        foreach (Transform obj in wantedPosterContainer.transform)
        {
            Destroy(obj.gameObject);
        }
    }
    
    #endregion
    
    #region Game State Updates
    
    /// <summary>
    /// Responds to game state changes by showing the appropriate UI
    /// This is called automatically when the game state changes
    /// </summary>
    public void UpdateGameState(GameState newState)
    {
        // Switch statement checks which state the game is in and shows appropriate UI
        switch (newState)
        {
            case GameState.Menu:
                MainMenu();
                break;
            case GameState.Playing:
                ShowGameHUD();
                break;
            case GameState.Paused:
                ShowPausePanel();
                break;
            case GameState.LevelComplete:
                ShowLevelComplete();
                break;
            case GameState.GameOver:
                ShowGameOver(false);
                break;
            case GameState.GameComplete:
                ShowGameComplete();
                break;
        }
    }
    
    #endregion
    
    #region HUD Visibility Control
    // These methods control which UI elements are visible at any time
    
    /// <summary>
    /// Hides all HUD elements (score, timer, lives, etc.)
    /// Used when showing menus or instructions
    /// </summary>
    private void HideHUDElements()
    {
        HUD.SetActive(false);
    }
    
    /// <summary>
    /// Shows all HUD elements
    /// Used when the game is actively being played
    /// </summary>
    private void ShowHUDElements()
    {
        HUD.SetActive(true);
    }
    
    #endregion
    
    #region Panel Management
    // These methods control which UI panels are shown/hidden
    // Each method handles a specific game state or UI screen
    
    /// <summary>
    /// Shows the instructions panel at the start of the game
    /// If no instructions panel exists, starts the game immediately
    /// </summary>
    public void MainMenu()
    {
        title.SetActive(true);
        
        ResetLevelButtons();

        SetAllPanelsInactive();  // Hide all other panels first
        HideHUDElements();       // Hide HUD elements
    }

    private void ResetLevelButtons()
    {
        foreach (LevelButton button in levelSelect.GetLevelButtons())
        {
            button.button.onClick.AddListener(() => OnLevelButtonClicked(button.level));
        }
    }

    public void OnLevelButtonClicked(int level)
    {
        levelSelect.gameObject.SetActive(false);

        if (level == 0)
        {
            tutorial.SetActive(true);
            return;
        }

        GameManager.Instance.JumpToLevel(level);
    }
    
    /// <summary>
    /// Shows the game HUD when actively playing
    /// </summary>
    private void ShowGameHUD()
    {
        SetAllPanelsInactive();  // Hide all panels
        ShowHUDElements();       // Show HUD elements
        UpdateProgress();        // Update progress display
    }
    
    /// <summary>
    /// Shows the pause panel when game is paused
    /// </summary>
    private void ShowPausePanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    /// <summary>
    /// Shows the level complete screen with appropriate text and buttons
    /// </summary>
    private void ShowLevelComplete()
    {
        if (gameOverPanel != null)
        {
            StartCoroutine(LevelCompleteSequence());
        }
    }

    private IEnumerator LevelCompleteSequence()
    {
        PlayerPrefs.SetInt("Level", GameManager.Instance.currentLevel + 1);

        yield return new WaitForSeconds(1);

        gameOverPanel.SetActive(true);

        // Set the title text
        if (gameOverTitle != null)
            gameOverTitle.text = "Level Complete!";

        // Show the final score
        if (finalScoreText != null && GameManager.Instance != null)
            finalScoreText.text = $"Final Score: {GameManager.Instance.Score:N0}";

        // Show/hide next level button based on whether there is a next level
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(GameManager.Instance.currentLevel < LevelLoader.Instance.levels.Length - 1);

            // Update button text to show which level is next
            var tmpText = nextLevelButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
                tmpText.text = GameManager.Instance.currentLevel + 1 > 0 ? $"Next Level ({GameManager.Instance.currentLevel + 2})" : "Next Level";
        }
    }
    
    /// <summary>
    /// Shows the game over screen when player fails a level
    /// </summary>
    private void ShowGameOver(bool isCompleteGameOver)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverTitle != null)
                gameOverTitle.text = "Level Failed!";
            
            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = $"Final Score: {GameManager.Instance.Score:N0}";
            
            // Hide next level button since player failed
            if (nextLevelButton != null)
                nextLevelButton.GetComponentInChildren<TextMeshProUGUI>().text = "Retry Level";
        }
    }
    
    /// <summary>
    /// Shows the game complete screen when all levels are finished
    /// </summary>
    private void ShowGameComplete()
    {
        HideHUDElements();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverTitle != null)
                gameOverTitle.text = "Game Complete";
            
            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = $"Final Score: {GameManager.Instance.Score:N0}";
            
            // Hide next level button since game is complete
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Hides all UI panels at once
    /// Used when switching between different UI states
    /// </summary>
    private void SetAllPanelsInactive()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }
    
    #endregion
    
    #region Button Handlers
    // These methods are called when UI buttons are clicked
    // They communicate with the GameManager to perform game actions
    
    /// <summary>
    /// Called when the "Start Game" button is clicked
    /// Hides instructions and starts the game
    /// </summary>
    private void OnStartGameClicked()
    {  
    }
    
    /// <summary>
    /// Called when any restart button is clicked
    /// Restarts the current level
    /// </summary>
    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        GameManager.Instance.RestartLevel();
    }
    
    /// <summary>
    /// Called when the "Next Level" button is clicked
    /// Advances to the next level
    /// </summary>
    private void OnNextLevelClicked()
    {
        GameManager.Instance?.AdvanceToNextLevel();
    }
    
    /// <summary>
    /// Called when the "Resume" button is clicked
    /// Unpauses the game
    /// </summary>
    private void OnResumeClicked()
    {
        GameManager.Instance?.TogglePause();
    }
    
    
    #endregion
    
    #region Input Handling
    
    /// <summary>
    /// Called every frame by Unity
    /// Handles keyboard input and updates UI elements
    /// </summary>
    void Update()
    {
        // Check if Escape key was pressed this frame
        // This uses Unity's new Input System
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Only allow pause/unpause when playing or already paused
            if (GameManager.Instance != null && 
                (GameManager.Instance.CurrentState == GameState.Playing || 
                 GameManager.Instance.CurrentState == GameState.Paused))
            {
                GameManager.Instance.TogglePause();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && GameManager.Instance.CurrentState == GameState.Menu)
        {
            PlayerPrefs.DeleteAll();
            ResetLevelButtons();
        }

        wantedPosterContainer.transform.localScale = Vector3.Lerp(wantedPosterContainer.transform.localScale, Vector3.one, Time.deltaTime);

        livesText.color = Color.Lerp(livesText.color, Color.white, Time.deltaTime);
        
        UpdateProgress();
    }
    
    #endregion
    
    #region Debug Info
    
    #endregion
}