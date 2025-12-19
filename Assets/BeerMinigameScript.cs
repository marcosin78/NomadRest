using UnityEngine;
using UnityEngine.UI;

public class BeerMinigameScript : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject minigameCanvas; // Asigna el Canvas en el Inspector
    [Header("Minijuego UI Root (solo hijos del minijuego)")]
    public Transform minigameIngredientsRoot; // Asigna el GameObject raíz de los ingredientes del minijuego
    public PlayerInteractor playerInteractor; // Asigna en el inspector
    public PlayerController playerController;
    private BeerDispenserScript currentDispenser;
    public IngredientDropArea ingredientDropArea;

    void Start()
    {
        if (minigameCanvas != null)
            minigameCanvas.SetActive(false);
        UnlockCameraAndLockCursor();

        if (ingredientDropArea == null)
        {
            ingredientDropArea.beerMinigameScript = this;
        }
        else
        {
            Debug.LogWarning("ingredientDropArea no asignado.");
        }
    }

    void Update()
    {
        // Cerrar el minijuego con Q
        if (minigameCanvas != null && minigameCanvas.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (minigameCanvas != null)
                {
                    minigameCanvas.SetActive(false);
                    UnlockCameraAndLockCursor();

                    // Limpiar solo los IngredientDropArea hijos de minigameIngredientsRoot
                    if (minigameIngredientsRoot != null)
                    {
                        var dropAreas = minigameIngredientsRoot.GetComponentsInChildren<IngredientDropArea>(true);
                        foreach (var area in dropAreas)
                        {
                            area.ClearIngredients();
                        }
                    }else
                    {
                        Debug.LogWarning("minigameIngredientsRoot no asignado.");
                    }
                }
            }
        }
    }

    public void StartMinigame(BeerDispenserScript dispenser)
{
    currentDispenser = dispenser;
    Debug.Log("Minigame started with dispenser: " + dispenser);
    if (minigameCanvas != null)
    {
        minigameCanvas.SetActive(true);
        LockCameraAndUnlockCursor();
    }
}

    public void OnMinigameComplete(System.Collections.Generic.List<int> ingredientIDs)
    {
        Debug.Log("Minigame complete! Ingredients: " + string.Join(", ", ingredientIDs));

        if (currentDispenser == null) // Arreglar posible null reference
        {
            currentDispenser = FindObjectOfType<BeerDispenserScript>();
            Debug.LogWarning("currentDispenser era null, se ha reasignado automáticamente.");
        }

        Debug.Log("currentDispenser: " + currentDispenser);

        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(false);
            UnlockCameraAndLockCursor();
            ingredientDropArea.ResetToInitialPosition();
        }
        if (currentDispenser != null)
        {
            currentDispenser.OnMinigameFinished(ingredientIDs);
            Debug.Log("Notified dispenser of minigame completion.");
        }
        
        // Limpiar solo los IngredientDropArea hijos de minigameIngredientsRoot
        if (minigameIngredientsRoot != null)
        {
            var dropAreas = minigameIngredientsRoot.GetComponentsInChildren<IngredientDropArea>(true);
            foreach (var area in dropAreas)
            {
                area.ClearIngredients();
            }
        }
        else
        {
            Debug.LogWarning("minigameIngredientsRoot no asignado.");
        }
    }

    void LockCameraAndUnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerInteractor != null)
            playerInteractor.enabled = false;
        if (playerController != null)
            playerController.LockAll(); // Bloquea movimiento y cámara
    }

    void UnlockCameraAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerInteractor != null)
            playerInteractor.enabled = true;
        if (playerController != null)
            playerController.UnlockAll(); // Desbloquea movimiento y cámara
    }

    // Llama este método desde el botón para sacar todos los ingredientes de los IngredientDropArea del minijuego
    public void RemoveAllIngredientsFromDropAreas()
    {
        if (minigameIngredientsRoot != null)
        {
            // Limpiar los IngredientDropArea
            var dropAreas = minigameIngredientsRoot.GetComponentsInChildren<IngredientDropArea>(true);
            foreach (var area in dropAreas)
            {
                area.ClearIngredients();
            }
            // Resetear la posición de los IngredientDropArea al pulsar el boton
            ingredientDropArea.ResetToInitialPosition();
        }
    }
}
