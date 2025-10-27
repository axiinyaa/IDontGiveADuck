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
    // ===== DUCK PREFABS =====
    // These arrays store the different duck prefabs that can be spawned
    // Each array has exactly 3 elements: Large, Medium, and Small ducks
    
    [Header("Duck Prefabs")]
    [SerializeField] private GameObject[] goodDuckPrefabs; // [0]=Large, [1]=Medium, [2]=Small
    [SerializeField] private GameObject[] decoyDuckPrefabs; // [0]=Large, [1]=Medium, [2]=Small
    
    // ===== SPAWN AREA CONFIGURATION =====
    // Defines where ducks can appear in the game world
    
    [Header("Spawn Area")]
    [SerializeField] private Collider2D spawnArea;  // The area where ducks can spawn
    [SerializeField] private float spawnPadding = 1f;  // Distance from spawn area edges (prevents ducks spawning too close to borders)
    
    // ===== DEBUG SETTINGS =====
    // These help during development by visualising spawn areas
    
    [Header("Debug Settings")]
    [SerializeField] private bool showSpawnArea = true;     // Toggle spawn area visualisation
    [SerializeField] private Color spawnAreaColor = Color.green;  // Colour of spawn area in scene view
    
    // ===== RUNTIME STATE =====
    // These variables track the current state of the spawning system
    
    // Current level configuration - contains all spawn settings
    private LevelData currentLevel;
    private bool isSpawning = false;  // Flag to control if spawning is active
    
    // ===== SPAWN TRACKING =====
    // Keep track of how many ducks are left to spawn and currently active
    
    private int goodDucksRemaining = 0;      // How many good ducks still need to be spawned
    private int decoyDucksRemaining = 0;     // How many decoy ducks still need to be spawned
    private List<GameObject> activeDucks = new List<GameObject>();  // All ducks currently in the scene
    
    // ===== COROUTINE MANAGEMENT =====
    // Reference to the spawning coroutine so we can stop it when needed
    
    private Coroutine spawnCoroutine;  // Reference to the currently running spawn coroutine
    
    #region Unity Lifecycle
    // Unity automatically calls these methods at specific times during the game's lifecycle
    
    /// <summary>
    /// Called when the script instance is being loaded
    /// This happens before Start() and is used for initialisation
    /// </summary>
    void Awake()
    {
        // Check that all required prefabs are properly configured
        ValidatePrefabs();
    }
    
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
    
    /// <summary>
    /// Validates that all required prefabs are properly configured
    /// This prevents runtime errors by catching configuration issues early
    /// </summary>
    private void ValidatePrefabs()
    {
        // Check good duck prefabs array
        if (goodDuckPrefabs == null || goodDuckPrefabs.Length < 3)
        {
            Debug.LogError("DuckSpawner: Good duck prefabs array must have 3 elements [Large, Medium, Small]");
        }
        
        // Check decoy duck prefabs array
        if (decoyDuckPrefabs == null || decoyDuckPrefabs.Length < 3)
        {
            Debug.LogError("DuckSpawner: Decoy duck prefabs array must have 3 elements [Large, Medium, Small]");
        }
        
        // Validate that each prefab has the correct component
        for (int i = 0; i < goodDuckPrefabs.Length; i++)
        {
            if (goodDuckPrefabs[i] != null && goodDuckPrefabs[i].GetComponent<Duck>() == null)
            {
                Debug.LogError($"Good duck prefab {i} is missing GoodDuck component");
            }
        }
        
        for (int i = 0; i < decoyDuckPrefabs.Length; i++)
        {
            if (decoyDuckPrefabs[i] != null && decoyDuckPrefabs[i].GetComponent<Goose>() == null)
            {
                Debug.LogError($"Decoy duck prefab {i} is missing DecoyDuck component");
            }
        }
    }
    
    #endregion
    
    #region Public Interface
    // These methods are called by other systems (like GameManager) to control spawning
    
    /// <summary>
    /// Start spawning ducks for the given level
    /// This is the main entry point for beginning a level's duck spawning
    /// </summary>
    public void StartSpawning(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("Cannot start spawning - level data is null");
            return;
        }
        
        // Store the level configuration for use during spawning
        currentLevel = levelData;
        goodDucksRemaining = levelData.goodDucks;      // Set how many good ducks to spawn
        decoyDucksRemaining = levelData.geese;    // Set how many decoy ducks to spawn

        isSpawning = true;  // Enable spawning flag
        
        // Clear any existing ducks from previous levels
        ClearActiveDucks();
        
        // Start the spawning coroutine (stops any existing one first)
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        spawnCoroutine = StartCoroutine(SpawnDucksCoroutine());
        
        Debug.Log($"Started spawning for level {levelData.levelId}: {levelData.goodDucks} good, {levelData.geese} decoys, maxTotalSpawns: {levelData.maxTotalSpawns}, continueSpawning: {levelData.continueSpawning}");
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
                if (GameManager.Instance.GeeseClicked >= currentLevel.geese)
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
            yield return new WaitForSeconds(currentLevel.spawnRate);
            
            // Decide what type of duck to spawn (good or decoy)
            bool spawnGoodDuck = ShouldSpawnGoodDuck();

            if (!spawnGoodDuck && decoyDucksRemaining > 0)
            {
                SpawnDecoyDuck();
            }
            else if (goodDucksRemaining >= 0)
            {
                SpawnGoodDuck();
            }
        }
        
        Debug.Log($"Spawning completed - Total good ducks spawned: {GameManager.Instance?.TotalGoodDucksSpawned ?? 0}");
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
        GameObject prefab = SelectGoodDuckPrefab();
        if (prefab == null) return;
        
        // Get a random position within the spawn area
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Create the duck GameObject at the spawn position
        GameObject duck = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // Configure the duck with level-specific properties
        Duck goodDuck = duck.GetComponent<Duck>();
        if (goodDuck != null)
        {
            goodDuck.Initialize(currentLevel.duckLifetime);
        }

        goodDucksRemaining--;
        
        // Add to our list of active ducks for tracking
        activeDucks.Add(duck);
        
        // Notify the game manager about the spawn
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDuckSpawned();
            GameManager.Instance.OnGoodDuckSpawned();
        }
        
        Debug.Log($"Spawned good duck. Total spawned: {GameManager.Instance?.TotalGoodDucksSpawned ?? 0}");
    }
    
    /// <summary>
    /// Spawn a decoy duck (the ones players should avoid)
    /// </summary>
    private void SpawnDecoyDuck()
    {
        // Select which size of decoy duck to spawn
        GameObject prefab = SelectDecoyDuckPrefab();
        if (prefab == null) return;
        
        // Get a random position within the spawn area
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Create the duck GameObject at the spawn position
        GameObject duck = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // Configure the duck with level-specific properties
        Goose decoyDuck = duck.GetComponent<Goose>();
        if (decoyDuck != null)
        {
            decoyDuck.Initialize(currentLevel.duckLifetime);
        }
        
        // Add to our list of active ducks for tracking
        activeDucks.Add(duck);
        decoyDucksRemaining--;  // Decrease the count of remaining decoys
        
        // Notify the game manager about the spawn
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDuckSpawned();
        }
        
        Debug.Log($"Spawned decoy duck. Remaining: {decoyDucksRemaining}");
    }
    
    #endregion
    
    #region Prefab Selection
    // These methods choose which size of duck to spawn based on level configuration
    
    /// <summary>
    /// Select a good duck prefab based on size distribution from level data
    /// This creates variety in duck sizes according to the level's design
    /// </summary>
    private GameObject SelectGoodDuckPrefab()
    {
        if (goodDuckPrefabs == null || goodDuckPrefabs.Length < 3)
        {
            Debug.LogError("Good duck prefabs not properly configured");
            return null;
        }
        
        // Generate a random number between 0 and 1
        float rand = Random.value;
        LevelData.SizeDistribution dist = currentLevel.sizeDistribution;
        
        // Use the size distribution to determine which duck to spawn
        // This creates the mix of large, medium, and small ducks
        if (rand < dist.large)
            return goodDuckPrefabs[0]; // Large duck
        else if (rand < dist.large + dist.medium)
            return goodDuckPrefabs[1]; // Medium duck
        else
            return goodDuckPrefabs[2]; // Small duck
    }
    
    /// <summary>
    /// Select a decoy duck prefab based on size distribution from level data
    /// Uses the same logic as good ducks for consistency
    /// </summary>
    private GameObject SelectDecoyDuckPrefab()
    {
        if (decoyDuckPrefabs == null || decoyDuckPrefabs.Length < 3)
        {
            Debug.LogError("Decoy duck prefabs not properly configured");
            return null;
        }
        
        // Generate a random number between 0 and 1
        float rand = Random.value;
        LevelData.SizeDistribution dist = currentLevel.sizeDistribution;
        
        // Use the same size distribution logic as good ducks
        if (rand < dist.large)
            return decoyDuckPrefabs[0]; // Large duck
        else if (rand < dist.large + dist.medium)
            return decoyDuckPrefabs[1]; // Medium duck
        else
            return decoyDuckPrefabs[2]; // Small duck
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
        if (spawnArea == null)
        {
            Debug.LogWarning("No spawn area defined, using origin");
            return Vector3.zero;
        }

        // Get the bounds of the spawn area
        Bounds bounds = spawnArea.bounds;

        float x, y;

        while (true)
        {
            // Generate random X coordinate within bounds (with padding)
            x = Random.Range(
                bounds.min.x + spawnPadding,  // Left edge + padding
                bounds.max.x - spawnPadding   // Right edge - padding
            );

            // Generate random Y coordinate within bounds (with padding)
            y = Random.Range(
                bounds.min.y + spawnPadding,  // Bottom edge + padding
                bounds.max.y - spawnPadding   // Top edge - padding
            );

            Vector2 pos = new Vector2(x, y);

            if (!bounds.Contains(pos)) continue;

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
    public int DecoyDucksRemaining => decoyDucksRemaining;
    
    /// <summary>
    /// How many ducks are currently active in the scene
    /// </summary>
    public int ActiveDuckCount => activeDucks.Count;
    
    /// <summary>
    /// The current level configuration being used for spawning
    /// </summary>
    public LevelData CurrentLevel => currentLevel;
    
    #endregion
}