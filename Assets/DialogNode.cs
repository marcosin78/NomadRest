[System.Serializable]
public class DialogNode
{
    public string text;
    public DialogChoice[] choices;
    public int nextNodeIndex = -1; // -1 para terminar el diálogo
}

[System.Serializable]
public class DialogChoice
{
    public string choiceText;
    public int nextNodeIndex = -1; // -1 si termina el diálogo
    // Puedes añadir aquí efectos, estadísticas, etc.
}
