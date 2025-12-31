using UnityEngine;

[System.Serializable]
public class UnlockableDialog
{
    public string key; // Identificador único para desbloqueo (ej: "quest1", "friendship", etc.)
    public DialogTree dialogTree; // Asigna el ScriptableObject de diálogo
    public bool unlocked = false; // Estado de desbloqueo
}

public class UnlockDialogScript : MonoBehaviour
{
    public UnlockableDialog[] dialogs;

    // Llama a esto para desbloquear un diálogo por clave
    public void UnlockDialog(string key)
    {
        foreach (var dialog in dialogs)
        {
            if (dialog.key == key)
                dialog.unlocked = true;
        }
    }

    // Obtiene el primer diálogo desbloqueado (puedes adaptar la lógica)
    public DialogTree GetUnlockedDialog()
    {
        foreach (var dialog in dialogs)
        {
            if (dialog.unlocked)
                return dialog.dialogTree;
        }
        return null;
    }
    public void LockDialog(string key)
    {
    foreach (var dialog in dialogs)
    {
        if (dialog.key == key)
            dialog.unlocked = false;
    }
    }
    // Obtiene el único diálogo desbloqueado si todos los demás están bloqueados
public DialogTree GetExclusiveUnlockedDialog()
{
    UnlockableDialog unlockedDialog = null;
    int unlockedCount = 0;

    foreach (var dialog in dialogs)
    {
        if (dialog.unlocked)
        {
            unlockedDialog = dialog;
            unlockedCount++;
        }
    }

    // Solo retorna si hay exactamente uno desbloqueado
    if (unlockedCount == 1)
        return unlockedDialog.dialogTree;

    return null;
}


    // Puedes añadir métodos para bloquear, resetear, o elegir el diálogo según lógica personalizada
}
