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
    private Canvas canvas;

    void Start()
    {
        // Busca el hijo llamado "IngredientImage" si no está asignado en el inspector
        if (ingredientImage == null)
            ingredientImage = transform.Find("IngredientImage")?.GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        LoadDataFromDatabase();
    }
    void Update()
    {
        UpdateCantidad();

    }

    // Carga los datos del item desde la base de datos y actualiza la UI
    public void LoadDataFromDatabase()
    {
        var data = ItemDatabase.Instance.GetItemById(ingredientID);
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
        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            var dropArea = result.gameObject.GetComponent<IngredientDropArea>();
            if (dropArea != null)
            {
                dropArea.AddIngredient(ingredientID, ingredientImage.sprite);

                // Resta uno del inventario
                InventorySystem.Instance.AddItem(ingredientID, -1);

                // Actualiza la cantidad en el botón
                UpdateCantidad();

                return;
            }
        }
        // Si no se soltó en el área, vuelve al panel original
        transform.SetParent(originalParent, true);
    }
}
