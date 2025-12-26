using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class NpcStatesConfig
{
    public List<NpcTypeConfig> npcTypes;
}

[System.Serializable]
public class NpcTypeConfig
{
    public string npcType;
    public List<NpcStateConfig> states;
}

[System.Serializable]
public class NpcStateConfig
{
    public string state;
    public List<string> conditions;
    public string dialogTreeName;
}

public class NpcStageScript : MonoBehaviour
{
    public string configFileName = "NpcStatesConfig.json";
    public NpcStatesConfig npcStatesConfig;

    void Awake()
    {
        LoadNpcStatesConfig();
    }

    void LoadNpcStatesConfig()
    {
        string path = Path.Combine(Application.dataPath, configFileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            npcStatesConfig = JsonUtility.FromJson<NpcStatesConfig>(json);
            Debug.Log("NpcStatesConfig loaded: " + npcStatesConfig.npcTypes.Count + " types");
        }
        else
        {
            Debug.LogWarning("NpcStatesConfig.json not found at: " + path);
        }
    }

    // Devuelve la configuración de estados para un tipo de NPC
    public List<NpcStateConfig> GetStatesForNpcType(string npcType)
    {
        if (npcStatesConfig == null) return null;
        var type = npcStatesConfig.npcTypes.Find(t => t.npcType == npcType);
        return type != null ? type.states : null;
    }

    // Devuelve el dialogTreeName para un tipo y estado de NPC
    public string GetDialogTreeName(string npcType, string state)
    {
        var states = GetStatesForNpcType(npcType);
        if (states == null) return null;
        var stateConfig = states.Find(s => s.state == state);
        return stateConfig != null ? stateConfig.dialogTreeName : null;
    }

    // Devuelve las condiciones para un tipo y estado de NPC
    public List<string> GetConditions(string npcType, string state)
    {
        var states = GetStatesForNpcType(npcType);
        if (states == null) return null;
        var stateConfig = states.Find(s => s.state == state);
        return stateConfig != null ? stateConfig.conditions : null;
    }

    // Ejemplo de integración: obtener el DialogTree para un NPC y lanzarlo con DialogManager
    public void StartNpcDialog(string npcType, string state, Transform npcTransform)
    {
        string dialogTreeName = GetDialogTreeName(npcType, state);
        if (!string.IsNullOrEmpty(dialogTreeName))
        {
            DialogTree dialogTree = FindDialogTreeByName(dialogTreeName);
            if (dialogTree != null)
            {
                DialogManager dialogManager = FindObjectOfType<DialogManager>();
                if (dialogManager != null)
                {
                    dialogManager.StartDialog(dialogTree, npcTransform); // <-- npcTransform es el NPC
                    Debug.Log($"[NpcStageScript] Lanzando DialogTree '{dialogTree.name}' para {npcType} en estado '{state}'");
                }
            }
        }
    }

    DialogTree FindDialogTreeByName(string name)
    {
        // Esto es solo un ejemplo. Puedes buscar en recursos, en la escena, etc.
        DialogTree[] allTrees = Resources.FindObjectsOfTypeAll<DialogTree>();
        foreach (var tree in allTrees)
        {
            if (tree.name == name)
                return tree;
        }
        return null;
    }

    public DialogTree GetDialogTreeForNpc(string npcType, string state)
    {
        if (npcStatesConfig == null) return null;
        var npcTypeConfig = npcStatesConfig.npcTypes.Find(t => t.npcType == npcType);
        if (npcTypeConfig == null) return null;
        var stateConfig = npcTypeConfig.states.Find(s => s.state == state);
        if (stateConfig == null || string.IsNullOrEmpty(stateConfig.dialogTreeName)) return null;

        // Busca el DialogTree en la escena o recursos por nombre
        DialogTree[] allTrees = FindObjectsOfType<DialogTree>();
        foreach (var tree in allTrees)
        {
            if (tree.name == stateConfig.dialogTreeName)
                return tree;
        }
        return null;
    }
}
