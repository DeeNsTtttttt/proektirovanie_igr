using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField, Min(0f)] private float defaultDuration = 0.08f;
    [SerializeField, Min(0f)] private float defaultMagnitude = 0.035f;

    private Vector3 startLocalPosition;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        startLocalPosition = transform.localPosition;
    }

    public static void Shake(float duration = -1f, float magnitude = -1f)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.PlayShake(duration, magnitude);
    }

    public void PlayShake(float duration = -1f, float magnitude = -1f)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        float activeDuration = duration > 0f ? duration : defaultDuration;
        float activeMagnitude = magnitude > 0f ? magnitude : defaultMagnitude;
        shakeRoutine = StartCoroutine(ShakeRoutine(activeDuration, activeMagnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float fade = 1f - Mathf.Clamp01(timer / duration);
            Vector2 offset = Random.insideUnitCircle * (magnitude * fade);
            transform.localPosition = startLocalPosition + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }

        transform.localPosition = startLocalPosition;
        shakeRoutine = null;
    }
}