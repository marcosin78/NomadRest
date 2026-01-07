using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Script encargado de gestionar la lógica de los NPCs que piden cerveza.
/// Permite definir los ingredientes requeridos, mostrar la UI de petición,
/// comprobar la entrega de la cerveza y recompensar al jugador.
/// Además, integra la limpieza del bar en la recompensa y gestiona el flujo de interacción con el jugador.
/// </summary>
public class BeerDrinkingScript : MonoBehaviour
{
    // Referencia al controlador del jugador
    PlayerController player;

    // Estado de la petición de cerveza
    public bool askingBeer = false;
    public bool beerDelivered = false;

    // Evento para notificar destrucción del NPC
    public event Action OnDestroyed;

    // Referencia al sistema de inventario y limpieza
    InventorySystem inventorySystem;    
    DirtynessScript dirtynessScript;

    // Ingredientes posibles para el crafting de cervezas
    [Header("Ingredientes")]
    public LicorData[] licors;
    public HierbaData[] herbs;

    // Referencias a la UI del NPC
    public GameObject bubbleUI; // Burbuja de diálogo (asignar en el inspector)
    public Transform ingredientesPanel; // Panel de ingredientes dentro de la burbuja
    public Canvas npcCanvas; // Canvas del NPC (asignar en el inspector)

    // IDs de los ingredientes pedidos por el NPC
    private int licorPedidoID;
    private int hierbaPedidaID;

    // Getters públicos para los IDs de ingredientes pedidos
    public int GetLicorPedidoID() { return licorPedidoID; }
    public int GetHierbaPedidaID() { return hierbaPedidaID; }

