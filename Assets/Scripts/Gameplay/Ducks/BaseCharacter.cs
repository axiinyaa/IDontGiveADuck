using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Abstract base class for all duck types
/// Provides common functionality and enforces consistent interface
/// </summary>
public abstract class BaseCharacter : MonoBehaviour
{
    [Header("Base Duck Properties")]
    [SerializeField] protected int pointValue = 1;
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected float moveSpeed = 0f; // For future moving ducks
    
    [Header("Visual Feedback")]
    [SerializeField] protected ParticleSystem destroyEffect;
    [SerializeField] protected AudioClip clickSound;
    
    // Protected properties accessible to child classes
    protected float currentLifetime;
    protected bool isClicked = false;
    protected bool isInitialized = false;
    public bool finishedLanding = false;
    public Vector2 startingPosition;
    public Vector2 spawnPosition;
    public Rigidbody2D body;
    public Collider2D collision;

    // Movement
    public Vector2 targetPosition;
    
    // Public properties for external access
    public int PointValue => pointValue;
    public bool IsClicked => isClicked;
    
    #region Unity Lifecycle
    
    protected virtual void Start()
    {
        Initialize();

        startingPosition = transform.position;
        targetPosition = transform.position;
        transform.position = new Vector3(startingPosition.x, startingPosition.y + 30f, -1);

        body = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Called every frame by Unity - this is where we handle all duck behaviour
    /// 
    /// Update() is one of Unity's most important methods:
    /// - Called once per frame (typically 60 times per second)
    /// - Used for continuous updates like movement, timers, input
    /// - Should be efficient since it runs so frequently
    /// </summary>
    protected virtual void Update()
    {
        // Safety check: Don't do anything if duck isn't properly set up yet
        // This prevents errors during the brief moment between object creation and initialization
        if (!isInitialized) return;

        if (Vector2.Distance(transform.position, startingPosition) > 2f && !finishedLanding)
        {
            transform.position = Vector2.Lerp(transform.position, startingPosition, Time.deltaTime);
        }
        else
        {
            finishedLanding = true;
        }
    }
    
    protected virtual void FixedUpdate()
    {
        // Handle duck movement (if any)
        // Currently not implemented but ready for future moving ducks
        HandleMovement();
    }
    
    // Keep OnMouseDown as backup for older Unity versions
    protected virtual void OnMouseDown()
    {
        if (!isClicked && isInitialized)
        {
            isClicked = true;
            // Disable collider to prevent further clicks
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            OnClicked();
        }
    }
    
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Initialise duck with custom properties
    /// </summary>
    public virtual void Initialize(float customLifetime = -1, int customPointValue = -1)
    {
        currentLifetime = customLifetime > 0 ? customLifetime : lifetime;
        if (customPointValue > 0) pointValue = customPointValue;

        isInitialized = true;
        OnDuckSpawned();
    }
    
    #endregion
    
    #region Core Behaviors
    
    /// <summary>
    /// Handle duck movement (override in child classes)
    /// </summary>
    protected virtual void HandleMovement()
    {
        // Base implementation - no movement
        // Override in child classes for moving ducks

        
    }
    
    /// <summary>
    /// Common destruction logic with effects
    /// </summary>
    protected virtual void DestroyDuck()
    {
        // Play destruction effects
        if (destroyEffect != null)
        {
            // Create effect at duck position
            ParticleSystem effect = Instantiate(destroyEffect, transform.position, transform.rotation);
            Destroy(effect.gameObject, effect.main.duration);
        }
        
        // Play sound effect
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, transform.position);
        }
        
        // Remove duck from scene
        Destroy(gameObject);
    }
    
    #endregion
    
    #region Abstract Methods - Must be implemented by child classes
    
    /// <summary>
    /// Handle duck click behaviour - specific to each duck type
    /// </summary>
    public abstract void OnClicked();
    
    #endregion
    
    #region Virtual Methods - Can be overridden by child classes
    
    /// <summary>
    /// Called when duck is first spawned
    /// </summary>
    protected virtual void OnDuckSpawned()
    {
        // Default implementation - can be overridden
        Debug.Log($"{GetType().Name} spawned at position {transform.position} with {currentLifetime}s lifetime");
    }
    
    /// <summary>
    /// Called when duck lifetime is getting low (< 1 second)
    /// </summary>
    protected virtual void OnLifetimeLow()
    {
        // Default implementation - visual warning
        // Override for custom low-lifetime effects
    }
    
    #endregion
    
    #region Debug Helpers
    
    #endregion
}