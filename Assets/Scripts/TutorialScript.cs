using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public class NpcPositionsConfig
{
    public List<NpcPositionsEntry> npcPositions;
}

[Serializable]
public class NpcPositionsEntry
{
    public string npcType;
    public List<NpcStatePositions> states;
}

[Serializable]
public class NpcStatePositions
{
    public string state;
    public List<int> positions;
}

public class TutorialScript : MonoBehaviour
{
    public Transform teleportPositionsParent; // Asigna el EmptyObject en el inspector
    public ParticleSystem teleportParticlesPrefab; // Asigna el prefab de partículas en el inspector
    public float particleDuration = 1.5f; // Duración de la animación de partículas
    public string positionsConfigFile = "NpcPositionsConfig.json";
    private NpcPositionsConfig npcPositionsConfig;

    // Guarda el último estado de cada NPC para detectar cambios
    private Dictionary<Transform, string> npcStateMemory = new Dictionary<Transform, string>();

    private NpcIdentity identity;
    private string lastState = "";

    void Awake()
    {
        // Busca el parent por tag si no está asignado
        if (teleportPositionsParent == null)
        {
            GameObject parentObj = GameObject.FindGameObjectWithTag("PositionsParent");
            if (parentObj != null)
            {
                teleportPositionsParent = parentObj.transform;
            }
            else
            {
                Debug.LogWarning("No se encontró ningún objeto con el tag 'PositionsParent'.");
            }
        }
        identity = GetComponent<NpcIdentity>();
        if (identity == null)
            Debug.LogWarning("No se encontró NpcIdentity en este GameObject.");
        LoadPositionsConfig();
    }

    void LoadPositionsConfig()
    {
        // Usa la nueva ruta relativa a la carpeta Assets/Scripts
        string path = Path.Combine(Application.dataPath, "Scripts", positionsConfigFile);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            npcPositionsConfig = JsonUtility.FromJson<NpcPositionsConfig>(json);
        }
        else
        {
            Debug.LogWarning("NpcPositionsConfig.json not found at: " + path);
            npcPositionsConfig = new NpcPositionsConfig { npcPositions = new List<NpcPositionsEntry>() };
        }
    }

    void Update()
    {
        if (teleportPositionsParent == null || identity == null) return;

        string npcType = identity.npcType;
        string currentState = identity.currentState;

        if (currentState != lastState)
        {
            TeleportNpcByState(transform, npcType, currentState);
            lastState = currentState;
        }
    }

    /// <summary>
    /// Teletransporta el NPC según su estado usando el JSON.
    /// </summary>
    public void TeleportNpcByState(Transform npc, string npcType, string state)
    {
        if (npcPositionsConfig == null || npcPositionsConfig.npcPositions == null)
        {
            Debug.LogWarning("npcPositionsConfig no cargado.");
            return;
        }

        var npcEntry = npcPositionsConfig.npcPositions.Find(e => e.npcType == npcType);
        if (npcEntry == null)
        {
            Debug.LogWarning($"No hay posiciones para {npcType}");
            return;
        }

        var stateEntry = npcEntry.states.Find(s => s.state == state);
        if (stateEntry == null)
        {
            Debug.LogWarning($"No hay posiciones para {npcType} en estado {state}");
            return;
        }

        if (stateEntry.positions.Count > 0)
        {
            TeleportWithParticles(npc, stateEntry.positions[0]);
            Debug.Log($"Teletransportado {npcType} a posición {stateEntry.positions[0]} para estado {state}");
        }
    }

    /// <summary>
    /// Llama a esto desde el DialogNode para hacer el efecto y teletransportar.
    /// </summary>
    public void TeleportWithParticles(Transform target, int positionIndex)
    {
        StartCoroutine(TeleportWithParticlesCoroutine(target, positionIndex));
    }

    private IEnumerator TeleportWithParticlesCoroutine(Transform target, int positionIndex)
    {
        // Instancia las partículas en la posición actual del target
        if (teleportParticlesPrefab != null && target != null)
        {
            ParticleSystem particles = Instantiate(teleportParticlesPrefab, target.position, Quaternion.identity);
            particles.Play();
            yield return new WaitForSeconds(particleDuration);
            Destroy(particles.gameObject);
        }

        // Teletransporta después de la animación
        TeleportToPosition(target, positionIndex);
    }

    /// <summary>
    /// Teletransporta el objeto dado a la posición del hijo con el índice indicado.
    /// </summary>
    public void TeleportToPosition(Transform target, int positionIndex)
    {
        if (teleportPositionsParent == null)
        {
            Debug.LogWarning("No se ha asignado teleportPositionsParent.");
            return;
        }

        if (positionIndex < 0 || positionIndex >= teleportPositionsParent.childCount)
        {
            Debug.LogWarning("Índice de posición fuera de rango.");
            return;
        }

        Transform targetPosition = teleportPositionsParent.GetChild(positionIndex);
        target.position = targetPosition.position;
        target.rotation = targetPosition.rotation; // Opcional: también rota al objetivo
    }
}