    /// <summary>
    /// Inicializa referencias y oculta la UI al iniciar.
    /// </summary>
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player == null)
            Debug.LogError("PlayerController not found.");

        inventorySystem = player.GetComponent<InventorySystem>();
        if (inventorySystem == null)
            inventorySystem = new InventorySystem();

        dirtynessScript = FindObjectOfType<DirtynessScript>();
        if (dirtynessScript == null)    
            Debug.LogError("DirtynessScript not found."); 

        // Busca el Bubble y el Panel SOLO entre los hijos de este NPC
        if (bubbleUI == null)
            bubbleUI = transform.Find("ChatBubble")?.gameObject;

        if (ingredientesPanel == null && bubbleUI != null)
            ingredientesPanel = bubbleUI.transform.Find("IngredientsPanel");

        if (npcCanvas == null)
            npcCanvas = GetComponentInChildren<Canvas>();

        if (bubbleUI != null)
            bubbleUI.SetActive(false);

        if (npcCanvas != null)
            npcCanvas.gameObject.SetActive(false);
        else
            Debug.LogError("NPC Canvas not assigned in BeerDrinkingScript on " + gameObject.name);
    }

    /// <summary>
    /// Lógica para entregar la cerveza al NPC.
    /// Comprueba si el jugador sostiene una cerveza con los ingredientes correctos,
    /// otorga la recompensa y notifica al NPC para que se retire.
    /// </summary>
    public void GiveBeer()
    {
        Debug.Log("GiveBeer called on BeerDrinkingScript for NPC: " + gameObject.name);
        if (askingBeer)
        {
            if (player != null && player.HoldPoint != null && player.HoldPoint.childCount > 0)
            {
                Transform heldItem = player.HoldPoint.GetChild(0);
                if (heldItem.CompareTag("Beer"))
                {
                    // Comprobación de ingredientes
                    var cocktailData = heldItem.GetComponent<BeerCocktailData>();
                    if (cocktailData != null)
                    {
                        bool licorOK = cocktailData.ingredientIDs.Contains(licorPedidoID);
                        bool hierbaOK = cocktailData.ingredientIDs.Contains(hierbaPedidaID);

                        if (licorOK && hierbaOK)
                        {
                            Debug.Log("¡Ingredientes correctos! El NPC acepta la cerveza.");
                        }
                        else
                        {
                            Debug.Log("Ingredientes incorrectos. El NPC rechaza la cerveza.");
                        }
                    }
                    else
                    {
                        Debug.Log("El objeto entregado no tiene datos de ingredientes.");
                    }

                    player.DropItem();
                    beerDelivered = true;
                    askingBeer = false;
                    Debug.Log("Beer delivered to NPC: " + gameObject.name);
                    inventorySystem.AddMoney(2); // Añade dinero al inventario del jugador
                    inventorySystem.AddMoneyByCleanliness(dirtynessScript.GetCleanPercentage());
                    // Notifica al NPCWalkingScript para que se retire
                    var walking = GetComponent<NPCWalkingScript>();
                    if (walking != null)
                        walking.GoToLeavePoint();
                }
                else
                {
                    Debug.Log("Player is not holding a beer!");
                }
            }
            else
            {
                Debug.Log("Player is not holding any item!");
            }
        }
        else
        {
            Debug.Log("NPC " + gameObject.name + " is not asking for a beer.");
        }
    }

    /// <summary>
    /// Actualiza el estado del NPC cada frame.
    /// Si el bar está cerrado y no se ha entregado la cerveza, el NPC se retira.
    /// También oculta la UI si el NPC deja de pedir cerveza.
    /// </summary>
    void Update()
    {
        // Si el bar está cerrado y el NPC no se ha ido aún, lo mandamos al LeavePoint
        if (ClockScript.Instance != null && !ClockScript.Instance.OpenBarTime && !beerDelivered)
        {
            var walking = GetComponent<NPCWalkingScript>();
            if (walking != null)
            {
                walking.GoToLeavePoint();
                beerDelivered = true; // Evita que vuelva a intentarlo
                Debug.Log("Bar cerrado, NPC se va: " + gameObject.name);
            }
        }

        // Si deja de pedir cerveza, oculta el canvas y el bubble
        if (!askingBeer)
        {
            if (npcCanvas != null)
                npcCanvas.gameObject.SetActive(false);
            if (bubbleUI != null)
                bubbleUI.SetActive(false);
        }
    }

    /// <summary>
    /// Notifica a los suscriptores cuando el NPC es destruido.
    /// </summary>
    void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }

    // =========================
    // FUNCIONALIDAD DE PEDIR INGREDIENTES
    // =========================

    /// <summary>
    /// Muestra los ingredientes requeridos en la UI del NPC.
    /// Selecciona aleatoriamente un licor y una hierba, y los muestra en la burbuja.
    /// </summary>
    public void AskIngredients()
    {
        if (npcCanvas != null)
            npcCanvas.gameObject.SetActive(true);
        else
            Debug.LogError("NPC Canvas not assigned in BeerDrinkingScript on " + gameObject.name);

        if (bubbleUI != null)
            bubbleUI.SetActive(true);
        else
            Debug.LogError("Bubble UI not assigned in BeerDrinkingScript on " + gameObject.name);

        // Limpia los slots
        Transform licorSlot = ingredientesPanel.Find("LicorSlot");
        Transform hierbaSlot = ingredientesPanel.Find("HierbaSlot");
        if (licorSlot != null)
            foreach (Transform child in licorSlot) Destroy(child.gameObject);
        if (hierbaSlot != null)
            foreach (Transform child in hierbaSlot) Destroy(child.gameObject);

        // Filtra los items por tipo usando ItemDatabase
        var licores = ItemDatabase.Instance.items.Where(i => i.ingredientType == "Licor").ToList();
        var hierbas = ItemDatabase.Instance.items.Where(i => i.ingredientType == "Herb").ToList();

        // Selecciona uno aleatorio de cada tipo
        ItemData licorElegido = licores.Count > 0 ? licores[UnityEngine.Random.Range(0, licores.Count)] : null;
        ItemData hierbaElegida = hierbas.Count > 0 ? hierbas[UnityEngine.Random.Range(0, hierbas.Count)] : null;

        licorPedidoID = licorElegido != null ? licorElegido.id : -1;
        hierbaPedidaID = hierbaElegida != null ? hierbaElegida.id : -1;

        Debug.Log("NPC " + gameObject.name + " is asking for Licor ID: " + licorPedidoID + " and Hierba ID: " + hierbaPedidaID);

        // Instancia la imagen del licor en su slot
        if (licorSlot != null && licorElegido != null)
        {
            GameObject licorObj = new GameObject("Licor");
            licorObj.transform.SetParent(licorSlot, false);
            var licorImg = licorObj.AddComponent<UnityEngine.UI.Image>();
            if (licorElegido.sprite != null)
            {
                licorImg.sprite = licorElegido.sprite;
            }
            else
            {
                Debug.LogWarning($"Sprite no encontrado para '{licorElegido.itemName}' (spriteName: {licorElegido.spriteName})");
                // licorImg.sprite = Resources.Load<Sprite>("default_icon"); // Opcional: sprite por defecto
            }
            licorObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0.5f, 0.5f);
        }
        else
        {
            Debug.LogError("LicorSlot not found or no licor available for NPC: " + gameObject.name);
        }

        // Instancia la imagen de la hierba en su slot
        if (hierbaSlot != null && hierbaElegida != null)
        {
            GameObject hierbaObj = new GameObject("Hierba");
            hierbaObj.transform.SetParent(hierbaSlot, false);
            var hierbaImg = hierbaObj.AddComponent<UnityEngine.UI.Image>();
            if (hierbaElegida.sprite != null)
            {
                hierbaImg.sprite = hierbaElegida.sprite;
            }
            else
            {
                Debug.LogWarning($"Sprite no encontrado para '{hierbaElegida.itemName}' (spriteName: {hierbaElegida.spriteName})");
                // hierbaImg.sprite = Resources.Load<Sprite>("default_icon"); // Opcional: sprite por defecto
            }
            hierbaObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0.5f, 0.5f);
        }
        else
        {
            Debug.LogError("HierbaSlot not found or no hierba available for NPC: " + gameObject.name);
        }
    }
}
