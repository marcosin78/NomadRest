using UnityEngine;
using UnityEngine.UI;

public class IngredientDropArea : MonoBehaviour
{
    public System.Collections.Generic.List<int> addedIngredientIDs = new System.Collections.Generic.List<int>();

    public void AddIngredient(int id, Sprite sprite)
    {
        addedIngredientIDs.Add(id);
        // Opcional: muestra el sprite en el área
        GameObject imgObj = new GameObject("AddedIngredient");
        imgObj.transform.SetParent(transform, false);
        var img = imgObj.AddComponent<Image>();
        img.sprite = sprite;
        imgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
    }
}