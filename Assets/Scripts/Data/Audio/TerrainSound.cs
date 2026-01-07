using UnityEngine;

// Script encargado de reproducir sonidos de pasos según el tipo de terreno bajo el jugador.
// Detecta el terreno usando raycast y reproduce el clip correspondiente (hierba, madera, piedra).
public class TerrainSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip grassClip;
    public AudioClip woodClip;
    public AudioClip stoneClip;
    public LayerMask terrainMask;
    public float stepInterval = 0.5f;

    private float stepTimer = 0f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Obtén la referencia al PlayerController
        PlayerController player = GetComponent<PlayerController>();
        bool isWalking = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        if (isWalking)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayStepSound();
                stepTimer = 0f;
                //Debug.Log  ("Step sound played");
            }
        }
        else
        {
            stepTimer = stepInterval; // Listo para el siguiente paso
        }
    }

    // Reproduce el sonido de paso según el terreno detectado bajo el jugador
    void PlayStepSound()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(transform.position, Vector3.down * 5f, Color.red, 1f);
        if (Physics.Raycast(ray, out hit, 5f, terrainMask))
        {
            string tag = hit.collider.tag;
            switch (tag)
            {
                case "Grass":
                    audioSource.PlayOneShot(grassClip);
                    break;
                case "Wood":
                    audioSource.PlayOneShot(woodClip);
                    break;
                case "Stone":
                    audioSource.PlayOneShot(stoneClip);
                    break;
                default:
                    // Sonido por defecto o silencio
                    break;
            }
        }
    }
}
