using System;
using UnityEngine;

public class BeerDrinkingScript : MonoBehaviour, IInteractable
{

    PlayerController player;
    public bool askingBeer = false;
    public event Action OnDestroyed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public string GetName()
    {
         return gameObject.name; // Opcional: nombre para UI
    }
 public void OnInteract() //ARREGLAR CONDICIONAL, DEMASIADOS IFS ANIDADOS
    {
        // Verifica si el NPC está pidiendo cerveza
        if (askingBeer)
        {
            // Verifica si el jugador tiene la cerveza en la mano
            if (player != null && player.HoldPoint != null && player.HoldPoint.childCount > 0)
            {
                Transform heldItem = player.HoldPoint.GetChild(0);
                if (heldItem.CompareTag("Beer")) // Asegúrate de que el prefab de la cerveza tenga el tag "Beer"
                {
                    Destroy(gameObject);
                    player.DropItem();
                    Debug.Log("Interacted with Beer Drinking NPC: " + gameObject.name + " (Beer delivered)");
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

    //Logica de destruccion de objeto
    void OnDestroy()
    {
        if (OnDestroyed != null)
            OnDestroyed.Invoke();
    }
}
