using UnityEngine;

/// Script encargado de gestionar el comportamiento de una gota de cerveza.
/// Detecta si la gota cae dentro o fuera de un área específica (LiquidDetector)
/// y reproduce un sonido si la gota cae fuera al ser destruida.

public class BeerDrop : MonoBehaviour
{

    /// Fuente de audio para reproducir el sonido cuando la gota cae fuera del detector.
    public AudioSource outsideAudioSource; // Audio de caída fuera (no loop)

    // Referencia al detector de líquido actual donde se encuentra la gota.
    private LiquidDetector currentDetector = null;

    // Se llama cuando la gota entra en un trigger.
    // Si el trigger es un LiquidDetector, se guarda la referencia.
    // other: Collider con el que colisiona
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<LiquidDetector>() != null)
            currentDetector = other.GetComponent<LiquidDetector>();
    }

    // Se llama cuando la gota sale de un trigger.
    // Si el trigger es el mismo LiquidDetector, se elimina la referencia.
    // other: Collider del que sale
    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<LiquidDetector>() == currentDetector)
            currentDetector = null;
    }

    // Se llama al destruir la gota.
    // Si la gota no está dentro de un detector, reproduce el sonido de caída fuera.
    void OnDestroy()
    {
        // Si la gota NO está en el detector al destruirse, suena el audio de fuera
        if (currentDetector == null && outsideAudioSource != null)
        {
            if (!outsideAudioSource.isPlaying)
            {
                outsideAudioSource.loop = true;
                outsideAudioSource.Play();
            }
        }
    }
}

