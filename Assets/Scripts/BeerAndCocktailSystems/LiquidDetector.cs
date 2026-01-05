using UnityEngine;

public class LiquidDetector : MonoBehaviour
{
    private int totalDrops = 0;
    private int dropsCollected = 0;

    public AudioSource insideAudioSource;
    public float fillPauseDelay = 0.5f; // Tiempo en segundos para pausar el audio tras la última gota

    private float fillPauseTimer = 0f;

    public float GetFillPercent()
    {
        if (totalDrops == 0) return 0f;
        return (float)dropsCollected / totalDrops;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BeerDrop"))
        {
            dropsCollected++; // Suma al total recogido

            // Si el audio no está sonando, reanúdalo o ponlo a sonar
            if (insideAudioSource != null && !insideAudioSource.isPlaying)
            {
                if (insideAudioSource.time > 0f)
                    insideAudioSource.UnPause();
                else
                    insideAudioSource.Play();
            }

            // Reinicia el temporizador cada vez que entra una gota
            fillPauseTimer = fillPauseDelay;

            Destroy(other.gameObject);
        }
    }

    void Update()
    {
        // Si el audio está sonando, cuenta hacia atrás
        if (insideAudioSource != null && insideAudioSource.isPlaying)
        {
            fillPauseTimer -= Time.deltaTime;
            if (fillPauseTimer <= 0f)
            {
                insideAudioSource.Pause();
            }
        }
    }

    public void setTotalDrops(int total)
    {
        totalDrops = total;
        dropsCollected = 0;
    }

    void Start()
    {
        if (insideAudioSource != null)
        {
            insideAudioSource.loop = true;
            insideAudioSource.Stop();
        }
    }
}
