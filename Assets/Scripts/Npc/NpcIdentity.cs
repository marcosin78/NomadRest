using UnityEngine;
using System.Linq;

// Script encargado de gestionar la identidad y el estado de un NPC.
// Permite actualizar el estado del NPC según las condiciones del juego y la prioridad de los estados definidos en NpcStageScript.
public class NpcIdentity : MonoBehaviour
{
    public string npcType;      // Tipo de NPC (por ejemplo, "Cliente", "Camarero", etc.)
    public string currentState; // Estado actual del NPC

    // Actualiza el estado del NPC cada frame
    public void Update()
    {
        RefreshState();
    }

    // Busca el estado válido con mayor prioridad y lo asigna si corresponde
    public void RefreshState()
    {
        // Busca el script de configuración de estados (ajusta si tienes singleton o referencia directa)
        var npcStage = FindObjectOfType<NpcStageScript>();
        if (npcStage == null) return;

        // Obtiene la lista de estados posibles para este tipo de NPC
        var states = npcStage.GetStatesForNpcType(npcType);
        if (states == null) return;

        // Busca el estado con mayor prioridad cuyas condiciones se cumplan
        var validState = states
            .OrderByDescending(s => s.priority)
            .FirstOrDefault(s => s.conditions == null || s.conditions.All(cond => GameConditions.Instance.HasCondition(cond)));

        if (validState != null && currentState != validState.state)
        {
            SetState(validState.state);
        }
    }

    // Cambia el estado del NPC y muestra un mensaje en consola si hay cambio
    public void SetState(string newState)
    {
        if (currentState != newState)
        {
            Debug.Log($"[NpcIdentity] {npcType} cambia de estado: {currentState} → {newState}");
            currentState = newState;
        }
    }
}
