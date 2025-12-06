using UnityEngine;
using TMPro;

public class ShopSystem : MonoBehaviour, IInteractable
{
    public InventorySystem playerInventory;

    public GameObject shopMenuUI;

    public ItemScript[] itemPrefabs; // Asigna en el Inspector los objetos ItemScript de la tienda
    public UnityEngine.UI.Button buyButton; // Asigna el botón de comprar

    public void Start()
    {
        shopMenuUI = GameObject.Find("ShopCanvas");

        

        if(buyButton == null)
        {
            buyButton = shopMenuUI.transform.Find("BuyButton").GetComponent<UnityEngine.UI.Button>();
        }else
        {
            Debug.LogError("No se encontró el botón BuyButton en el ShopCanvas.");
        }
        
        // Asegúrate de que el menú de la tienda está oculto al inicio
        if (shopMenuUI != null)
        {
            shopMenuUI.SetActive(false);
        }
        else
        {
            
            Debug.LogError("No se encontró el objeto ShopCanvas en la escena.");
        }

        //PlayerInventory
        if (playerInventory == null)
        {
            playerInventory = InventorySystem.Instance;
        }

        buyButton.onClick.AddListener(BuyItems);
    }
    public void Update()
    {
      if(Input.GetKeyDown(KeyCode.Q) && shopMenuUI.activeSelf)
      {
        OnEndInteract();
      }
    }
    
    public void BuyItems()
{
    int totalCost = 0;

    for (int i = 0; i < itemPrefabs.Length; i++)
    {
        var itemScript = itemPrefabs[i];

        totalCost += itemScript.price * itemScript.cantidadComprar;
    }

    if (playerInventory.money >= totalCost)
    {
        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            var itemScript = itemPrefabs[i];

            if (itemScript.cantidadComprar > 0)
            {
                playerInventory.AddItem(itemScript, itemScript.cantidadComprar);
                itemScript.cantidadComprar = 0;
                itemScript.UpdateUI();
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
    public void OnEndInteract()
    {
        shopMenuUI.SetActive(false);

        // Bloquea y oculta el ratón
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}