using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NpcSpawnScheduleCondition
{
    public int day;
    public int hour;
    public List<NpcSpawnCondition> conditions = new List<NpcSpawnCondition>();
}

[System.Serializable]
public class NpcSpawnCondition
{
    public string conditionKey; // La clave de la condición en GameConditions
    public bool requiredValue = true; // El valor que debe tener la condición para permitir el spawn
}

[System.Serializable]
public class NpcSpawnInfo
{
    public GameObject npcPrefab;
    public Vector3 spawnPosition;
    public List<NpcSpawnScheduleCondition> schedules = new List<NpcSpawnScheduleCondition>();

    [HideInInspector] public HashSet<string> spawnedFlags = new HashSet<string>();
}

public class SpecialNpcSpawner : MonoBehaviour
{
    [Header("Configuración de NPCs especiales")]
    public List<NpcSpawnInfo> npcSpawns = new List<NpcSpawnInfo>();

    void Update()
    {
        if (ClockScript.Instance == null) return;

        int currentDay = ClockScript.Instance.Day;
        int currentHour = ClockScript.Instance.Hour;

        foreach (var info in npcSpawns)
        {
            foreach (var schedule in info.schedules)
            {
                string flag = $"{schedule.day}_{schedule.hour}";
                if (info.spawnedFlags.Contains(flag)) continue;

                // ¿Coincide el día y la hora?
                if (schedule.day == currentDay && schedule.hour == currentHour)
                {
                    // ¿Cumple todas las condiciones?
                    bool allConditionsMet = true;
                    foreach (var cond in schedule.conditions)
                    {
                        if (GameConditions.Instance == null ||
                            GameConditions.Instance.HasCondition(cond.conditionKey) != cond.requiredValue)
                        {
                            allConditionsMet = false;
                            break;
                        }
                    }

                    if (allConditionsMet)
                    {
                        GameObject npc = Instantiate(info.npcPrefab, transform.position, Quaternion.identity);
                        var movement = npc.GetComponent<SpecialNPCMovement>();
                        if (movement != null)
                        {
                            movement.exitTarget = transform.position; // El lugar de spawn es el exitTarget
                            // Opcional: también puedes asignar aquí el initialTarget si lo tienes en info.spawnPosition
                            movement.initialTarget = info.spawnPosition;
                        }
                        info.spawnedFlags.Add(flag);
                        Debug.Log($"NPC {info.npcPrefab.name} spawneado en día {schedule.day} a las {schedule.hour}:00");
                    }
                }
            }
        }
    }
}
