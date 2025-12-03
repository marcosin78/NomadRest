using UnityEngine;
using System.Collections.Generic;

public class DirtynessScript : MonoBehaviour
{
    [Header("Zona de spawn de suciedad")]
    public Collider spawnArea; // Asigna un BoxCollider o similar como plano invisible

    [Header("Prefabs de suciedad")]
    public GameObject[] dirtPrefabs; // Prefabs de objetos 3D (botellas, papeles, etc.)
    public GameObject[] stainPrefabs; // Prefabs de manchas (sprites o planos con sprite)
    public GameObject trashBin; // Contenedor de basura para recoger suciedad

    public float cleanTime = 2f; // Segundos necesarios para limpiar la mancha
    private float cleaningTimer = 0f;
    private bool isCleaning = false;


    private Camera mainCamera;

    private int totalDirtSpawned = 0;

    private List<GameObject> spawnedDirt = new List<GameObject>();

    /// <summary>
    /// Spawnea un objeto de suciedad aleatorio en la zona.
    /// </summary>

    void Start()
    {
        mainCamera = Camera.main;
    }
    void Update()
    {
        if(trashBin == null)
        {
            trashBin = GameObject.FindWithTag("TrashBin");
        }

        if(Input.GetMouseButton(0))
        {
            StartCleaning();
        }else
        {
            ResetCleaning();
        }


    }
    public void SpawnRandomDirt()
    {
        if (spawnArea == null || dirtPrefabs.Length == 0) return;

        Vector3 pos = GetRandomPointInBounds(spawnArea.bounds);
        GameObject prefab = dirtPrefabs[Random.Range(0, dirtPrefabs.Length)];
        GameObject dirt = Instantiate(prefab, pos, Quaternion.identity, transform);
        spawnedDirt.Add(dirt);
        totalDirtSpawned++;
    }

    /// <summary>
    /// Spawnea una mancha (sprite/plano) aleatoria en la zona.
    /// </summary>
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
  
    /// Elimina y limpia un objeto de suciedad (llamar desde la fregona).

    public void CleanDirt(GameObject dirt)
    {
        if (spawnedDirt.Contains(dirt))
        {
            spawnedDirt.Remove(dirt);
            Destroy(dirt);
        }
    }


    /// Devuelve un punto aleatorio dentro del área de spawn.
 
    Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public float GetCleanPercentage()
    {
        if (totalDirtSpawned == 0) return 100f;
        float cleaned = totalDirtSpawned - spawnedDirt.Count;
        return cleaned / totalDirtSpawned * 100f;
    }

    void ResetCleaning()
    {
        isCleaning = false;
        cleaningTimer = 0f;
    }

    void StartCleaning()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 3f))
            {
                //Aqui se puede añadir más lógica para detectar si el objeto es limpiable
                if (hit.collider.gameObject == GameObject.FindWithTag("Dirt"))
                {
                    isCleaning = true;
                    cleaningTimer += Time.deltaTime;
                    Debug.Log("Cleaning timer: " + cleaningTimer);
                    if (cleaningTimer >= cleanTime)
                    {
                        CleanDirt(GameObject.FindWithTag("Dirt"));
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
