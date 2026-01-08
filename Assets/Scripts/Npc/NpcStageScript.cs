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
    public int priority = 0;
    public List<string> conditions;
    public string dialogTreeName;
}

// Script encargado de gestionar la configuración de estados y diálogos de los NPCs.
// Permite cargar los estados desde un archivo JSON, consultar el estado actual según condiciones,
// y lanzar el árbol de diálogo correspondiente para cada NPC.
public class NpcStageScript : MonoBehaviour
{
    public string configFileName = "NpcStatesConfig.json";
    public NpcStatesConfig npcStatesConfig;

    void Awake()
    {
        LoadNpcStatesConfig();
    }

    // Carga la configuración de estados de los NPCs desde un archivo JSON en StreamingAssets
    void LoadNpcStatesConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, configFileName);
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

    // Lanza el árbol de diálogo correspondiente para el NPC y estado actual
    public void StartNpcDialog(string npcType, string state, Transform npcTransform)
    {
        string dialogTreeName = GetDialogTreeName(npcType, state);
        Debug.Log($"[NpcStageScript] Para {npcType} en estado '{state}', dialogTreeName es '{dialogTreeName}'");
        if (!string.IsNullOrEmpty(dialogTreeName))
        {
            DialogTree dialogTree = FindDialogTreeByName(dialogTreeName);
            Debug.Log($"[NpcStageScript] DialogTree encontrado: {(dialogTree != null ? dialogTree.name : "NULO")}");
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

    // Busca el DialogTree por nombre en Resources/DialogLines o en la escena
    DialogTree FindDialogTreeByName(string name)
    {
        DialogTree tree = Resources.Load<DialogTree>("DialogLines/" + name);
        if (tree != null)
            return tree;

        DialogTree[] allTrees = FindObjectsOfType<DialogTree>();
        foreach (var t in allTrees)
        {
            if (t.name == name)
                return t;
        }
        return null;
    }   

    // Devuelve el DialogTree para un NPC y estado concreto
    public DialogTree GetDialogTreeForNpc(string npcType, string state)
    {
        if (npcStatesConfig == null) return null;
        var npcTypeConfig = npcStatesConfig.npcTypes.Find(t => t.npcType == npcType);
        if (npcTypeConfig == null) return null;
        var stateConfig = npcTypeConfig.states.Find(s => s.state == state);
        if (stateConfig == null || string.IsNullOrEmpty(stateConfig.dialogTreeName)) return null;

        DialogTree[] allTrees = FindObjectsOfType<DialogTree>();
        foreach (var tree in allTrees)
        {
            if (tree.name == stateConfig.dialogTreeName)
                return tree;
        }
        return null;
    }

    // Devuelve el estado actual para un NPC según las condiciones y prioridad
    public string GetCurrentStateForNpc(string npcType)
    {
        var states = GetStatesForNpcType(npcType);
        if (states == null)
        {
            Debug.LogWarning($"[GetCurrentStateForNpc] No se encontraron estados para el NPC '{npcType}'.");
            return null;
        }

        NpcStateConfig bestState = null;
        int bestPriority = int.MinValue;

        Debug.Log($"[GetCurrentStateForNpc] Evaluando estados para '{npcType}': {states.Count} estados encontrados.");

        foreach (var state in states)
        {
            if (string.IsNullOrEmpty(state.state))
            {
                Debug.LogWarning($"[GetCurrentStateForNpc] Estado sin 'state' definido, ignorando.");
                continue;
            }

            bool allMet = true;
            if (state.conditions != null && state.conditions.Count > 0)
            {
                foreach (var cond in state.conditions)
                {
                    bool condMet = GameConditions.Instance.HasCondition(cond);
                    Debug.Log($"[GetCurrentStateForNpc] Estado '{state.state}' requiere condición '{cond}': {(condMet ? "CUMPLIDA" : "NO CUMPLIDA")}");
                    if (!condMet)
                    {
                        allMet = false;
                        break;
                    }
                }
            }
            else
            {
                Debug.Log($"[GetCurrentStateForNpc] Estado '{state.state}' no tiene condiciones.");
            }

            if (allMet)
            {
                Debug.Log($"[GetCurrentStateForNpc] Estado '{state.state}' cumple condiciones con prioridad {state.priority}.");
                if (state.priority > bestPriority)
                {
                    bestState = state;
                    bestPriority = state.priority;
                    Debug.Log($"[GetCurrentStateForNpc] Nuevo mejor estado: '{state.state}' (prioridad {state.priority})");
                }
                else if (state.priority == bestPriority)
                {
                    bestState = state;
                    Debug.Log($"[GetCurrentStateForNpc] Empate de prioridad, se selecciona el último encontrado: '{state.state}'");
                }
            }
        }
        
        if (bestState != null)
        {
            Debug.Log($"[GetCurrentStateForNpc] Estado seleccionado para '{npcType}': '{bestState.state}' (prioridad {bestState.priority})");
            return bestState.state;
        }

        var defaultState = states.Find(s => (s.conditions == null || s.conditions.Count == 0) && !string.IsNullOrEmpty(s.state));
        if (defaultState != null)
        {
            Debug.Log($"[GetCurrentStateForNpc] No se cumplieron condiciones, usando estado por defecto: '{defaultState.state}'");
            return defaultState.state;
        }

        Debug.LogWarning($"[GetCurrentStateForNpc] No se encontró un estado válido para '{npcType}'.");
        return null;
    }
}
