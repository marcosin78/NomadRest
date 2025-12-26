using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class ConditionData
{
    public string key;
    public string description;
    public bool active;
}

[System.Serializable]
public class ConditionsConfig
{
    public List<ConditionData> conditions;
}

public class GameConditions : MonoBehaviour
{
    public string configFileName = "ConditionsConfig.json";
    public ConditionsConfig conditionsConfig;

    private Dictionary<string, ConditionData> conditionDict = new Dictionary<string, ConditionData>();

    public static GameConditions Instance;

    void Awake()
    {
        Instance = this;
        LoadConditionsConfig();
    }

    void LoadConditionsConfig()
    {
        string path = Path.Combine(Application.dataPath, configFileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            conditionsConfig = JsonUtility.FromJson<ConditionsConfig>(json);
            foreach (var cond in conditionsConfig.conditions)
            {
                conditionDict[cond.key] = cond;
            }
        }
        else
        {
            Debug.LogWarning("ConditionsConfig.json not found at: " + path);
        }
    }

    public bool HasCondition(string key)
    {
        return conditionDict.ContainsKey(key) && conditionDict[key].active;
    }

    public void SetCondition(string key, bool value = true)
    {
        if (conditionDict.ContainsKey(key))
        {
            conditionDict[key].active = value;
        }
    }
}