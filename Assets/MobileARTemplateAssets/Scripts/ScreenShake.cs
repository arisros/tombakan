using System.Collections;
using UnityEngine;

/// <summary>
/// Simple camera shake for hit feedback juice (Phase 3).
/// Attach to the AR Camera or its parent; call Shake() from GameManager on hit events.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake I;

    [Header("Shake Settings")]
    public float correctHitMagnitude = 0.04f;
    public float wrongHitMagnitude   = 0.08f;
    public float duration            = 0.18f;

    Coroutine shakeRoutine;

    void Awake()
    {
        I = this;
    }

    public void ShakeOnCorrect() => TriggerShake(correctHitMagnitude, duration);
    public void ShakeOnWrong()   => TriggerShake(wrongHitMagnitude,   duration);

    public void TriggerShake(float magnitude, float shakeDuration)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(DoShake(magnitude, shakeDuration));
    }

    IEnumerator DoShake(float magnitude, float shakeDuration)
    {
        Vector3 origin = transform.localPosition;
        float elapsed  = 0f;

        while (elapsed < shakeDuration)
        {
            float t     = elapsed / shakeDuration;
            float damp  = Mathf.Lerp(1f, 0f, t); // dampen toward end
            Vector3 offset = Random.insideUnitSphere * magnitude * damp;
            transform.localPosition = origin + offset;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = origin;
        shakeRoutine = null;
    }
}
