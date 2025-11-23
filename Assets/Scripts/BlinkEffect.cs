using System.Collections;
using UnityEngine;

public class BlinkEffect : MonoBehaviour
{
    [Header("Blink Settings")]
    [Tooltip("Time in seconds for one blink cycle")]
    public float blinkInterval = 0.5f;

    [Tooltip("Should blink automatically on start")]
    public bool blinkOnStart = true;

    [Tooltip("Number of times to blink (0 = infinite)")]
    public int blinkCount = 0;

    [Header("Blink Method")]
    [Tooltip("Choose blink method: Toggle, Fade, or Color")]
    public BlinkMethod method = BlinkMethod.Toggle;

    [Header("Fade Settings (for Fade method)")]
    [Range(0f, 1f)]
    public float minAlpha = 0f;
    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    [Header("Color Settings (for Color method)")]
    public Color blinkColor = Color.red;
    private Color originalColor;

    public enum BlinkMethod
    {
        Toggle,     // On/Off toggle
        Fade,       // Smooth fade in/out
        Color       // Change color
    }

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;
    private bool isBlinking = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("BlinkEffect requires a SpriteRenderer component!");
            enabled = false;
            return;
        }

        originalColor = spriteRenderer.color;

        if (blinkOnStart)
        {
            StartBlinking();
        }
    }

    /// <summary>
    /// Start blinking effect
    /// </summary>
    public void StartBlinking()
    {
        if (isBlinking) return;

        isBlinking = true;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        switch (method)
        {
            case BlinkMethod.Toggle:
                blinkCoroutine = StartCoroutine(BlinkToggle());
                break;
            case BlinkMethod.Fade:
                blinkCoroutine = StartCoroutine(BlinkFade());
                break;
            case BlinkMethod.Color:
                blinkCoroutine = StartCoroutine(BlinkColor());
                break;
        }
    }

    /// <summary>
    /// Stop blinking effect
    /// </summary>
    public void StopBlinking()
    {
        isBlinking = false;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // Reset to original state
        spriteRenderer.enabled = true;
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// Blink once (useful for feedback)
    /// </summary>
    public void BlinkOnce()
    {
        StartCoroutine(BlinkOnceCoroutine());
    }

    private IEnumerator BlinkOnceCoroutine()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = blinkColor;
        yield return new WaitForSeconds(blinkInterval / 2);
        spriteRenderer.color = original;
    }

    // METHOD 1: Toggle On/Off
    private IEnumerator BlinkToggle()
    {
        int blinks = 0;

        while (isBlinking)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);

            blinks++;
            if (blinkCount > 0 && blinks >= blinkCount * 2)
            {
                spriteRenderer.enabled = true;
                isBlinking = false;
                break;
            }
        }
    }

    // METHOD 2: Fade In/Out
    private IEnumerator BlinkFade()
    {
        int blinks = 0;
        bool fadingOut = true;

        while (isBlinking)
        {
            float elapsedTime = 0f;
            Color startColor = spriteRenderer.color;
            float startAlpha = fadingOut ? maxAlpha : minAlpha;
            float endAlpha = fadingOut ? minAlpha : maxAlpha;

            while (elapsedTime < blinkInterval)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / blinkInterval);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            fadingOut = !fadingOut;

            if (!fadingOut)
            {
                blinks++;
                if (blinkCount > 0 && blinks >= blinkCount)
                {
                    spriteRenderer.color = originalColor;
                    isBlinking = false;
                    break;
                }
            }
        }
    }

    // METHOD 3: Color Change
    private IEnumerator BlinkColor()
    {
        int blinks = 0;
        bool useBlinkColor = true;

        while (isBlinking)
        {
            spriteRenderer.color = useBlinkColor ? blinkColor : originalColor;
            yield return new WaitForSeconds(blinkInterval);

            useBlinkColor = !useBlinkColor;

            if (!useBlinkColor)
            {
                blinks++;
                if (blinkCount > 0 && blinks >= blinkCount)
                {
                    spriteRenderer.color = originalColor;
                    isBlinking = false;
                    break;
                }
            }
        }
    }

    void OnDisable()
    {
        StopBlinking();
    }
}