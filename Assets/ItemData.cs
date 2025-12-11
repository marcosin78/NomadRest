using UnityEngine;

[System.Serializable]
public class ItemData
{
    public int id;
    public string itemName;
    public string ingredientType; // Usar string para facilitar la carga desde JSON
    public int price;
    public string spriteName; // Nombre del sprite para cargar desde Resources

    [System.NonSerialized] public Sprite sprite; // Se asigna en tiempo de ejecución

}