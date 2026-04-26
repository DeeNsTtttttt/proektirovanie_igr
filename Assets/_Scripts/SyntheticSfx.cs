using UnityEngine;

public static class SyntheticSfx
{
    private const int SampleRate = 44100;

    private static AudioClip shotClip;
    private static AudioClip reloadClip;
    private static AudioClip pickupClip;

    public static AudioClip GetShotClip()
    {
        if (shotClip == null)
        {
            shotClip = CreateTone("sfx_shot", 920f, 0.07f, 0.38f, 0.0006f, 0.045f);
        }

        return shotClip;
    }

    public static AudioClip GetReloadClip()
    {
        if (reloadClip == null)
        {
            reloadClip = CreateDualTone("sfx_reload", 420f, 620f, 0.12f, 0.16f, 0.32f);
        }

        return reloadClip;
    }

    public static AudioClip GetPickupClip()
    {
        if (pickupClip == null)
        {
            pickupClip = CreateTone("sfx_pickup", 1280f, 0.08f, 0.34f, 0.001f, 0.05f);
        }

        return pickupClip;
    }

    private static AudioClip CreateTone(
        string name,
        float frequency,
        float durationSeconds,
        float volume,
        float attackSeconds,
        float releaseSeconds)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(durationSeconds * SampleRate));
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);
            float envelope = Envelope(t, durationSeconds, attackSeconds, releaseSeconds);
            samples[i] = wave * envelope * volume;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateDualTone(
        string name,
        float frequencyA,
        float frequencyB,
        float firstPartSeconds,
        float secondPartSeconds,
        float volume)
    {
        float total = firstPartSeconds + secondPartSeconds;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(total * SampleRate));
        float[] samples = new float[sampleCount];

        int split = Mathf.RoundToInt(firstPartSeconds * SampleRate);
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float f = i < split ? frequencyA : frequencyB;
            float wave = Mathf.Sin(2f * Mathf.PI * f * t);
            float envelope = Envelope(t, total, 0.002f, 0.06f);
            samples[i] = wave * envelope * volume;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static float Envelope(float t, float total, float attack, float release)
    {
        float attackValue = attack <= 0f ? 1f : Mathf.Clamp01(t / attack);
        float tail = total - t;
        float releaseValue = release <= 0f ? 1f : Mathf.Clamp01(tail / release);
        return Mathf.Min(attackValue, releaseValue);
    }
}
