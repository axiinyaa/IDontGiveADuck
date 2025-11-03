using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// DuckSpawner - Manages the spawning of ducks during gameplay
/// 
/// This class demonstrates several important game development concepts:
/// - Coroutines: For time-based spawning without blocking the main thread
/// - Object Pooling Concepts: Managing active game objects
/// - Random Generation: Creating varied spawn patterns
/// - Event-Driven Architecture: Notifying other systems of spawn events
/// - Level-Based Configuration: Adapting spawn behavior to different levels
/// - Debug Visualisation: Using Gizmos to show spawn areas in the editor
/// </summary>
public class DuckSpawner : MonoBehaviour
{
    // ===== SPAWN AREA CONFIGURATION =====
    // Defines where ducks can appear in the game world
    
    [Header("Spawn Area")]
    [SerializeField] private Collider2D spawnArea;  // The area where ducks can spawn
    [SerializeField] private float spawnPadding = 0f;  // Distance from spawn area edges (prevents ducks spawning too close to borders)
    
    // ===== DEBUG SETTINGS =====
    // These help during development by visualising spawn areas
    
    [Header("Debug Settings")]
    [SerializeField] private bool showSpawnArea = true;     // Toggle spawn area visualisation
    [SerializeField] private Color spawnAreaColor = Color.green;  // Colour of spawn area in scene view
    
    // ===== RUNTIME STATE =====
    // These variables track the current state of the spawning system
    
    // Current level configuration - contains all spawn settings
    private Level currentLevel;
    private bool isSpawning = false;  // Flag to control if spawning is active
    
    // ===== SPAWN TRACKING =====
    // Keep track of how many ducks are left to spawn and currently active
    
    private int goodDucksRemaining = 0;      // How many good ducks still need to be spawned
    private int geeseRemaining = 0;     // How many decoy ducks still need to be spawned
    private List<GameObject> activeDucks = new List<GameObject>();  // All ducks currently in the scene
    
    // ===== COROUTINE MANAGEMENT =====
    // Reference to the spawning coroutine so we can stop it when needed
    
    private Coroutine spawnCoroutine;  // Reference to the currently running spawn coroutine
    
    #region Unity Lifecycle
    // Unity automatically calls these methods at specific times during the game's lifecycle

    
    /// <summary>
    /// Called by Unity's Gizmo system to draw debug information in the Scene view
    /// This helps visualise the spawn area during development
    /// </summary>
    void OnDrawGizmos()
    {
        if (showSpawnArea && spawnArea != null)
        {
            Gizmos.color = spawnAreaColor;
            // Draw a wireframe cube showing the spawn area bounds
            Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
        }
    }
    
    #endregion
    
    #region Setup and Validation
    
    #endregion
    
    #region Public Interface
    // These methods are called by other systems (like GameManager) to control spawning
    
    /// <summary>
    /// Start spawning ducks for the given level
    /// This is the main entry point for beginning a level's duck spawning
    /// </summary>
    public void StartSpawning()
    {
        Level level = LevelLoader.GetCurrentLevel();

        if (level == null)
        {
            Debug.LogError("Cannot start spawning - level data is null");
            return;
        }
        
        // Store the level configuration for use during spawning
        currentLevel = level;
        goodDucksRemaining = level.DucksToSpawn.Length;      // Set how many good ducks to spawn
        geeseRemaining = level.GeeseToSpawn.Length;    // Set how many decoy ducks to spawn

        isSpawning = true;  // Enable spawning flag
        
        // Clear any existing ducks from previous levels
        ClearActiveDucks();
        
        // Start the spawning coroutine (stops any existing one first)
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        spawnCoroutine = StartCoroutine(SpawnDucksCoroutine());
    }
    
    /// <summary>
    /// Stop spawning ducks
    /// Called when the level ends or is paused
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;  // Disable spawning flag
        
