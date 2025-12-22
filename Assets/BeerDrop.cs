using UnityEngine;

public class BeerDrop : MonoBehaviour
{

    public AudioSource outsideAudioSource; // Audio de caída fuera (no loop)
    private LiquidDetector currentDetector = null;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<LiquidDetector>() != null)
            currentDetector = other.GetComponent<LiquidDetector>();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<LiquidDetector>() == currentDetector)
            currentDetector = null;
    }
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
    
