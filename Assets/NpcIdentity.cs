using UnityEngine;

public class NpcIdentity : MonoBehaviour
{
    public string npcType;
    public string currentState;
    public void SetState(string newState)
    {
        Debug.Log($"[NpcIdentity] {npcType} cambia de estado: {currentState} → {newState}");
        currentState = newState;
    }
}
