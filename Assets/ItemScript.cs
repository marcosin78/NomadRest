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
    public string itemName;
    public int cantidadInventario = 0;
    public int cantidadComprar = 0;

    public int price; // Precio del item

    void Start()
    {
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
        itemNameText.text = itemName;
        cantidadInventarioText.text = cantidadInventario.ToString();
        cantidadComprarText.text = cantidadComprar.ToString();
    } 

    // Puedes añadir métodos para actualizar la cantidad en inventario desde el InventorySystem
}
