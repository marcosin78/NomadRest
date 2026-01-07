using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Script que gestiona el minijuego de preparación de cerveza.
// Controla la UI, la interacción del jugador, la gestión de ingredientes y la comunicación con el dispensador.
public class BeerMinigameScript : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject minigameCanvas; // Canvas principal del minijuego, asignar en el Inspector

    [Header("Minijuego UI Root (solo hijos del minijuego)")]
    public Transform minigameIngredientsRoot; // Raíz de los ingredientes del minijuego
    public PlayerInteractor playerInteractor; // Referencia al script de interacción del jugador
    public PlayerController playerController; // Referencia al controlador del jugador
    private BeerDispenserScript currentDispenser; // Referencia al dispensador actual
    public IngredientDropArea ingredientDropArea; // Área donde se sueltan los ingredientes

    // Inicializa referencias, desactiva el canvas y bloquea el cursor al iniciar.
    void Start()
    {
        if (minigameCanvas != null)
            minigameCanvas.SetActive(false);
        HideAndLockCursor();

        // Si no está asignado el IngredientDropArea, intenta buscarlo o crearlo
        if (ingredientDropArea == null)
        {
            if (minigameIngredientsRoot != null)
            {
                ingredientDropArea = minigameIngredientsRoot.GetComponentInChildren<IngredientDropArea>(true);
                if (ingredientDropArea == null)
                {
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

        // Busca referencias a PlayerInteractor y PlayerController si no están asignadas
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

    // Si el minijuego está activo y se pulsa Q, inicia la rutina de cierre
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
    
    // Rutina para cerrar el minijuego solo cuando todos los ingredientes hayan vuelto a su posición original
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
                            btn.AnimateReturnToOriginal();
                    }
                }
            }
        }

        // Espera a que todas las animaciones terminen
        yield return StartCoroutine(WaitForAllIngredientButtonsToFinish());

        // Desactiva el canvas y limpia los ingredientes
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

    // Activa el minijuego, muestra el canvas y desbloquea el cursor
    public void StartMinigame(BeerDispenserScript dispenser)
    {
        currentDispenser = dispenser;
        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(true);
            ShowAndUnlockCursor();
        }
    }

    // Lógica al completar el minijuego: notifica al dispensador, limpia ingredientes y oculta la UI
    public void OnMinigameComplete(System.Collections.Generic.List<int> ingredientIDs)
    {
        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(false);
            HideAndLockCursor();
            ingredientDropArea.ResetToInitialPosition();
        }
        if (currentDispenser == null)
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
        
        // Limpia los IngredientDropArea hijos de minigameIngredientsRoot
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

    // Elimina todos los ingredientes de los IngredientDropArea del minijuego y resetea la posición
    public void RemoveAllIngredientsFromDropAreas()
    {
        if (minigameIngredientsRoot != null)
        {
            var dropAreas = minigameIngredientsRoot.GetComponentsInChildren<IngredientDropArea>(true);
            foreach (var area in dropAreas)
            {
                area.ClearIngredients();
            }
            ingredientDropArea.ResetToInitialPosition();
        }
    }

    // Muestra y desbloquea el cursor, desactiva el control del jugador y la interacción
    void ShowAndUnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerInteractor != null)
            playerInteractor.enabled = false;
        if (playerController != null)
            playerController.LockAll();
    }

    // Oculta y bloquea el cursor, reactiva el control del jugador y la interacción
    void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerInteractor != null)
            playerInteractor.enabled = true;
        if (playerController != null)
            playerController.UnlockAll();
    }

    // Espera a que todos los IngredientButton terminen su animación de retorno antes de cerrar el minijuego
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
