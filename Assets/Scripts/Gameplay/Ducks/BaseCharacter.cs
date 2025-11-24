using Unity.Collections;
using UnityEngine;
using Axiinyaa.Tweening;
using Unity.VisualScripting;
using UnityEngine.Rendering;

/// <summary>
/// Abstract base class for all duck types
/// Provides common functionality and enforces consistent interface
/// </summary>
public abstract class BaseCharacter : MonoBehaviour
{
    [Header("Base Duck Properties")]
    [SerializeField] protected int pointValue = 1;
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected float moveSpeed = 1f; // For future moving ducks
    [SerializeField] protected float flySpeed = 1f;
    [SerializeField] protected float minMoveDistance = 0.1f;
    
    [Header("Visual Feedback")]
    [SerializeField] protected ParticleSystem destroyEffect;
    [SerializeField] protected AudioClip[] clickSound;
    [SerializeField] protected AudioSource source;

    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected float soundVolume = 1;
    
    // Protected properties accessible to child classes
    protected float currentLifetime;
    protected bool isClicked = false;
    protected bool isInitialized = false;
    public bool finishedLanding = false;
    public bool scared = false;
    public Vector2 startingPosition;
    public Vector2 spawnPosition;
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
        collision = GetComponent<Collider2D>();

        currentScale = transform.localScale;
        lastPosition = transform.position;
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

        KeepSize();
        
        if (scared)
        {
            FlyAway();
            return;
        }

        if (!finishedLanding) FlyDown();

    }

    private void KeepSize()
    {
        transform.localScale = Vector2.Lerp(transform.localScale, currentScale, Time.deltaTime * 8);
    }

    private void FlyAway()
    {
        transform.position = Tweening.TweenPosition(transform.position, new Vector2(startingPosition.x, startingPosition.y + 40), 5, Easing.EaseOut);
        if (flyAnimation != null) flyAnimation.Play();
        if (scareAnimation != null) scareAnimation.Play();
    }
    
    private void FlyDown()
    {
        if (Vector2.Distance(transform.position, startingPosition) > 0.5f && !finishedLanding)
        {
            transform.position = Vector2.Lerp(transform.position, startingPosition, Time.deltaTime * flySpeed);
            if (flyAnimation != null) flyAnimation.Play();
        }
        else
        {
            finishedLanding = true;

            if (flyAnimation != null) flyAnimation.Stop();
            if (idleAnimation != null) idleAnimation.Play();
        }
    }

    Vector2 lastPosition;

    protected virtual void FixedUpdate()
    {
        if (!finishedLanding) return;

        HandleMovement();

        spriteRenderer.flipX = transform.position.x > lastPosition.x;
        lastPosition = transform.position;
    }


    void OnMouseDown()
    {
        if (!isClicked && isInitialized)
        {
            isClicked = true;
            OnClicked();
            
            transform.localScale = currentScale * 0.5f;
            destroyEffect.Play();
            scared = true;
            if (clickSound.Length > 0)
            {
                source.PlayOneShot(clickSound[Random.Range(0, clickSound.Length - 1)], soundVolume);
            }
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