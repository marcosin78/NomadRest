using UnityEngine;

// Script encargado de detectar y contar las gotas de cerveza que caen en el recipiente durante el minijuego.
// Controla el porcentaje de llenado, reproduce el audio al recibir gotas y pausa el sonido tras un tiempo sin recibir nuevas gotas.
public class LiquidDetector : MonoBehaviour
{
    private int totalDrops = 0;         // Número total de gotas esperadas en el minijuego
    private int dropsCollected = 0;     // Número de gotas recogidas hasta el momento

    public AudioSource insideAudioSource;   // Fuente de audio para el sonido de llenado
    public float fillPauseDelay = 0.5f;     // Tiempo en segundos para pausar el audio tras la última gota

    private float fillPauseTimer = 0f;      // Temporizador para pausar el audio

    // Devuelve el porcentaje de llenado respecto al total de gotas esperadas
    public float GetFillPercent()
    {
        if (totalDrops == 0) return 0f;
        return (float)dropsCollected / totalDrops;
    }

    // Detecta la entrada de una gota de cerveza (BeerDrop) en el trigger
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

    // Actualiza el temporizador y pausa el audio si no han caído gotas recientemente
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

    // Establece el número total de gotas esperadas y reinicia el contador de recogidas
    public void setTotalDrops(int total)
    {
        totalDrops = total;
        dropsCollected = 0;
    }

    // Inicializa la fuente de audio al iniciar el script
    void Start()
    {
        if (insideAudioSource != null)
        {
            insideAudioSource.loop = true;
            insideAudioSource.Stop();
        }
    }
}
