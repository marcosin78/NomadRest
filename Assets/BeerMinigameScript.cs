using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BeerMinigameScript : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject minigameCanvas; // Asigna el Canvas en el Inspector
    [Header("Minijuego UI Root (solo hijos del minijuego)")]
    public Transform minigameIngredientsRoot; // Asigna el GameObject raíz de los ingredientes del minijuego
    public PlayerInteractor playerInteractor; // Inspector asignado
    public PlayerController playerController; // Inspector asignado
    private BeerDispenserScript currentDispenser;
    public IngredientDropArea ingredientDropArea;

    void Start()
    {
        if (minigameCanvas != null)
            minigameCanvas.SetActive(false);
        HideAndLockCursor();

        // Si no está asignado en el inspector, buscarlo automáticamente
        if (ingredientDropArea == null)
        {
            // Busca un IngredientDropArea hijo del minigameIngredientsRoot
            if (minigameIngredientsRoot != null)
            {
                ingredientDropArea = minigameIngredientsRoot.GetComponentInChildren<IngredientDropArea>(true);
                if (ingredientDropArea == null)
                {
                    // Si no existe, lo creamos como hijo
                    GameObject go = new GameObject("IngredientDropArea", typeof(RectTransform), typeof(IngredientDropArea));
                    go.transform.SetParent(minigameIngredientsRoot, false);
                    ingredientDropArea = go.GetComponent<IngredientDropArea>();
                }
                Debug.Log("IngredientDropArea asignado por código.");
            }
            else
            {
                Debug.LogWarning("minigameIngredientsRoot no asignado, no se puede crear IngredientDropArea.");
            }
        }

        if (ingredientDropArea != null)
        {
            ingredientDropArea.beerMinigameScript = this;
        }
        else
        {
            Debug.LogWarning("ingredientDropArea no asignado ni encontrado.");
        }

        if (playerInteractor == null)
        {
            playerInteractor = FindObjectOfType<PlayerInteractor>();
        }
        else
        {
            Debug.LogWarning("playerInteractor no asignado.");
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
        else
        {
            Debug.LogWarning("playerController no asignado.");
        }
    }

    void Update()
    {
        if (minigameCanvas != null && minigameCanvas.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(CloseMinigameWhenButtonsFinish());
            }
        }
    }
    
    private IEnumerator CloseMinigameWhenButtonsFinish()
    {
        if (minigameIngredientsRoot != null)
        {
            var dropAreas = minigameIngredientsRoot.GetComponentsInChildren<IngredientDropArea>(true);
            foreach (var area in dropAreas)
            {
                foreach (var info in area.movedIngredientButtons)
                {
                    if (info.buttonTransform != null)
                    {
                        var btn = info.buttonTransform.GetComponent<IngredientButton>();
                        if (btn != null)
                            btn.AnimateReturnToOriginal(); // Animación normal
                    }
                }
            }
        }

        // Espera a que todas las animaciones terminen
        yield return StartCoroutine(WaitForAllIngredientButtonsToFinish());

        // Ahora sí, desactiva el canvas y limpia
        minigameCanvas.SetActive(false);
        HideAndLockCursor();

        if (minigameIngredientsRoot != null)
        {
            var dropAreas = minigameIngredientsRoot.GetComponentsInChildren<IngredientDropArea>(true);
            foreach (var area in dropAreas)
            {
                area.ClearIngredients();
            }
        }
    }

    public void StartMinigame(BeerDispenserScript dispenser)
    {
        currentDispenser = dispenser;
        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(true);
            ShowAndUnlockCursor();
        }
    }

    public void OnMinigameComplete(System.Collections.Generic.List<int> ingredientIDs)
    {
        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(false);
            HideAndLockCursor();
            ingredientDropArea.ResetToInitialPosition();
        }
        if (currentDispenser == null) // Arreglar posible null reference
        {
            currentDispenser = FindObjectOfType<BeerDispenserScript>();
            Debug.LogWarning("currentDispenser era null, se ha reasignado automáticamente.");
        }

        Debug.Log("currentDispenser: " + currentDispenser);

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

    void ShowAndUnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerInteractor != null)
            playerInteractor.enabled = false;
        if (playerController != null)
            playerController.LockAll();
    }

    void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerInteractor != null)
            playerInteractor.enabled = true;
        if (playerController != null)
            playerController.UnlockAll();
    }
    private IEnumerator WaitForAllIngredientButtonsToFinish()
{
    bool anyRunning = false;

    if (minigameIngredientsRoot != null)
    {
        var dropAreas = minigameIngredientsRoot.GetComponentsInChildren<IngredientDropArea>(true);
        foreach (var area in dropAreas)
        {
            foreach (var info in area.movedIngredientButtons)
            {
                if (info.buttonTransform != null)
                {
                    var btn = info.buttonTransform.GetComponent<IngredientButton>();
                    if (btn != null && btn.IsReturning)
                    {
                        anyRunning = true;
                        break;
                    }
                }
            }
            if (anyRunning) break;
        }
    }

    // Solo entra al bucle si hay alguno retornando
    while (anyRunning)
    {
        Debug.Log("Esperando a que todos los IngredientButton terminen sus corrutinas de retorno...");
        anyRunning = false;
        if (minigameIngredientsRoot != null)
        {
            var dropAreas = minigameIngredientsRoot.GetComponentsInChildren<IngredientDropArea>(true);
            foreach (var area in dropAreas)
            {
                foreach (var info in area.movedIngredientButtons)
                {
                    if (info.buttonTransform != null)
                    {
                        var btn = info.buttonTransform.GetComponent<IngredientButton>();
                        if (btn != null && btn.IsReturning)
                        {
                            anyRunning = true;
                            break;
                        }
                    }
                }
                if (anyRunning) break;
            }
        }
        if (anyRunning)
            yield return null; // Espera un frame
    }
}

}
