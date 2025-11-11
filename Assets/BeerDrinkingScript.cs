using System;
using UnityEngine;

public class BeerDrinkingScript : MonoBehaviour
{
    PlayerController player;
    public bool askingBeer = false;
    public event Action OnDestroyed;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player == null)
            Debug.LogError("PlayerController not found.");
    }

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
                    Destroy(gameObject);
                    player.DropItem();
                    Debug.Log("Beer delivered to NPC: " + gameObject.name);
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

    void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}
