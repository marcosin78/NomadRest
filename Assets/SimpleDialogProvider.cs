using UnityEngine;

public class SimpleDialogProvider : MonoBehaviour, IDialogProvider
{
    public string[] dialogLines;

    public string[] GetDialogLines()
    {
        return dialogLines;
    }
}
