using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR;

public class BeerDispenserScript : MonoBehaviour, IInteractable
{
    PlayerController player;
    public GameObject[] drinkPrefabs;
    public bool isDispensing = false;
    public int[] drinkOptions = new int[2]; //Array de opciones de bebida (Por si en un futuro hay más tipos de bebida)
    public int selectedDrink = 0; //Opción de bebida seleccionada (Por defecto la primera)

    public BeerMinigameScript beerMinigameScript; // Asigna en el inspector


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        beerMinigameScript = FindObjectOfType<BeerMinigameScript>();

        if (beerMinigameScript == null)
        {
            Debug.LogWarning("beerMinigameScript no encontrado en la escena.");
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnInteract()
    {
        if(player!=null && player.availableHands==true)
    {
        Debug.Log("Player's hands are available to take a drink.");

        // Lanza el minijuego de ingredientes
        if (beerMinigameScript != null)
        {
            beerMinigameScript.StartMinigame(this);
        }
        else
        {
            Debug.LogWarning("beerMinigameScript no asignado.");
        }
    }
    
    else
    {
        Debug.Log("Player's hands are not available to take a drink.");
    }
    }
    public System.Collections.Generic.List<int> lastUsedIngredients; // Puedes usar esto para guardar los ingredientes

public void OnMinigameFinished(System.Collections.Generic.List<int> ingredientIDs)
{
    isDispensing = true;
    lastUsedIngredients = ingredientIDs;
    // Aquí puedes hacer lógica extra, como validar la receta, etc.
    Debug.Log("Minigame finished, dispensing drink with ingredients: " + string.Join(", ", ingredientIDs));

}
    public string GetName()
    {
        return "Drink Dispenser";
    }

   
}
