using UnityEngine;
using System.Collections.Generic;
using TMPro; // Añadido para usar TextMeshProUGUI

// Script encargado de gestionar el inventario del jugador.
// Permite añadir y quitar dinero, añadir y quitar ingredientes por ID, consultar cantidades y mostrar el inventario.
// Incluye lógica para bonificar al jugador según la limpieza del bar.
public class InventorySystem : MonoBehaviour
{
    public int money = 0;

    // Diccionario para almacenar la cantidad de cada item por ID
    private Dictionary<int, int> itemCounts = new Dictionary<int, int>();

    public static InventorySystem Instance;

    private TextMeshProUGUI moneyText; // Referencia al texto de dinero

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

        // Busca el texto con tag "MoneyText" dentro del Player
        var moneyTextObj = GameObject.FindGameObjectWithTag("MoneyText");
        if (moneyTextObj != null)
            moneyText = moneyTextObj.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        UpdateMoneyText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            PrintAllItemsWithCounts();
        }
    }

    // Muestra el inventario actual en consola
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

    // Añade dinero al inventario
    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log("Dinero ganado: " + amount + ". Total: " + money);
        UpdateMoneyText();
    }

    // Resta dinero si hay suficiente
    public void SpendMoney(int amount)
    {
        if (amount > money)
        {
            Debug.Log("No tienes suficiente dinero para gastar " + amount);
            return;
        }
        money -= amount;
        Debug.Log("Dinero gastado: " + amount + ". Total restante: " + money);
        UpdateMoneyText();
    }

    // Añade una cantidad de un ingrediente por ID
    public void AddItem(int itemID, int amount)
    {
        if (!itemCounts.ContainsKey(itemID))
            itemCounts[itemID] = 0;
        itemCounts[itemID] += amount;

        var data = ItemDatabase.Instance.GetItemById(itemID);
        Debug.Log($"Añadido {amount} a {data.itemName}. Total: {itemCounts[itemID]}");
    }

    // Quita una cantidad de un ingrediente por ID si hay suficiente
    public bool RemoveItem(int itemID, int amount)
    {
        if (!itemCounts.ContainsKey(itemID) || itemCounts[itemID] < amount)
        {
            Debug.LogWarning($"No hay suficiente cantidad del ingrediente ID {itemID} para restar {amount}.");
            return false;
        }

        itemCounts[itemID] -= amount;
        if (itemCounts[itemID] <= 0)
        {
            itemCounts.Remove(itemID);
        }

        var data = ItemDatabase.Instance.GetItemById(itemID);
        Debug.Log($"Removido {amount} de {data.itemName}. Total restante: {GetItemCount(itemID)}");
        return true;
    }

    // Devuelve la cantidad de un ingrediente por ID
    public int GetItemCount(int itemID)
    {
        return itemCounts.ContainsKey(itemID) ? itemCounts[itemID] : 0;
    }

    // Devuelve la cantidad de un ingrediente por nombre
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

    // Muestra todos los ingredientes y sus cantidades en consola
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

    // Añade dinero extra según el porcentaje de limpieza del bar
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

    // Actualiza el texto de dinero en la UI
    private void UpdateMoneyText()
    {
        if (moneyText != null)
            moneyText.text = $"DINERO : {money}";
    }
}

