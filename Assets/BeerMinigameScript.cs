using UnityEngine;
using UnityEngine.UI;

public class BeerMinigameScript : MonoBehaviour
{
         [Header("Datos de ingredientes")]
    public LicorData[] licors; // Array ampliable de licores
    public HierbaData[] herbs; // Array ampliable de hierbas

    [Header("Referencias UI")]
    public GameObject minigameCanvas; // Asigna el Canvas en el Inspector
    public Transform licorPanel;      // Panel izquierdo para sprites de licores
    public Transform hierbaPanel;     // Panel derecho para sprites de hierbas
    public GameObject ingredientButtonPrefab; // Prefab para cada botón de ingrediente

    void Start()
    {
        if (minigameCanvas != null)
            minigameCanvas.SetActive(false);
    }
    void Update()
{
    if (Input.GetKeyDown(KeyCode.H))
    {
        OpenMinigame();
    }
}

    public void OpenMinigame()
{
    if (minigameCanvas != null)
        minigameCanvas.SetActive(true);

    // Limpia los paneles
    foreach (Transform child in licorPanel) Destroy(child.gameObject);
    foreach (Transform child in hierbaPanel) Destroy(child.gameObject);

    // Recorre todos los items del inventario
    /*foreach (var item in InventorySystem.Instance.items)
    {
        if (item == null) continue;

        // Instancia el botón en el panel correspondiente según el tipo
        Transform parentPanel = null;
        IngredientType type = item.ingredientType;

        if (type == IngredientType.Licor)
            parentPanel = licorPanel;
        else if (type == IngredientType.Hierba)
            parentPanel = hierbaPanel;

        if (parentPanel != null)
        {
            GameObject btnObj = Instantiate(ingredientButtonPrefab, parentPanel);
            var img = btnObj.GetComponent<Image>();
            if (img != null) img.sprite = item.GetComponent<Image>()?.sprite; // O usa una referencia al sprite si la tienes

            btnObj.name = type.ToString() + "_" + item.id;

            var btnScript = btnObj.GetComponent<IngredientButton>();
            if (btnScript != null)
            {
                int cantidad = item.cantidadInventario;
                btnScript.Setup(img.sprite, item.id, type, cantidad);
            }
        }
    }*/
}

    public void CloseMinigame()
    {
        if (minigameCanvas != null)
            minigameCanvas.SetActive(false);
    }
}
