using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemScript : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI cantidadInventarioText;
    public TextMeshProUGUI cantidadComprarText;
    public Button sumarButton;
    public Button restarButton;

    [Header("Datos del item")]
    public int id; // Solo el ID es público

    private ItemData data;
    public int cantidadInventario = 0;
    public int cantidadComprar = 0;

    void Start()
    {
        data = ItemDatabase.Instance.GetItemById(id);
        UpdateUI();

        sumarButton.onClick.AddListener(() =>
        {
            cantidadComprar++;
            UpdateUI();
        });

        restarButton.onClick.AddListener(() =>
        {
            cantidadComprar = Mathf.Max(0, cantidadComprar - 1);
            UpdateUI();
        });
    }

    public void UpdateUI()
    {
        if (data != null)
        {
            itemNameText.text = data.itemName;
            cantidadInventarioText.text = cantidadInventario.ToString();
            cantidadComprarText.text = cantidadComprar.ToString();
        }
    }

    // Métodos para acceder a los datos del item
    public int GetPrice() => data?.price ?? 0;
    public string GetIngredientType() => data?.ingredientType ?? "";
    public string GetItemName() => data?.itemName ?? "";

    // Métodos para actualizar cantidades desde InventorySystem
    public void SetCantidadInventario(int cantidad)
    {
        cantidadInventario = cantidad;
        UpdateUI();
    }
    public int GetCantidadComprar() => cantidadComprar;
    public void ResetCantidadComprar()
    {
        cantidadComprar = 0;
        UpdateUI();
    }

    public void SyncCantidadInventario()
    {
        cantidadInventario = InventorySystem.Instance.GetItemCount(id);
        UpdateUI();
    }
}
