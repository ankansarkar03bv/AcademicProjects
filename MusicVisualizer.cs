using UnityEngine;

public class MusicVisualizer : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Appearance")]
    public Material barMaterial;

    [Header("Visualizer")]
    public int numberOfBars = 16;
    public float radius = 10f;
    public float barWidth = 0.5f;
    public float maxHeight = 5f;
    public float sensitivity = 500f;

    [Header("Smoothness")]
    public float riseSpeed = 18f;
    public float fallSpeed = 6f;

    private Transform[] bars;
    private Renderer[] renderers;
    private Color[] colors;

    private Color[] colorPalette =
    {
        new Color(0.1f, 0.8f, 1f),   // Cyan
        new Color(0.1f, 0.4f, 1f),   // Blue
        new Color(0.3f, 0.1f, 1f),   // Violet
        new Color(0.7f, 0.1f, 1f),   // Purple

        new Color(1f, 0.1f, 0.7f),   // Pink
        new Color(1f, 0.1f, 0.3f),   // Red
        new Color(1f, 0.3f, 0.1f),   // Orange
        new Color(1f, 0.7f, 0.1f),   // Yellow

        new Color(0.2f, 1f, 0.4f),   // Green
        new Color(0.1f, 1f, 0.8f),   // Turquoise
        new Color(0.1f, 0.6f, 1f),   // Blue
        new Color(0.4f, 0.2f, 1f),   // Purple

        new Color(0.9f, 0.1f, 1f),   // Magenta
        new Color(1f, 0.2f, 0.5f),   // Pink
        new Color(1f, 0.5f, 0.1f),   // Orange
        new Color(0.2f, 1f, 0.6f)    // Green
    };

    void Start()
    {
        bars = new Transform[numberOfBars];
        renderers = new Renderer[numberOfBars];
        colors = new Color[numberOfBars];

        for (int i = 0; i < numberOfBars; i++)
        {
            GameObject bar =
                GameObject.CreatePrimitive(PrimitiveType.Cube);

            bar.name = "MusicBar_" + i;

            bar.transform.SetParent(transform);

            float angle =
                (360f / numberOfBars) * i;

            float radians =
                angle * Mathf.Deg2Rad;

            float x =
                Mathf.Sin(radians) * radius;

            float z =
                Mathf.Cos(radians) * radius;

            bar.transform.position =
                new Vector3(x, 0.5f, z);

            bar.transform.localScale =
                new Vector3(
                    barWidth,
                    1f,
                    barWidth
                );

            Renderer r =
                bar.GetComponent<Renderer>();

            renderers[i] = r;

            // Give each bar its own material
            if (barMaterial != null)
            {
                r.material = new Material(barMaterial);
            }

            // Assign unique color
            colors[i] =
                colorPalette[i % colorPalette.Length];

            SetMaterialColor(
                r,
                colors[i]
            );

            bars[i] = bar.transform;
        }
    }

    void Update()
    {
        if (audioSource == null)
            return;

        float[] spectrum =
            new float[64];

        audioSource.GetSpectrumData(
            spectrum,
            0,
            FFTWindow.Blackman
        );

        for (int i = 0; i < numberOfBars; i++)
        {
            // Get audio frequency strength
            float value =
                spectrum[i] * sensitivity;

            // Convert audio strength to height
            float targetHeight =
                Mathf.Clamp(
                    1f + value,
                    1f,
                    maxHeight
                );

            float currentHeight =
                bars[i].localScale.y;

            // Different speeds for rising and falling
            float speed;

            if (targetHeight > currentHeight)
                speed = riseSpeed;
            else
                speed = fallSpeed;

            float newHeight =
                Mathf.Lerp(
                    currentHeight,
                    targetHeight,
                    Time.deltaTime * speed
                );

            // Apply height
            bars[i].localScale =
                new Vector3(
                    barWidth,
                    newHeight,
                    barWidth
                );

            // Keep bars sitting on the floor
            Vector3 position =
                bars[i].position;

            bars[i].position =
                new Vector3(
                    position.x,
                    newHeight / 2f,
                    position.z
                );

            // KEEP ORIGINAL BRIGHT COLOR
            Color finalColor =
                colors[i];

            SetMaterialColor(
                renderers[i],
                finalColor
            );
        }
    }

    void SetMaterialColor(
        Renderer renderer,
        Color color
    )
    {
        Material material =
            renderer.material;

        // URP material color
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor(
                "_BaseColor",
                color
            );
        }

        // Standard material compatibility
        if (material.HasProperty("_Color"))
        {
            material.SetColor(
                "_Color",
                color
            );
        }

        // Small emission for neon glow
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor(
                "_EmissionColor",
                color * 0.4f
            );
        }
    }
}