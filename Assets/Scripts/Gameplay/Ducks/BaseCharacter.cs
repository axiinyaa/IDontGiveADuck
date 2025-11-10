using Unity.Collections;
using UnityEngine;
using Axiinyaa.Tweening;

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
    public bool scared = false;
    public Vector2 startingPosition;
    public Vector2 spawnPosition;
    public Rigidbody2D body;
    public Collider2D collision;

    private Vector2 currentScale;

    // Movement
    public Vector2 targetPosition;
    
    // Public properties for external access
    public int PointValue => pointValue;
    public bool IsClicked => isClicked;

    [Header("Animations")]
    [SerializeField] SpriteAnimation idleAnimation;
    [SerializeField] SpriteAnimation flyAnimation;
    [SerializeField] SpriteAnimation scareAnimation;
    
    #region Unity Lifecycle
    
    protected virtual void Start()
    {
        Initialize();

        body = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collider2D>();

        currentScale = transform.localScale;
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

        if (scared)
        {
            FlyAway();
            return;
        }

        if (!finishedLanding) FlyDown();

        KeepSize();
    }

    private void KeepSize()
    {
        transform.localScale = Vector2.Lerp(transform.localScale, currentScale, Time.deltaTime * 2);
    }

    private void FlyAway()
    {
        transform.position = new Vector2.Tween(new Vector2(startingPosition.x, startingPosition.y + 25), 10, Easing.EaseOut);
        if (flyAnimation != null) flyAnimation.Play();
    }
    
    private void FlyDown()
    {
        if (Vector2.Distance(transform.position, startingPosition) > 0.5f && !finishedLanding)
        {
            transform.position = Vector2.Lerp(transform.position, startingPosition, Time.deltaTime);
            if (flyAnimation != null) flyAnimation.Play();
        }
        else
        {
            finishedLanding = true;

            if (flyAnimation != null) flyAnimation.Stop();
            if (idleAnimation != null) idleAnimation.Play();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!finishedLanding) return;

        HandleMovement();
    }

    void OnMouseOver()
    {
        Debug.Log("Mouse Over");
    }

    void OnMouseDown()
    {
        if (!isClicked && isInitialized)
        {
            isClicked = true;
            OnClicked();
            
            transform.localScale = currentScale * 0.8f;
            destroyEffect.Play();
            scared = true;
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
        //Destroy(gameObject);
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

        startingPosition = transform.position;
        targetPosition = transform.position;
        transform.position = new Vector3(startingPosition.x, startingPosition.y + 30f, -1);
        Debug.Log($"{GetType().Name} spawned at position {transform.position}.");
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