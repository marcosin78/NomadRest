using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Script encargado de gestionar la base de datos de ingredientes y objetos del juego.
// Permite cargar los datos desde un archivo JSON, acceder a los datos por ID y cargar los sprites asociados desde Resources.
public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    public List<ItemData> items = new List<ItemData>();
    private Dictionary<int, ItemData> itemDict = new Dictionary<int, ItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Carga los items desde un archivo JSON y los guarda en la lista y el diccionario
    void LoadItems()
    {
        string path = Application.dataPath + "/Data/items.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            items = JsonUtility.FromJson<ItemDataListWrapper>("{\"items\":" + json + "}").items;
            foreach (var item in items)
            {
                itemDict[item.id] = item;
                // Carga el sprite desde Resources usando el nombre
                item.sprite = Resources.Load<Sprite>(item.spriteName);
            }
        }
        else
        {
            Debug.LogError("No se encontró el archivo items.json en " + path);
        }
    }

    // Devuelve el ItemData correspondiente a un ID
    public ItemData GetItemById(int id)
    {
        itemDict.TryGetValue(id, out var data);
        return data;
    }

    [System.Serializable]
    private class ItemDataListWrapper
    {
        public List<ItemData> items;
    }
}