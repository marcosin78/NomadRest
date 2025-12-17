using UnityEngine;
using UnityEngine.UI;

public class BeerMinigameScript : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject minigameCanvas; // Asigna el Canvas en el Inspector
    public PlayerInteractor playerInteractor; // Asigna en el inspector
    
    private BeerDispenserScript currentDispenser;
    void Start()
    {
        if (minigameCanvas != null)
            minigameCanvas.SetActive(false);
        UnlockCameraAndLockCursor();
    }

    void Update()
    {
        // Abrir el minijuego con H
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (minigameCanvas != null)
            {
                minigameCanvas.SetActive(true);
                LockCameraAndUnlockCursor();
            }
        }

        // Cerrar el minijuego con Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (minigameCanvas != null)
            {
                minigameCanvas.SetActive(false);
                UnlockCameraAndLockCursor();

                // Resetear todos los IngredientButton al cerrar el menú
                var ingredientButtons = minigameCanvas.GetComponentsInChildren<IngredientButton>(true);
                foreach (var btn in ingredientButtons)
                {
                    btn.ResetButton();
                }

                // Limpiar todos los IngredientDropArea al cerrar el menú
                var dropAreas = minigameCanvas.GetComponentsInChildren<IngredientDropArea>(true);
                foreach (var area in dropAreas)
                {
                    area.ClearIngredients();
                }
            }
        }
    
    }
   public void StartMinigame(BeerDispenserScript dispenser)
{
    Debug.Log("Starting cocktail minigame...");
    Debug.Log("minigameCanvas is " + (minigameCanvas == null ? "NULL" : "OK"));
    currentDispenser = dispenser;
    if (minigameCanvas != null)
    {
        minigameCanvas.SetActive(true);
        LockCameraAndUnlockCursor();
        Debug.Log("Minigame canvas activated.");
    }
    else
    {
        Debug.LogWarning("Minigame canvas is not assigned.");
    }
}

    public void OnMinigameComplete(System.Collections.Generic.List<int> ingredientIDs)
{
    if (minigameCanvas != null)
    {
        minigameCanvas.SetActive(false);
        UnlockCameraAndLockCursor();
    }
    if (currentDispenser != null)
    {
        currentDispenser.OnMinigameFinished(ingredientIDs);
        currentDispenser = null;
    }
}

    void LockCameraAndUnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerInteractor != null)
            playerInteractor.enabled = false;
    }

    void UnlockCameraAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerInteractor != null)
            playerInteractor.enabled = true;
    }
}
