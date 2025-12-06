using UnityEngine;

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

    void PlayStepSound()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(transform.position, Vector3.down * 5f, Color.red, 1f);
        if (Physics.Raycast(ray, out hit, 5f, terrainMask))
        {
            string tag = hit.collider.tag;
            Debug.Log  ("Hit terrain with tag: " + tag);
            switch (tag)
            {
                case "Grass":
                    audioSource.PlayOneShot(grassClip);
                    Debug.Log  ("Playing grass sound");
                    break;
                case "Wood":
                    audioSource.PlayOneShot(woodClip);
                    Debug.Log  ("Playing wood sound");
                    break;
                case "Stone":
                    audioSource.PlayOneShot(stoneClip);
                    Debug.Log  ("Playing stone sound");
                    break;
                default:
                    // Sonido por defecto o silencio
                    Debug.Log  ("Playing default sound or silence");
                    break;
            }
        }
    }
}
