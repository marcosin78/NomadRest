using UnityEngine;
using System.Linq;

public class NpcIdentity : MonoBehaviour
{
    public string npcType;
    public string currentState;

    public void Update()
    {
        RefreshState();
    }

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

    public void SetState(string newState)
    {
        if (currentState != newState)
        {
            Debug.Log($"[NpcIdentity] {npcType} cambia de estado: {currentState} → {newState}");
            currentState = newState;
        }
    }
}
