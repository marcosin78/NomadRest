using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

public enum IngredientType
{
    Licor,
    Hierba,
    other
}
public class IngredientButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int ingredientID;
    public IngredientType ingredientType;
    public Image ingredientImage;

    public TextMeshProUGUI cantidadText;

    private Transform originalParent;
    private Canvas canvas;

    void Awake()
    {
        ingredientImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void Setup(Sprite sprite, int id, IngredientType type, int cantidad)
    {
        ingredientImage.sprite = sprite;
        ingredientID = id;
        ingredientType = type;

            if (cantidadText != null)
        cantidadText.text = cantidad.ToString(); 
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
    // Comprueba si se soltó sobre el área de destino
    var results = new System.Collections.Generic.List<RaycastResult>();
    EventSystem.current.RaycastAll(eventData, results);
    foreach (var result in results)
    {
        var dropArea = result.gameObject.GetComponent<IngredientDropArea>();
        if (dropArea != null)
        {
            dropArea.AddIngredient(ingredientID, ingredientImage.sprite);

            // Resta uno del inventario
            int cantidadRestante = InventorySystem.Instance.GetItemCount(gameObject.name) - 1;
            //InventorySystem.Instance.AddItem(GetComponent<ItemScript>(), -1);

            // Elimina el botón solo si ya no quedan ingredientes
            if (cantidadRestante <= 0)
                Destroy(gameObject);

            return;
        }
    }
    // Si no se soltó en el área, vuelve al panel original
    transform.SetParent(originalParent, true);
}
}
