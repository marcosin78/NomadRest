using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Script encargado de gestionar el dispensador de bebidas.
/// Permite la interacción del jugador para iniciar el minijuego de servir bebidas,
/// controla el tipo de bebida seleccionada y almacena los ingredientes usados.
/// Además, integra condiciones del juego y reproduce audio al interactuar.
/// </summary>
public class BeerDispenserScript : MonoBehaviour, IInteractable
{
    // Referencia al controlador del jugador
    PlayerController player;

    // Prefabs de las bebidas que puede dispensar
    public GameObject[] drinkPrefabs;

    // Indica si el dispensador está actualmente sirviendo una bebida
    public bool isDispensing = false;

    // Array de opciones de bebida (pensado para futuras ampliaciones)
    public int[] drinkOptions = new int[2];

    // Opción de bebida seleccionada (por defecto la primera)
    public int selectedDrink = 0;

    // Referencia al script del minijuego de cerveza (asignar en el inspector)
    public BeerMinigameScript beerMinigameScript;

    [Header("Audio")]
    // Clip de audio que se reproduce al abrir el dispensador
    public AudioClip openingAudioClip;


    /// Inicializa referencias al jugador y al minijuego de cerveza.

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        beerMinigameScript = FindObjectOfType<BeerMinigameScript>();

        if (beerMinigameScript == null)
        {
            Debug.LogWarning("beerMinigameScript no encontrado en la escena.");
        }
    }

    // No se utiliza lógica en Update actualmente
    void Update()
    {
    }

    // Controla si ya se ha comprobado la condición de cócteles
    private bool hasCheckedCocktails = false;


    /// Método llamado al interactuar con el dispensador.
    /// Si el jugador tiene las manos libres, lanza el minijuego y gestiona condiciones del tutorial.

    public void OnInteract()
    {
        if(player!=null && player.availableHands==true)
        {
            Debug.Log("Player's hands are available to take a drink.");

            // Solo la primera vez que se abre la interfaz y si la condición de limpieza está activa
            if (!hasCheckedCocktails)
            {
                if (GameConditions.Instance != null && GameConditions.Instance.HasCondition("PlayerHasCleanedSaloonWithTutorialBird"))
                {
                    hasCheckedCocktails = true;
                    GameConditions.Instance.SetCondition("PlayerHasCheckedCocktailsWithTutorialBird", true);
                    Debug.Log("PlayerHasCheckedCocktailsWithTutorialBird activada por primera vez.");
                }
            }

            // Lanza el minijuego de ingredientes
            if (beerMinigameScript != null)
            {
                beerMinigameScript.StartMinigame(this);
                AudioManager.Instance.PlaySound(openingAudioClip);
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

    // Lista de ingredientes usados en la última bebida dispensada
    public System.Collections.Generic.List<int> lastUsedIngredients;


    /// Llamado al finalizar el minijuego, almacena los ingredientes usados y marca el dispensador como ocupado.
    /// <param name="ingredientIDs">Lista de IDs de ingredientes usados</param>
    public void OnMinigameFinished(System.Collections.Generic.List<int> ingredientIDs)
    {
        isDispensing = true;
        lastUsedIngredients = new System.Collections.Generic.List<int>(ingredientIDs); // COPIA, no referencia
    

        Debug.Log("Minigame finished, dispensing drink with ingredients: " + string.Join(", ", ingredientIDs));
    }
    public string GetName()
    {
        return "Drink Dispenser";
    }
}
