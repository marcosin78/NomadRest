using UnityEngine;
using System.IO;
using System.Collections.Generic;
using TMPro;

// Script encargado de gestionar la tienda del juego.
// Permite mostrar la UI de la tienda, comprar ingredientes, actualizar el inventario y controlar el acceso según condiciones del juego.
public class ShopSystem : MonoBehaviour, IInteractable
{
    public InventorySystem playerInventory;

    public GameObject shopMenuUI;

    public UnityEngine.UI.Button buyButton;

    public List<ItemData> itemDataList = new List<ItemData>();
    public ItemScript[] itemPrefabs; // Prefabs de UI, asignados en el Inspector

    [Header("Audio")]
    public AudioClip interactAudioClip;
    public AudioClip endInteractAudioClip;

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
        
        if (shopMenuUI != null)
        {
            shopMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogError("No se encontró el objeto ShopCanvas en la escena.");
        }

        if (playerInventory == null)
        {
            playerInventory = InventorySystem.Instance;
        }

        buyButton.onClick.AddListener(BuyItems);

        LoadItemsFromJson();

        // Asigna solo el id a cada prefab
        for (int i = 0; i < itemPrefabs.Length && i < itemDataList.Count; i++)
        {
            itemPrefabs[i].UpdateUI();
        }
    }
    public void Update()
    {
      if(Input.GetKeyDown(KeyCode.Q) && shopMenuUI.activeSelf)
      {
        OnEndInteract();
      }
    }
    
    // Realiza la compra de los ingredientes seleccionados y actualiza el inventario
    public void BuyItems()
    {
        int totalCost = 0;

        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            var itemScript = itemPrefabs[i];
            totalCost += itemScript.GetPrice() * itemScript.GetCantidadComprar();
        }

        if (playerInventory.money >= totalCost)
        {
            for (int i = 0; i < itemPrefabs.Length; i++)
            {
                var itemScript = itemPrefabs[i];

                if (itemScript.GetCantidadComprar() > 0)
                {
                    playerInventory.AddItem(itemScript.id, itemScript.GetCantidadComprar());
                    itemScript.ResetCantidadComprar();

                    foreach (var item in itemPrefabs)
                    {
                        item.SyncCantidadInventario();
                    }
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

    // Carga los datos de los ingredientes desde el archivo JSON
    private void LoadItemsFromJson()
    {
        string path = Application.dataPath + "/Data/items.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            itemDataList = JsonUtility.FromJson<ItemDataListWrapper>("{\"items\":" + json + "}").items;
        }
        else
        {
            Debug.LogError("No se encontró el archivo items.json en " + path);
        }
    }

    [System.Serializable]
    private class ItemDataListWrapper
    {
        public List<ItemData> items;
    }

    public string GetName()
    {
        throw new System.NotImplementedException();
    }

    private bool hasCheckedShop = false;

    // Muestra la tienda y desbloquea el cursor, activa condición si corresponde
    public void OnInteract()
    {
        // Solo la primera vez que se abre la tienda y si HasCheckedCocktails es true
        if (!hasCheckedShop)
        {
            if (GameConditions.Instance != null && GameConditions.Instance.HasCondition("PlayerHasCheckedCocktailsWithTutorialBird"))
            {
                hasCheckedShop = true;
                GameConditions.Instance.SetCondition("PlayerHasCheckedShop", true);
                Debug.Log("PlayerHasCheckedShop activada por primera vez.");
            }
        }

        shopMenuUI.SetActive(true);

        // Desbloquea y muestra el ratón
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AudioManager.Instance.PlaySound(interactAudioClip);
    }

    // Oculta la tienda y bloquea el cursor
    public void OnEndInteract()
    {
        shopMenuUI.SetActive(false);

        // Bloquea y oculta el ratón
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        AudioManager.Instance.PlaySound(endInteractAudioClip);
    }
}