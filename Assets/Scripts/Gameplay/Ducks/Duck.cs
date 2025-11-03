using UnityEngine;

/// <summary>
/// Good duck that players should click for points
/// Save this as: Assets/Scripts/Gameplay/Ducks/GoodDuck.cs/// </summary>
public class Duck : BaseCharacter
{
    [Header("Good Duck Settings")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private GameObject successTextPrefab; // Optional floating text

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Goose closestGoose;

    protected override void Start()
    {
        base.Start();

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("DeathArea")) return;

        GameManager.Instance.OnGoodDuckLost(this);

        DestroyDuck();
    }

    #region Abstract Implementation

    public override void OnClicked()
    {
        if (GameManager.Instance.CurrentState == GameState.LevelComplete) return;

        Debug.Log($"Good duck clicked... subtracted -1 life.");
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoodDuckClicked(this);
        }
        
        // Play success feedback
        PlaySuccessEffects();
        
        // Destroy duck
        DestroyDuck();
    }
    
    #endregion
    
    #region Virtual Overrides
    
    protected override void OnDuckSpawned()
    {
        base.OnDuckSpawned();
        
        // Good duck specific spawn behaviour
        // Could add spawn animation, sound, etc.
        
        // Ensure proper tag for identification
        gameObject.tag = "GoodDuck";
    }

    protected override void HandleMovement()
    {
        if (!finishedLanding) return;

        float minDistance = 0.1f;

        if (Vector2.Distance(transform.position, targetPosition) < minDistance)
        {
            Vector3 randomPosition = GameManager.Instance.spawner.GetRandomSpawnPosition();

            targetPosition = randomPosition;
        }
        else
        {
            if (closestGoose == null)
            {
                body.linearVelocity = Vector2.Lerp(transform.position, targetPosition, Time.deltaTime * 5) - new Vector2(transform.position.x, transform.position.y);
                FindClosestGoose();
            }
            else
            {
                Vector2 direction = closestGoose.transform.position - transform.position;

                body.linearVelocity = -direction.normalized * Time.deltaTime * 30;
            }
        }

        spriteRenderer.flipX = body.linearVelocityX > 0;
    }

    protected override void Update()
    {
        base.Update();

        if (!finishedLanding) collision.enabled = false;
        else collision.enabled = true;
    }

    #endregion

    #region Good Duck Specific Methods

    /// <summary>
    /// Play success effects when clicked
    /// </summary>
    private void PlaySuccessEffects()
    {
        // Particle effect
        if (successParticles != null)
        {
            ParticleSystem effect = Instantiate(successParticles, transform.position, transform.rotation);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Sound effect - use AudioManager for consistency
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDuckClickGood(transform.position);
        }

        // Floating score text (optional)
        if (successTextPrefab != null)
        {
            GameObject scoreText = Instantiate(successTextPrefab, transform.position, Quaternion.identity);
            // Assume the prefab has a script to handle floating animation
        }
    }
    
    private void FindClosestGoose()
    {
        Goose[] geese = FindObjectsByType<Goose>(FindObjectsSortMode.None);
        float closestDistance = 10;

        for (int i = 0; i < geese.Length; i++)
        {
            Goose goose = geese[i];

            if (!goose.finishedLanding) continue;

            float distance = Vector2.Distance(transform.position, goose.transform.position);

            if (distance < closestDistance)
            {
                closestGoose = goose;
                closestDistance = distance;
            }
        }
    }
    
    #endregion

}