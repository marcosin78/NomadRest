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

    //EN UN FUTURO AÑADIR FUNCION PARA SELECCIONAR BEBIDA DESDE EL JUEGO

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

        if(player!=null && player.availableHands==true)
        {
            Debug.Log("Player's hands are available to take a drink.");
            
            isDispensing = true;    

            // Comprueba que el índice seleccionado es válido
        if (selectedDrink >= 0 && selectedDrink < drinkPrefabs.Length)
        {
            GameObject selectedPrefab = drinkPrefabs[selectedDrink];

                Debug.Log("Seleccionando prefab de bebida: " + selectedPrefab.name);
            
                // Ejemplo de comparación por tipo (puedes usar tags, nombres, etc.)
                if (selectedPrefab.CompareTag("Beer"))
                {
                    Debug.Log("Giving beer to player");
                }
                else if (selectedPrefab.CompareTag("Soda"))
                {
                    Debug.Log("Giving soda to player");
                }
            player.TakeItem(selectedPrefab);
        }
        else
        {
            Debug.LogWarning("Selected drink index is out of range.");
        }
        }
        else
        {
            Debug.Log("Player's hands are not available to take a drink.");

        }
        
    }

    public string GetName()
    {
        return "Drink Dispenser";
    }

   
}
