using UnityEngine;

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

    void Start()
    {
        mainCamera = Camera.main;
        GameObject playerGO = GameObject.FindWithTag("Player");

         if (playerGO != null)
        playerMeshRenderer = playerGO.GetComponent<MeshRenderer>();

        // Busca el objeto vacío con el tag "DialogLookPoint"
        GameObject focusObj = GameObject.FindWithTag("DialogLookPoint");
        if (focusObj != null)
        {
            focusPoint = focusObj.transform;
        }
        else
        {
            Debug.LogError("No se encontró ningún objeto con el tag 'DialogLookPoint' para focusPoint en " + gameObject.name);
        }
    }

    void Update()
{
    if (mainCamera == null) return;

    if (isDialogActive && focusPoint != null)
    {
        // Mantén la cámara fija en la posición y rotación objetivo
        Vector3 offset = focusPoint.forward * CameraDistance;
        mainCamera.transform.position = focusPoint.position + offset;
        mainCamera.transform.LookAt(focusPoint.position + Vector3.up * 0.5f);
    }

    if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q)) && isDialogActive)
    {
        EndDialog();
    }
}

    public void StartDialog()
{
    Debug.Log("Dialog started with: " + gameObject.name);

    // Rota el GameObject (personaje) para mirar hacia el jugador usando Rigidbody
    Transform player = GameObject.FindWithTag("Player")?.transform;
    if (player != null)
    {
    Vector3 lookDir = player.position - transform.position;
    lookDir.y = 0; // Solo rota en el eje Y
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized);
            // Aplica la rotación directamente, ignorando Rigidbody si el juego se va a pausar
            transform.rotation = targetRotation;
        }
    }

    // Oculta solo el MeshRenderer del Player
    if (playerMeshRenderer != null)
        playerMeshRenderer.enabled = false;

    // Ahora sí, pausa el juego
    Time.timeScale = 0f;

    if (mainCamera != null && focusPoint != null)
    {
        Vector3 offset = focusPoint.forward * CameraDistance; // Ajusta la distancia de la cámara al personaje
        originalCamPosition = mainCamera.transform.position;
        originalCamRotation = mainCamera.transform.rotation;

        mainCamera.transform.position = focusPoint.position + offset;
        mainCamera.transform.LookAt(focusPoint.position + Vector3.up * 0.5f);
    }

    isDialogActive = true;
}
    public void EndDialog()
    {
        Time.timeScale = 1f;
        isDialogActive = false;
        if (mainCamera != null)
        {
            mainCamera.transform.position = originalCamPosition;
            mainCamera.transform.rotation = originalCamRotation;
        }

            // Vuelve a mostrar el MeshRenderer del Player
            if (playerMeshRenderer != null)
            playerMeshRenderer.enabled = true;
    }
}