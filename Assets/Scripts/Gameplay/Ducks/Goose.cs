using UnityEngine;

/// <summary>
/// Decoy duck that penalises players when clicked
/// </summary>
public class Goose : BaseCharacter
{
    [Header("Decoy Duck Settings")]
    [SerializeField] private ParticleSystem penaltyParticles;
    [SerializeField] private AudioClip penaltySound;
    [SerializeField] private int timePenalty = 3; // seconds to subtract
    [SerializeField] private GameObject penaltyTextPrefab; // Optional floating text
    
    [Header("Visual Distinction")]
    [SerializeField] private bool subtleVisualDifference = true; // Make it harder to distinguish
    
    #region Initialization Override
    
    /// <summary>
    /// Initialise decoy duck with custom properties
    /// </summary>
    public override void Initialize(float customLifetime = -1, int customPointValue = -1)
    {
        base.Initialize(customLifetime, customPointValue);
    
    }
    
    #endregion
    
    #region Abstract Implementation
    
    public override void OnClicked()
    {
        
        // Notify game manager about penalty
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGooseClicked(this);
        }
        
        // Play penalty feedback
        PlayPenaltyEffects();
    }
    
    #endregion
    
    #region Virtual Overrides
    
    protected override void OnDuckSpawned()
    {
        base.OnDuckSpawned();
    
        // Optional: Add subtle behavioural differences
        if (subtleVisualDifference)
        {
            AddSubtleBehavioralDifferences();
        }
    }
    
    protected override void HandleMovement()
    {
        if (!finishedLanding) return;
        if (scared) return;

        if (Vector2.Distance(transform.position, targetPosition) < minMoveDistance)
        {
            Vector3 randomPosition = GameManager.Instance.spawner.GetRandomSpawnPosition();

            targetPosition = randomPosition;
        }
        else
        {
            transform.position = Vector2.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }
    }
    
    #endregion
    
    #region Decoy Duck Specific Methods
    
    /// <summary>
    /// Play penalty effects when clicked
    /// </summary>
    private void PlayPenaltyEffects()
    {   
        // Floating penalty text (optional)
        if (penaltyTextPrefab != null)
        {
            GameObject penaltyText = Instantiate(penaltyTextPrefab, transform.position, Quaternion.identity);
            // Assume the prefab has a script to handle floating animation
        } 
    }
    
    /// <summary>
    /// Add subtle differences to make decoys learnable but not obvious
    /// </summary>
    private void AddSubtleBehavioralDifferences()
    {
        // Example: Decoy ducks spawn slightly closer to edges
        // Or have slightly different timing patterns
        // This gives observant players a chance to learn the differences
        
        // Slight scale difference (barely noticeable)
        float scaleVariation = Random.Range(0.95f, 1.05f);
        transform.localScale *= scaleVariation;
    }
    
    #endregion
}