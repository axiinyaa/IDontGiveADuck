using UnityEngine;

/// <summary>
/// Good duck that players should click for points
/// Save this as: Assets/Scripts/Gameplay/Ducks/GoodDuck.cs/// </summary>
public class Duck : BaseCharacter
{
    [Header("Good Duck Settings")]
    [SerializeField] private float runawaySpeed = 3;
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private GameObject successTextPrefab; // Optional floating text

    [Header("Visual Feedback")]

    private Goose closestGoose;

    protected override void Update()
    {
        base.Update();

        float distance = Vector2.Distance(transform.position, GameManager.Instance.pondArea.transform.position);

        if (distance > 3.3f && finishedLanding && !scared)
        {
            OnClicked();
        }
    }

    #region Abstract Implementation

    public override void OnClicked()
    {
        if (GameManager.Instance.CurrentState == GameState.LevelComplete) return;

        Debug.Log($"Good duck clicked... subtracted -1 life.");

        scared = true;
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoodDuckClicked(this);
        }
        
        // Play success feedback
        PlaySuccessEffects();
        
    }
    
    #endregion
    
    #region Virtual Overrides

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
            if (closestGoose == null || closestGoose.scared)
            {
                transform.position = Vector2.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
                FindClosestGoose();
            }
            else
            {
                Vector3 direction = closestGoose.transform.position - transform.position;

                transform.position += runawaySpeed * Time.deltaTime * -direction.normalized;
            }
        }

        
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