using UnityEngine;

public class BeerDispenserScript : MonoBehaviour, IInteractable
{

    PlayerController player;
    public GameObject beerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnInteract()
    {

        // Busca el PlayerController en la escena (puedes mejorar esto si tienes varios jugadores)

        if (player != null && player.availableHands)
        {

            Debug.Log("Interacted with Beer Dispenser");
            player.TakeItem(beerPrefab);

        }
        else
        {
            Debug.Log("Player does not have available hands!");
        }
        
    }

    public string GetName()
    {
        return "Beer Dispenser";
    }

   
}
