using UnityEngine;
using System.Collections.Generic;

// Script encargado de gestionar la suciedad en el bar.
// Permite spawnear objetos y manchas de suciedad, limpiar con la fregona, reproducir efectos y sonidos,
// y calcular el porcentaje de limpieza para activar condiciones del juego.
public class DirtynessScript : MonoBehaviour
{
    [Header("Zona de spawn de suciedad")]
    public Collider spawnArea; // Asigna un BoxCollider o similar como plano invisible

    [Header("Prefabs de suciedad")]
    public GameObject[] dirtPrefabs; // Prefabs de objetos 3D (botellas, papeles, etc.)
    public GameObject[] stainPrefabs; // Prefabs de manchas (sprites o planos con sprite)
    public GameObject trashBin; // Contenedor de basura para recoger suciedad

    [Header("Partícula de limpieza")]
    public ParticleSystem cleanParticlePrefab; // Asigna un prefab de partícula de polvo

    [Header("Sonidos de limpieza")]
    public AudioClip cleanSoundClip; // Asigna un clip de sonido de limpieza

    public float cleanTime = 2f; // Segundos necesarios para limpiar la mancha
    private float cleaningTimer = 0f;
    private bool isCleaning = false;

    private Camera mainCamera;

    private int totalDirtSpawned = 0;
    private List<GameObject> spawnedDirt = new List<GameObject>();
    private bool barCleanedOnce = false;

    // Inicializa la referencia a la cámara principal
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Controla el proceso de limpieza y activa condiciones del tutorial si corresponde
    void Update()
    {
        if(trashBin == null)
        {
            trashBin = GameObject.FindWithTag("TrashBin");
        }

        if(Input.GetMouseButton(0))
        {
            StartCleaning();

            // Activa la condición de limpieza del tutorial si se ha limpiado el 10% por primera vez
            if (!barCleanedOnce && GetCleanPercentage() >= 10f && totalDirtSpawned > 0)
            {
                if (GameConditions.Instance != null && GameConditions.Instance.HasCondition("PlayerPendingOfCleaningSaloonWithTutorialBird"))
                {
                    barCleanedOnce = true;
                    GameConditions.Instance.SetCondition("PlayerHasCleanedSaloonWithTutorialBird", true);
                    Debug.Log("Bar cleaned 10% for the first time. Condition activated.");
                }
            }
        }
        else
        {
            ResetCleaning();
        }
    }

    // Spawnea un objeto de suciedad aleatorio en la zona
    public void SpawnRandomDirt()
    {
        if (spawnArea == null || dirtPrefabs.Length == 0) return;

        Vector3 pos = GetRandomPointInBounds(spawnArea.bounds);
        GameObject prefab = dirtPrefabs[Random.Range(0, dirtPrefabs.Length)];
        GameObject dirt = Instantiate(prefab, pos, Quaternion.identity, transform);
        spawnedDirt.Add(dirt);
        totalDirtSpawned++;
    }

    // Spawnea una mancha (sprite/plano) aleatoria en la zona
    public void SpawnRandomStain()
    {
        if (spawnArea == null || stainPrefabs.Length == 0) return;

        Vector3 pos = GetRandomPointInBounds(spawnArea.bounds);
        GameObject prefab = stainPrefabs[Random.Range(0, stainPrefabs.Length)];
        Quaternion rot = Quaternion.Euler(-90f, 0f, 0f); // Rotación -90 en X
        GameObject stain = Instantiate(prefab, pos, rot, transform);
        spawnedDirt.Add(stain);
        totalDirtSpawned++;
    }

    // Elimina y limpia un objeto de suciedad (llamar desde la fregona)
    public void CleanDirt(GameObject dirt)
    {
        if (spawnedDirt.Contains(dirt))
        {
            spawnedDirt.Remove(dirt);
            Destroy(dirt);
        }
    }

    // Devuelve un punto aleatorio dentro del área de spawn
    Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    // Calcula el porcentaje de limpieza del bar
    public float GetCleanPercentage()
    {
        if (totalDirtSpawned == 0) return 100f;
        float cleaned = totalDirtSpawned - spawnedDirt.Count;
        return cleaned / totalDirtSpawned * 100f;
    }

    // Resetea el estado de limpieza
    void ResetCleaning()
    {
        isCleaning = false;
        cleaningTimer = 0f;
    }

    // Inicia el proceso de limpieza si el jugador tiene la fregona equipada y está apuntando a suciedad
    void StartCleaning()
    {
        MopScript mop = FindObjectOfType<MopScript>();
        if (mop == null || !mop.grabbingMop)
        {
            ResetCleaning();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.collider.CompareTag("Dirt"))
            {
                if (!isCleaning)
                {
                    AudioManager.Instance.PlaySound(cleanSoundClip); // Solo al empezar
                    ParticleSystem particle = Instantiate(cleanParticlePrefab, hit.point, Quaternion.identity);
                    particle.Play();
                }

                isCleaning = true;
                cleaningTimer += Time.deltaTime;

                Debug.Log("Cleaning timer: " + cleaningTimer);

                if (cleaningTimer >= cleanTime)
                {
                    CleanDirt(hit.collider.gameObject);
                    cleaningTimer = 0f;
                    isCleaning = false;
                }
            }
            else
            {
                ResetCleaning();
            }
        }
        else
        {
            ResetCleaning();
        }
    }
}
