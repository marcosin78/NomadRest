using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public int money = 0;
    public ItemScript[] items; // Asume que tienes una clase ItemScript

    public static InventorySystem Instance;
    internal object shopMenuUI;

    void Awake()
    {
        // Busca el objeto con el tag "Player" y asigna este InventorySystem si corresponde
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

    public void PrintInventory()
    {
        Debug.Log("Inventario:");
        if (items == null || items.Length == 0)
        {
            Debug.Log("No tienes objetos.");
            return;
        }
        foreach (var item in items)
        {
            Debug.Log(item != null ? item.name : "Objeto vacío");
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log("Dinero ganado: " + amount + ". Total: " + money);
    }

     // Nuevo método para calcular la recompensa según el % de limpieza
    public void AddMoneyByCleanliness(float cleanPercent)
    {
        int reward = 0;
        if (cleanPercent >= 90f)
            reward = 8;
        else if (cleanPercent >= 70f)
            reward = 6;
        else if (cleanPercent >= 50f)
            reward = 4;
        else if (cleanPercent >= 30f)
            reward = 2;
        else
            reward = 1;

        AddMoney(reward);
        Debug.Log($"Limpieza: {cleanPercent:F2}%. Recompensa: {reward} monedas.");
    }
     public int GetItemCount(string itemName)
    {
        foreach (var it in items)
        {
            if (it.name == itemName)
                return it.cantidadInventario;
        }
        return 0;
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
    public void AddItem(ItemScript item, int amount)
    {
    item.cantidadInventario += amount;
    Debug.Log($"Añadido {amount} a {item.name}. Total: {item.cantidadInventario}");
    }
}

