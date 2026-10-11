using UnityEngine;

public class BeatParticleWave : MonoBehaviour
{
    public AudioSource audioSource;

    public int particleCount = 700;
    public float sphereRadius = 4f;
    public float height = 6f;

    public float sensitivity = 1200f;
    public float smoothness = 8f;

    public float minimumPulse = 0.9f;
    public float maximumPulse = 1.8f;
    public float pulseSpeed = 10f;

    public float movementSpeed = 1.5f;
    public float movementAmount = 0.12f;

    public float particleSize = 0.07f;

    public Color bassColor = new Color(1f, 0.1f, 0.2f);
    public Color midColor = new Color(0.7f, 0.1f, 1f);
    public Color highColor = new Color(0.1f, 0.7f, 1f);

    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;
    private Vector3[] basePositions;
    private int[] frequencyBands;
    private float currentPulse = 1f;

    void Start()
    {
        particleSystem = gameObject.AddComponent<ParticleSystem>();

        var main = particleSystem.main;
        main.maxParticles = particleCount;
        main.startLifetime = 999999f;
        main.startSpeed = 0f;
        main.startSize = particleSize;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particleSystem.emission;
        emission.enabled = false;

        var shape = particleSystem.shape;
        shape.enabled = false;

        particles = new ParticleSystem.Particle[particleCount];
        basePositions = new Vector3[particleCount];
        frequencyBands = new int[particleCount];

        CreateSphere();
    }

    void CreateSphere()
    {
        for (int i = 0; i < particleCount; i++)
        {
            float y = Random.Range(-1f, 1f);
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Mathf.Sqrt(1f - y * y);

            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);

            Vector3 position = new Vector3(x, y, z) * sphereRadius;

            basePositions[i] = position;

            particles[i].position =
                transform.position +
                position +
                Vector3.up * height;

            particles[i].startSize = particleSize;
            particles[i].remainingLifetime = 999999f;
            particles[i].startLifetime = 999999f;

            int band = i % 3;
            frequencyBands[i] = band;
            particles[i].startColor = GetFrequencyColor(band);
        }

        particleSystem.SetParticles(particles, particles.Length);
    }

    Color GetFrequencyColor(int band)
    {
        if (band == 0)
            return bassColor;

        if (band == 1)
            return midColor;

        return highColor;
    }

    void Update()
    {
        if (audioSource == null)
            return;

        float[] spectrum = new float[64];

        audioSource.GetSpectrumData(
            spectrum,
            0,
            FFTWindow.Blackman
        );

        float bass = 0f;
        float mid = 0f;
        float high = 0f;

        for (int i = 0; i < 8; i++)
            bass += spectrum[i];

        for (int i = 8; i < 24; i++)
            mid += spectrum[i];

        for (int i = 24; i < 50; i++)
            high += spectrum[i];

        bass = Mathf.Clamp01(bass * sensitivity);
        mid = Mathf.Clamp01(mid * sensitivity);
        high = Mathf.Clamp01(high * sensitivity);

        float overallAudio =
            bass * 0.5f +
            mid * 0.3f +
            high * 0.2f;

        float targetPulse =
            Mathf.Lerp(
                minimumPulse,
                maximumPulse,
                overallAudio
            );

        currentPulse =
            Mathf.Lerp(
                currentPulse,
                targetPulse,
                Time.deltaTime * pulseSpeed
            );

        for (int i = 0; i < particleCount; i++)
        {
            Vector3 basePosition = basePositions[i];

            float wave =
                Mathf.Sin(
                    Time.time * movementSpeed +
                    i * 0.13f
                );

            float wave2 =
                Mathf.Cos(
                    Time.time * movementSpeed * 0.7f +
                    i * 0.19f
                );

            Vector3 position =
                basePosition * currentPulse;

            position +=
                basePosition.normalized *
                wave *
                movementAmount;

            position.y +=
                wave2 * movementAmount;

            position.y += height;

            particles[i].position =
                transform.position +
                position;

            int band = frequencyBands[i];

            float frequencyStrength;

            if (band == 0)
                frequencyStrength = bass;
            else if (band == 1)
                frequencyStrength = mid;
            else
                frequencyStrength = high;

            particles[i].startSize =
                particleSize +
                frequencyStrength * 0.12f;

            Color baseColor =
                GetFrequencyColor(band);

            Color finalColor =
                Color.Lerp(
                    baseColor * 0.45f,
                    baseColor,
                    frequencyStrength
                );

            particles[i].startColor =
                finalColor;
        }

        particleSystem.SetParticles(
            particles,
            particles.Length
        );
    }
}