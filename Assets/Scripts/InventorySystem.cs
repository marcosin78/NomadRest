using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public int money = 0;

    // Diccionario para almacenar la cantidad de cada item por ID
    private Dictionary<int, int> itemCounts = new Dictionary<int, int>();

    public static InventorySystem Instance;

    void Awake()
    {
        if (CompareTag("Player"))
        {
            Instance = this;
        }
        else
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                Instance = playerObj.GetComponent<InventorySystem>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            PrintAllItemsWithCounts();
        }
    }

    public void PrintInventory()
    {
        Debug.Log("Inventario:");
        if (itemCounts.Count == 0)
        {
            Debug.Log("No tienes objetos.");
            return;
        }
        foreach (var kvp in itemCounts)
        {
            var data = ItemDatabase.Instance.GetItemById(kvp.Key);
            Debug.Log($"{data.itemName} (ID: {kvp.Key}): {kvp.Value}");
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log("Dinero ganado: " + amount + ". Total: " + money);
    }

    public void SpendMoney(int amount)
    {
        if (amount > money)
        {
            Debug.Log("No tienes suficiente dinero para gastar " + amount);
            return;
        }
        money -= amount;
        Debug.Log("Dinero gastado: " + amount + ". Total restante: " + money);
    }

    public void AddItem(int itemID, int amount)
    {
        if (!itemCounts.ContainsKey(itemID))
            itemCounts[itemID] = 0;
        itemCounts[itemID] += amount;

        var data = ItemDatabase.Instance.GetItemById(itemID);
        Debug.Log($"Añadido {amount} a {data.itemName}. Total: {itemCounts[itemID]}");
    }

    public int GetItemCount(int itemID)
    {
        return itemCounts.ContainsKey(itemID) ? itemCounts[itemID] : 0;
    }

    public int GetItemCount(string itemName)
    {
        foreach (var kvp in itemCounts)
        {
            var data = ItemDatabase.Instance.GetItemById(kvp.Key);
            if (data.itemName == itemName)
                return kvp.Value;
        }
        return 0;
    }

    public void PrintAllItemsWithCounts()
    {
        if (ItemDatabase.Instance == null || ItemDatabase.Instance.items == null)
        {
            Debug.LogError("ItemDatabase no está inicializado o no hay items cargados.");
            return;
        }
        Debug.Log("Inventario completo:");
        foreach (var item in ItemDatabase.Instance.items)
        {
            int count = GetItemCount(item.id);
            Debug.Log($"{item.itemName} (ID: {item.id}): {count}");
        }
    }

    public void AddMoneyByCleanliness(float cleanPercentage)
    {
        // Ajusta el multiplicador según tu economía de juego
        int maxBonus = 5; // Máximo de monedas extra por limpieza perfecta
        int bonus = Mathf.RoundToInt((cleanPercentage / 100f) * maxBonus);

        if (bonus > 0)
        {
            AddMoney(bonus);
            Debug.Log($"¡Has recibido {bonus} monedas extra por la limpieza de la tienda! (Limpieza: {cleanPercentage:F1}%)");
        }
        else
        {
            Debug.Log("La tienda está demasiado sucia para recibir un bono de limpieza.");
        }
    }
}

