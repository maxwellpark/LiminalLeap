using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void StopShake()
    {
        if (shakeCoroutine == null)
        {
            return;
        }

        StopCoroutine(shakeCoroutine);
        shakeCoroutine = null;
        transform.localPosition = originalPosition;
    }

    public void Shake(CameraShakeSettings settings)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        if (settings.Duration <= 0)
        {
            shakeCoroutine = StartCoroutine(ShakeCoroutine(settings.Amplitude));
        }
        else
        {
            shakeCoroutine = StartCoroutine(ShakeCoroutine(settings.Amplitude, settings.Duration));
        }
    }

    private IEnumerator ShakeCoroutine(float amplitude)
    {
        while (true)
        {
            var shakePosition = Random.insideUnitSphere * amplitude;
            transform.localPosition = originalPosition + shakePosition;
            yield return null;
        }
    }

    private IEnumerator ShakeCoroutine(float amplitude, float duration)
    {
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            var shakePosition = Random.insideUnitSphere * amplitude;
            transform.localPosition = originalPosition + shakePosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}

[Serializable]
public class CameraShakeSettings
{
    [Tooltip("The strength of the shake")]
    public float Amplitude;

    [Tooltip("How long the shake lasts for")]
    public float Duration;
}
