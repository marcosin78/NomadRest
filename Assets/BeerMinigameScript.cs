using UnityEngine;
using UnityEngine.UI;

public class BeerMinigameScript : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject minigameCanvas; // Asigna el Canvas en el Inspector
    public PlayerInteractor playerInteractor; // Asigna en el inspector

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
            }
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