        // Stop the spawning coroutine if it's running
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        Debug.Log("Duck spawning stopped");
    }
    
    /// <summary>
    /// Clear all active ducks from the scene
    /// Used when starting a new level or resetting the game
    /// </summary>
    public void ClearActiveDucks()
    {
        // Destroy each active duck GameObject
        foreach (GameObject duck in activeDucks)
        {
            if (duck != null)
            {
                Destroy(duck);
            }
        }
        
        // Clear the list of active ducks
        activeDucks.Clear();
        Debug.Log("All active ducks cleared");
    }
    
    #endregion
    
    #region Spawning Logic
    // The core spawning system using coroutines for time-based spawning
    
    /// <summary>
    /// Main spawning coroutine - runs continuously while spawning is active
    /// 
    /// Coroutines are special methods that can pause execution and resume later
    /// This allows us to spawn ducks at regular intervals without blocking the main thread
    /// </summary>
    private IEnumerator SpawnDucksCoroutine()
    {
        Debug.Log("holy shit fucks i mean duck");
        // Keep spawning until told to stop
        while (isSpawning)
        {
            // Check win/lose conditions first
            if (GameManager.Instance != null)
            {
                // WIN: Player got required good ducks
                if (GameManager.Instance.GeeseClicked >= currentLevel.GeeseToSpawn.Length)
                {
                    Debug.Log("WIN: Player got required good ducks");
                    break;  // Exit the coroutine
                }
                
                // LOSE: Only when time runs out
                if (GameManager.Instance.Lives <= 0)
                {
                    Debug.Log("LOSE: Time ran out");
                    break;  // Exit the coroutine
                }
            }
            
            // Wait for the spawn interval before spawning the next duck
            // This creates the timing for duck spawning
            yield return new WaitForSeconds(currentLevel.SecondsBeforeSpawn);
            
            // Decide what type of duck to spawn (good or decoy)
            bool spawnGoodDuck = ShouldSpawnGoodDuck();

            if (!spawnGoodDuck && geeseRemaining > 0)
            {
                SpawnDecoyDuck();
            }
            else if (goodDucksRemaining >= 0)
            {
                SpawnGoodDuck();
            }
        }
        
    }
    
    /// <summary>
    /// Determine whether to spawn a good duck or decoy
    /// This creates the mix of duck types that makes the game challenging
    /// </summary>
    private bool ShouldSpawnGoodDuck()
    {
        if (goodDucksRemaining <= 0) return false;
        
        return Random.value < 0.5f;
    }
    
    /// <summary>
    /// Spawn a good duck (the ones players should click)
    /// </summary>
    private void SpawnGoodDuck()
    {
        // Select which size of good duck to spawn
        GameObject prefab = currentLevel.PickNextDuck();
        if (prefab == null) return;
        
        // Get a random position within the spawn area
        Vector2 spawnPosition = GetRandomSpawnPosition();
        
        // Create the duck GameObject at the spawn position
        GameObject duck = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // Configure the duck with level-specific properties
        Duck goodDuck = duck.GetComponent<Duck>();

        goodDuck.Initialize();

        goodDucksRemaining--;
        
        // Add to our list of active ducks for tracking
        activeDucks.Add(duck);
        
        // Notify the game manager about the spawn
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDuckSpawned();
            GameManager.Instance.OnGoodDuckSpawned();
        }
    }
    
    /// <summary>
    /// Spawn a decoy duck (the ones players should avoid)
    /// </summary>
    private void SpawnDecoyDuck()
    {
        // Select which size of decoy duck to spawn
        GameObject prefab = currentLevel.PickNextGoose();
        if (prefab == null) return;
        
        // Get a random position within the spawn area
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Create the duck GameObject at the spawn position
        GameObject duck = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // Add to our list of active ducks for tracking
        activeDucks.Add(duck);
        geeseRemaining--;  // Decrease the count of remaining decoys
        
        // Notify the game manager about the spawn
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDuckSpawned();
        }
        
        Debug.Log($"Spawned decoy duck. Remaining: {geeseRemaining}");
    }
    
    #endregion
    
    #region Spawn Position
    // Handles where ducks appear in the game world
    
    /// <summary>
    /// Get a random position within the spawn area
    /// This ensures ducks appear in valid locations
    /// </summary>
    public Vector2 GetRandomSpawnPosition()
    {

        // Get the bounds of the spawn area
        Bounds bounds = spawnArea.bounds;

        float x, y;

        while (true)
        {
            // Generate random X coordinate within bounds (with padding)
            x = Random.Range(bounds.min.x, bounds.max.x);
            y = Random.Range(bounds.min.y, bounds.max.y);

            Vector2 pos = transform.TransformPoint(new Vector2(x, y));

            return pos;
        }
    }
    
    #endregion
    
    #region Cleanup
    // Ensures proper cleanup when the spawner is destroyed
    
    /// <summary>
    /// Called when this GameObject is being destroyed
    /// Ensures we clean up properly to prevent memory leaks
    /// </summary>
    void OnDestroy()
    {
        StopSpawning();
    }
    
    #endregion
    
    #region Public Getters (for UI/debugging)
    // These properties allow other systems to check the spawner's state
    
    /// <summary>
    /// Whether the spawner is currently spawning ducks
    /// </summary>
    public bool IsSpawning => isSpawning;
    
    /// <summary>
    /// How many good ducks are left to spawn
    /// </summary>
    public int GoodDucksRemaining => goodDucksRemaining;
    
    /// <summary>
    /// How many decoy ducks are left to spawn
    /// </summary>
    public int DecoyDucksRemaining => geeseRemaining;
    
    /// <summary>
    /// How many ducks are currently active in the scene
    /// </summary>
    public int ActiveDuckCount => activeDucks.Count;
    
    /// <summary>
    /// The current level configuration being used for spawning
    /// </summary>
    public Level CurrentLevel => currentLevel;
    
    #endregion
}