using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class IngredientButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    
{
    public int ingredientID;
    public Image ingredientImage;
    public TextMeshProUGUI cantidadText;

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Canvas canvas;
    private int initialCantidad = -1;
    void Start()
    {
        // Busca el hijo llamado "IngredientImage" si no está asignado en el inspector
        if (ingredientImage == null)
            ingredientImage = transform.Find("IngredientImage")?.GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        LoadDataFromDatabase();
        SaveInitialCantidad();
    }
    void Update()
    {
        UpdateCantidad();

    }

    // Carga los datos del item desde la base de datos y actualiza la UI
    public void LoadDataFromDatabase()
    {
        var data = ItemDatabase.Instance.GetItemById(ingredientID);
        // Siempre reactiva el botón al cargar datos (reset visual)
        gameObject.SetActive(true);
        if (data != null)
        {
            if (ingredientImage != null)
            {
                ingredientImage.sprite = data.spriteName != null ? Resources.Load<Sprite>(data.spriteName) : null;
                ingredientImage.enabled = true;
            }
            UpdateCantidad();
        }
        else
        {
            Debug.LogWarning($"No se encontró el item con ID {ingredientID} en la base de datos.");
            if (ingredientImage != null)
            {
                ingredientImage.sprite = null;
                ingredientImage.enabled = false;
            }
            if (cantidadText != null)
                cantidadText.text = "0";
        }
    }

    private void UpdateCantidad()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("InventorySystem.Instance es nulo en IngredientButton.");
            if (cantidadText != null)
                cantidadText.text = "0";
            return;
        }

        int actualCantidad = InventorySystem.Instance.GetItemCount(ingredientID);
        if (cantidadText != null)
            cantidadText.text = actualCantidad.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        transform.SetParent(canvas.transform, true);

        // Guarda la cantidad inicial al empezar a interactuar (opcional, por si se reabren menús)
        if (initialCantidad == -1)
            SaveInitialCantidad();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        bool droppedOnArea = false;
        foreach (var result in results)
        {
            var dropArea = result.gameObject.GetComponent<IngredientDropArea>();
            if (dropArea != null)
            {
                // Solo añade si AddIngredient devuelve true
                bool added = dropArea.AddIngredient(ingredientID, ingredientImage.sprite);
                if (added)
                {
                    // Resta uno del inventario
                    InventorySystem.Instance.AddItem(ingredientID, -1);

                    // Actualiza la cantidad en el botón
                    UpdateCantidad();

                    // Oculta el botón al soltarlo correctamente
                    gameObject.SetActive(false);

                    droppedOnArea = true;
                }
                // Si no se pudo añadir, droppedOnArea sigue en false y el botón vuelve a su sitio
                break;
            }
        }
        // Si no se soltó en el área, vuelve al panel original y posición original
        if (!droppedOnArea)
        {
            transform.SetParent(originalParent, true);
            transform.localPosition = originalLocalPosition;
        }
    }
    public void ResetButton()
    {
        gameObject.SetActive(true);
        if (originalParent != null)
        {
            transform.SetParent(originalParent, true);
            transform.localPosition = originalLocalPosition;
        }
        RestoreInitialCantidad();
    }

    // Guarda la cantidad inicial del inventario para este ingrediente
    public void SaveInitialCantidad()
    {
        if (InventorySystem.Instance != null)
            initialCantidad = InventorySystem.Instance.GetItemCount(ingredientID);
    }

    // Restaura la cantidad inicial del inventario para este ingrediente
    public void RestoreInitialCantidad()
    {
        if (InventorySystem.Instance != null && initialCantidad != -1)
        {
            int actual = InventorySystem.Instance.GetItemCount(ingredientID);
            int diff = initialCantidad - actual;
            if (diff != 0)
            {
                InventorySystem.Instance.AddItem(ingredientID, diff);
                UpdateCantidad();
            }
        }
    }
}
