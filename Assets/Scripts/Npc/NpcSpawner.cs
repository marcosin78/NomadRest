using UnityEngine;

public class NpcSpawner : MonoBehaviour
{
    public GameObject[] npcPrefabs; // Array de prefabs de NPCs para elegir aleatoriamente
    public Transform spawnPoint; // Asignar en el inspector un punto de spawn fijo si se desea

    public bool allowSpawning = false;
    private float spawnCooldown = 10f;
    private float lastSpawnTime = -10f;

    void Start()
    {
        
    }

    void Update()
    {
        // Solo permite spawnear si la hora global es 9:00 o más
        if (ClockScript.Instance != null && ClockScript.Instance.OpenBarTime)
        {
            allowSpawning = true;
        }
        else
        {
            allowSpawning = false;
        }

        if (!allowSpawning) return;

        if (Time.time - lastSpawnTime >= spawnCooldown)
        {
            var waypoints = FindObjectsOfType<WaypointScript>();
            foreach (var wp in waypoints)
            {
                if (wp.IsAvailable)
                {
                    SpawnNpcAtWaypoint(wp);
                    lastSpawnTime = Time.time;
                    break;
                }
            }
        }
    }

    void SpawnNpcAtWaypoint(WaypointScript waypoint)
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0)
        {
            Debug.LogWarning("No hay NPC prefabs asignados en el array.");
            return;
        }

        // Elige un prefab aleatorio del array
        int randomIndex = Random.Range(0, npcPrefabs.Length);
        GameObject prefab = npcPrefabs[randomIndex];

        // Instancia el NPC en la posición del spawnPoint
        var npcObj = Instantiate(prefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

        // Asigna el waypoint como destino al NPC
        var npcScript = npcObj.GetComponent<NPCWalkingScript>();
        if (npcScript != null)
        {
            npcScript.provider = waypoint;
        }
    }
}
