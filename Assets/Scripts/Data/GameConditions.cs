using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Representa una condición individual del juego.
/// </summary>
[System.Serializable]
public class ConditionData
{
    public string key;           // Identificador único de la condición
    public string description;   // Descripción legible de la condición
    public bool active;          // Estado actual de la condición (activa o no)
}

/// <summary>
/// Contenedor serializable para una lista de condiciones.
/// </summary>
[System.Serializable]
public class ConditionsConfig
{
    public List<ConditionData> conditions; // Lista de condiciones configuradas
}

/// <summary>
/// Sistema centralizado para gestionar condiciones globales del juego.
/// Permite cargar, consultar y modificar condiciones desde un archivo JSON.
/// Implementa el patrón Singleton para acceso global.
/// </summary>
public class GameConditions : MonoBehaviour
{
    [Tooltip("Nombre del archivo de configuración de condiciones (JSON)")]
    public string configFileName = "ConditionsConfig.json";

    [Tooltip("Configuración cargada de condiciones")]
    public ConditionsConfig conditionsConfig;

    // Diccionario para acceso rápido a las condiciones por clave
    private Dictionary<string, ConditionData> conditionDict = new Dictionary<string, ConditionData>();

    // Instancia global (Singleton)
    public static GameConditions Instance;

    /// <summary>
    /// Inicializa la instancia Singleton y carga la configuración de condiciones al iniciar.
    /// </summary>
    void Awake()
    {
        Instance = this;
        LoadConditionsConfig();
    }

    /// <summary>
    /// Carga la configuración de condiciones desde un archivo JSON ubicado en Assets/Scripts.
    /// Llena el diccionario interno para acceso rápido.
    /// </summary>
    void LoadConditionsConfig()
    {
        // Cambia la ruta para buscar en Assets/Scripts
        string path = Path.Combine(Application.dataPath, "Scripts", configFileName);
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

    /// <summary>
    /// Consulta si una condición está activa.
    /// </summary>
    /// <param name="key">Clave única de la condición</param>
    /// <returns>True si la condición existe y está activa, false en caso contrario</returns>
    public bool HasCondition(string key)
    {
        return conditionDict.ContainsKey(key) && conditionDict[key].active;
    }

    /// <summary>
    /// Establece el estado de una condición existente.
    /// </summary>
    /// <param name="key">Clave única de la condición</param>
    /// <param name="value">Nuevo valor de estado (por defecto: true)</param>
    public void SetCondition(string key, bool value = true)
    {
        if (conditionDict.ContainsKey(key))
        {
            conditionDict[key].active = value;
        }
    }
}