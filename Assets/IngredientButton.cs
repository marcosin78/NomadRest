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
    private int originalSiblingIndex;

    void Start()
    {
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

    public void LoadDataFromDatabase()
    {
        var data = ItemDatabase.Instance.GetItemById(ingredientID);
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
        if (originalParent == null)
            originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalLocalPosition = transform.localPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out pos
        );
        transform.position = canvas.transform.TransformPoint(pos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Detecta el objeto bajo el puntero al soltar
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            var dropArea = r.gameObject.GetComponent<IngredientDropArea>();
            if (dropArea != null)
            {
                // Intenta añadir el ingrediente
                var data = ItemDatabase.Instance.GetItemById(ingredientID);
                Sprite sprite = data != null ? Resources.Load<Sprite>(data.spriteName) : null;
                dropArea.AddIngredient(ingredientID, sprite);
                break;
            }
        }

        // SIEMPRE vuelve a su posición original
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        transform.localPosition = originalLocalPosition;
    }

    public void ResetButton()
    {
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalSiblingIndex);
        gameObject.SetActive(true);
    }

    public void SaveInitialCantidad()
    {
        if (InventorySystem.Instance != null)
            initialCantidad = InventorySystem.Instance.GetItemCount(ingredientID);
    }

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
