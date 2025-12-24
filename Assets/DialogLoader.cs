using UnityEngine;

public static class DialogLoader
{
    /// <summary>
    /// Carga y asigna un DialogTree al DialogManager usando el evento estático.
    /// </summary>
    public static void LoadDialogStatic(DialogTree dialogTree, Transform entity = null)
    {
        // Busca el DialogManager en la escena
        var dialogManager = Object.FindObjectOfType<DialogManager>();
        if (dialogManager == null)
        {
            Debug.LogWarning("No se encontró DialogManager en la escena.");
            return;
        }

        dialogManager.StartDialog(dialogTree, entity);
    }
}
