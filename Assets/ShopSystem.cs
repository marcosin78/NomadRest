using UnityEngine;
using TMPro;

public class ShopSystem : MonoBehaviour, IInteractable
{
    public InventorySystem playerInventory;

    public GameObject shopMenuUI;
    public ShopItem[] shopItems; // Cada ShopItem tiene un Item y un precio
    private int[] cantidadesAComprar;

    public UnityEngine.UI.Button[] plusButtons;
    public UnityEngine.UI.Button[] minusButtons;
    public TextMeshProUGUI[] cantidadTexts;
    public void Start()
    {
        cantidadesAComprar = new int[shopItems.Length];
        // Busca entre los hijos el objeto con el tag "ShopCanvas"
    foreach (Transform child in transform)
    {
        if (child.CompareTag("ShopCanvas"))
        {
            shopMenuUI = child.gameObject;
            break;
        }
    }
    // Asegúrate de que el menú de la tienda está oculto al inicio
    if (shopMenuUI != null)
    {
        shopMenuUI.SetActive(false);
    }

    for (int i = 0; i < shopItems.Length; i++)
    {
    int index = i; // Necesario para lambdas

    plusButtons[i].onClick.AddListener(() =>
    {
        cantidadesAComprar[index]++;
        cantidadTexts[index].text = cantidadesAComprar[index].ToString();
    });

    minusButtons[i].onClick.AddListener(() =>
    {
        cantidadesAComprar[index] = Mathf.Max(0, cantidadesAComprar[index] - 1);
        cantidadTexts[index].text = cantidadesAComprar[index].ToString();
    });

    cantidadTexts[i].text = cantidadesAComprar[i].ToString();
    }
    }
    
    // Este método lo llamas desde el botón "Comprar" (Pendiente de asignar boton)
    public void BuyItems()
{
    int totalCost = 0;

    for (int i = 0; i < shopItems.Length; i++)
    {
        totalCost += shopItems[i].price * cantidadesAComprar[i];
    }

    if (playerInventory.money >= totalCost)
    {
        for (int i = 0; i < shopItems.Length; i++)
        {
            if (cantidadesAComprar[i] > 0)
            {
                playerInventory.AddItem(shopItems[i].item, cantidadesAComprar[i]);
                cantidadesAComprar[i] = 0; // Resetea la cantidad a comprar
            }
        }
        playerInventory.SpendMoney(totalCost);
        Debug.Log("Compra realizada por " + totalCost + " monedas.");
    }
    else
    {
        Debug.Log("No tienes suficiente dinero para la compra.");
    }
}

    public string GetName()
    {
        throw new System.NotImplementedException();
    }

    public void OnInteract()
    {

    shopMenuUI.SetActive(true);

    // Desbloquea y muestra el ratón
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
    }
}

[System.Serializable]
public class ShopItem
{
    public Item item;
    public int price;
}