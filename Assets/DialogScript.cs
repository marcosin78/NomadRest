using UnityEngine;
using TMPro;

public class DialogScript : MonoBehaviour
{
    public Transform focusPoint;

    private MeshRenderer playerMeshRenderer;
    private Camera mainCamera;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    public bool isDialogActive = false;

    public float CameraDistance = 2f;
    public bool hasSpecialDialog = false;

    // Referencias para el sistema de diálogo
    public TextMeshProUGUI dialogUIText;
    public Canvas dialogCanvas;
    private IDialogProvider dialogProvider;
    private string[] dialogLines;
    private int currentLine = 0;

    void Start()
{
    mainCamera = Camera.main;
    GameObject playerGO = GameObject.FindWithTag("Player");
    if (playerGO != null)
        playerMeshRenderer = playerGO.GetComponent<MeshRenderer>();

    GameObject focusObj = GameObject.FindWithTag("DialogLookPoint");
    if (focusObj != null)
        focusPoint = focusObj.transform;

    // Busca el Canvas por tag
        GameObject canvasObj = GameObject.FindWithTag("DialogCanvas");
        if (canvasObj != null)
        {
            dialogCanvas = canvasObj.GetComponent<Canvas>();
            dialogUIText = canvasObj.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        else
        {
            Debug.LogError("No se encontró ningún Canvas con el tag 'DialogCanvas'.");
        }
    dialogProvider = GetComponentInChildren<IDialogProvider>();
    if (dialogProvider != null)
        dialogLines = dialogProvider.GetDialogLines();
}

public void StartDialog()
{
    if (dialogLines == null || dialogLines.Length == 0)
    {
        Debug.LogWarning("No hay líneas de diálogo para mostrar.");
        return;
    }

    // Rota el personaje hacia el jugador
    Transform player = GameObject.FindWithTag("Player")?.transform;
    if (player != null)
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir.normalized);
    }

    if (playerMeshRenderer != null)
        playerMeshRenderer.enabled = false;

    if (dialogCanvas != null)
        dialogCanvas.gameObject.SetActive(true);

    
    // Pausa el tiempo del juego
        Time.timeScale = 0f;


    if (mainCamera != null && focusPoint != null)
    {
        Vector3 offset = focusPoint.forward * CameraDistance;
        originalCamPosition = mainCamera.transform.position;
        originalCamRotation = mainCamera.transform.rotation;

        mainCamera.transform.position = focusPoint.position + offset;
        mainCamera.transform.LookAt(focusPoint.position + Vector3.up * 0.5f);
    }

    currentLine = 0;
    isDialogActive = true;
    ShowCurrentLine();
}

void Update()
{
    if (mainCamera == null) return;

    if (isDialogActive && focusPoint != null)
    {
        Vector3 offset = focusPoint.forward * CameraDistance;
        mainCamera.transform.position = focusPoint.position + offset;
        mainCamera.transform.LookAt(focusPoint.position + Vector3.up * 0.5f);

        if (Input.GetMouseButtonDown(0))
            ShowNextLine();
    }

    if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q)) && isDialogActive)
        EndDialog();
}

private void ShowCurrentLine()
{
    if (dialogUIText != null && dialogLines != null && currentLine < dialogLines.Length)
        dialogUIText.text = dialogLines[currentLine];
}

private void ShowNextLine()
{
    currentLine++;
    if (currentLine < dialogLines.Length)
        ShowCurrentLine();
    else
        EndDialog();
}

public void EndDialog()
{
    isDialogActive = false;

    // Reanuda el tiempo del juego
        Time.timeScale = 1f;
        
    if (mainCamera != null)
    {
        mainCamera.transform.position = originalCamPosition;
        mainCamera.transform.rotation = originalCamRotation;
    }
    if (playerMeshRenderer != null)
        playerMeshRenderer.enabled = true;

    if (dialogUIText != null)
        dialogUIText.text = "";

    if (dialogCanvas != null)
        dialogCanvas.gameObject.SetActive(false);
}
    
}