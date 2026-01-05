using UnityEngine;

public class NpcSpawner : MonoBehaviour
{

    public GameObject npcPrefab;
    public Transform spawnPoint; //Asignar en el inspector un punto de spawn fijo si se desea

    public bool allowSpawning = false;
    private float spawnCooldown = 10f;
    private float lastSpawnTime = -10f; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    
    // Update is called once per frame
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
        // Instancia el NPC en la posición del waypoint
        var npcObj = Instantiate(npcPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

        // Asigna el waypoint como destino al NPC
        var npcScript = npcObj.GetComponent<NPCWalkingScript>();
        if (npcScript != null)
        {
            npcScript.provider = waypoint;
        }
    }
}
