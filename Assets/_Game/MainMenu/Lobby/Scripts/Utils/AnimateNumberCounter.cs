using System.Collections;
using TMPro;
using UnityEngine;

public class AnimateNumberCounter
{
    
    private static Coroutine CountingCoroutine;

    public static void UpdateText(MonoBehaviour behaviour, TextMeshProUGUI text, float newValue, float duration)
    {
        if (CountingCoroutine != null)
        {
            behaviour.StopCoroutine(CountingCoroutine);
        }

        CountingCoroutine = behaviour.StartCoroutine(CountText(text, newValue, duration));
    }

    private static IEnumerator CountText(TextMeshProUGUI text, float newValue, float duration)
    {
        // Get previous value dynamically from text instead of relying on a static variable
        float previousValue;
        if (!float.TryParse(text.text, out previousValue))
        {
            previousValue = newValue; // Fallback in case parsing fails
        }

        var elapsed = 0f;
        var waitTime = 1f / 30f; // 30 FPS

        while (elapsed < duration)
        {
            elapsed += waitTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smoothly interpolate between values
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(previousValue, newValue, t));
            text.SetText(currentValue.ToString("N0"));

            yield return new WaitForSeconds(waitTime);
        }

        // Ensure final value is set
        text.SetText(newValue.ToString("N0"));
    }
}
