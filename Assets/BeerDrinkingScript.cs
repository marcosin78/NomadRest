using System;
using UnityEngine;

public class BeerDrinkingScript : MonoBehaviour
{
    PlayerController player;
    public bool askingBeer = false;

    public bool beerDelivered = false;
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
                    player.DropItem();
                    beerDelivered = true;
                    Debug.Log("Beer delivered to NPC: " + gameObject.name);

                    // Notifica al NPCWalkingScript
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
    }

    void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}
