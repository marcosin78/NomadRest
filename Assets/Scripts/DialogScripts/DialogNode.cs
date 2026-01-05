using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogNode
{
    [TextArea]
    public string text;
    public DialogChoice[] choices; // Opciones de decisión para este nodo
    public int nextNodeIndex = -1; // -1 para terminar el diálogo
    public DialogTree nextDialogTree; // Nuevo árbol de diálogo para cambiar
    public string setNpcStateIfCondition; // Nuevo estado del NPC si se cumple la condición
}

[System.Serializable]
public class DialogChoice
{
    public string choiceText;      // Texto que aparece en el botón
    public int nextNodeIndex = -1; // -1 si termina el diálogo
    public string unlockKey;       // Clave del diálogo a desbloquear (opcional)
    public int requiredMoney = 0; // Dinero requerido para seleccionar esta opción
}
