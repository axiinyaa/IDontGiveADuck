using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimation : MonoBehaviour
{
    public string Name = "My Sprite Animation";
    public SpriteRenderer spriteRenderer;

    [Header("Animation")]
    public Sprite[] frames;
    public int framesPerSecond = 10;
    public bool loop = false;
    public bool interruptable = false;

    private bool playing;
    public bool Playing => playing;

    private int currentFrame;
    public int CurrentFrame => currentFrame;

    public void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Play()
    {
        if (!interruptable && playing) return;

        playing = true;
        currentFrame = 0;

        StartCoroutine(ProcessAnimation());
    }

    private IEnumerator ProcessAnimation()
    {
        while (playing)
        {
            spriteRenderer.sprite = frames[currentFrame];

            yield return new WaitForSeconds(1.0f / framesPerSecond);

            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                    continue;
                }

                playing = false;
            }

        }
    }
    
    public void Stop()
    {
        playing = false;
    }
}
