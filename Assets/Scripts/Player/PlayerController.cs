using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

        private bool movementLocked = false;
        private bool cameraLocked = false;
        private Vector3 initialCameraPosition;
        private Quaternion initialCameraRotation;

        [SerializeField] private float cameraTiltAmount = 5f; // Grados máximos de inclinación
        [SerializeField] private float cameraTiltSpeed = 1.5f;   // Velocidad de interpolación
        private float currentTilt = 0f;

        private bool showGrabPoint = false;
        private Color grabPointColor = Color.green;

        private bool showInteractablePoint = false;
        private Color interactablePointColor = Color.blue;

    int movementSpeed = 5;
    Vector3 movement;
    private Rigidbody rb;

    public bool availableHands = true;
    public Transform HoldPoint;

    private Rigidbody grabbedRigidbody = null;
private Vector3 grabOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Initial camera position:" + initialCameraPosition);

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

         // Guarda la posición y rotación inicial de la cámara
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            initialCameraPosition = cam.transform.localPosition;
            initialCameraRotation = cam.transform.localRotation;
        }

    }

    // Update is called once per frame
    void Update()
    {

         if (movementLocked)
        {
            movement = Vector3.zero;
            return;
        }

        movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            movement += transform.forward;
        if (Input.GetKey(KeyCode.S))
            movement -= transform.forward;
        if (Input.GetKey(KeyCode.A))
            movement -= transform.right;
        if (Input.GetKey(KeyCode.D))
            movement += transform.right;

        movement.Normalize(); // Prevents faster diagonal movement

        if(Input.GetKey(KeyCode.Q) && HoldPoint != null && HoldPoint.childCount > 0)
        {
            DropItem();
            Debug.Log("Player dropped item with Q key.");
        }

        // Control para agarrar y mover un Rigidbody con M1 (botón izquierdo del ratón)
    if (Input.GetMouseButtonDown(0))
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3f)) // 3f: distancia máxima de agarre
        {
            // Solo agarra si implementa IGrabbable
            var grabbable = hit.collider.GetComponent<Grabbable>();
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (grabbable != null && rb != null && rb != this.rb) // No agarrar el propio Rigidbody del jugador
            {
                grabbedRigidbody = rb;
                grabOffset = hit.point - rb.transform.position;
                grabbedRigidbody.useGravity = false;
            }
        }
    }

    if (Input.GetMouseButton(0) && grabbedRigidbody != null)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint = ray.GetPoint(2f); // 2f: distancia delante de la cámara
        grabbedRigidbody.MovePosition(targetPoint - grabOffset);
    }

    if (Input.GetMouseButtonUp(0) && grabbedRigidbody != null)
    {
        grabbedRigidbody.useGravity = true;
        grabbedRigidbody = null;
    }

    //TILT DE CÁMARA

    Camera cam = GetComponentInChildren<Camera>();
    if (cam != null && !cameraLocked)
    {
    float mouseX = Input.GetAxis("Mouse X");
    float targetTilt = Mathf.Clamp(-mouseX * cameraTiltAmount, -cameraTiltAmount, cameraTiltAmount);

    // Si el ratón se mueve, interpola hacia el tilt, si no, interpola hacia 0
    if (Mathf.Abs(mouseX) > 0.01f)
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * cameraTiltSpeed);
    else
        currentTilt = Mathf.Lerp(currentTilt, 0f, Time.deltaTime * cameraTiltSpeed);

    // Aplica el tilt (roll) manteniendo la rotación original
    Vector3 euler = cam.transform.localEulerAngles;
    euler.z = currentTilt;
    cam.transform.localEulerAngles = euler;
    }

    // --- Mostrar punto si hay objeto agarrable ---
    showGrabPoint = false;
    Ray centerRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
    RaycastHit centerHit;
    if (Physics.Raycast(centerRay, out centerHit, 3f))
    {
        var grabbable = centerHit.collider.GetComponent<Grabbable>();
        Rigidbody rb = centerHit.collider.GetComponent<Rigidbody>();
        if (grabbable != null && rb != null && rb != this.rb)
        {
            showGrabPoint = true;
        }
    }


    // --- Mostrar punto si hay objeto con tag Interactable ---
    showInteractablePoint = false;
    Ray interactableRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
    RaycastHit interactableHit;
    if (Physics.Raycast(interactableRay, out interactableHit, 3f))
    {
        if (interactableHit.collider.CompareTag("Interactable"))
        {
            showInteractablePoint = true;
        }
    }

    }
    void FixedUpdate()
    {

        rb.MovePosition(rb.position + movement * movementSpeed * Time.fixedDeltaTime);

    }

    public void TakeItem(GameObject itemPrefab)
    {
        if (!availableHands || HoldPoint == null || itemPrefab == null)
            return;

        Instantiate(itemPrefab, HoldPoint.position, HoldPoint.rotation, HoldPoint);
        availableHands = false;
        Debug.Log("Player took item: " + itemPrefab.name);
    }

    public void DropItem()
    {
        if (HoldPoint == null || HoldPoint.childCount == 0)
            return;

        Transform item = HoldPoint.GetChild(0);
        Destroy(item.gameObject); // Destruye el objeto al soltarlo
        availableHands = true;
        Debug.Log("Player dropped item: " + item.name);
    }
    
       // Bloquea movimiento y cámara
    public void LockAll()
    {
        movementLocked = true;
        cameraLocked = true;

        // Desactiva el control de cámara por ratón
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            var camVision = cam.GetComponent<CamaraVision>();
            if (camVision != null)
                camVision.enabled = false;
        }
    }

    // Desbloquea movimiento y cámara
    public void UnlockAll()
    {
        movementLocked = false;
        cameraLocked = false;

        // Reactiva el control de cámara por ratón
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            var camVision = cam.GetComponent<CamaraVision>();
            if (camVision != null)
                camVision.enabled = true;
        }
    }
    // Bloquea movimiento y coloca la cámara sobre el jugador mirando a un objetivo
    public void LockCameraAboveAndLookAt(Vector3 target)
    {
        movementLocked = true;
        cameraLocked = true;
        Debug.Log("Player movement and camera locked above, looking at target.");

        // Desbloquea el ratón
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Coloca la cámara sobre el jugador y la orienta al objetivo
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            Vector3 abovePlayer = transform.position + Vector3.up * 2f; // 2 unidades sobre el jugador
            cam.transform.position = abovePlayer;
            cam.transform.LookAt(target);

            // Desactiva el control de cámara por ratón
            var camVision = cam.GetComponent<CamaraVision>();
            if (camVision != null)
                camVision.enabled = false;
        }
    }
    // Solo bloquea la cámara (sin bloquear el movimiento)
    public void LockCamera()
    {
        cameraLocked = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            var camVision = cam.GetComponent<CamaraVision>();
            if (camVision != null)
                camVision.enabled = false;
        }
    }
    // Solo desbloquea la cámara (sin desbloquear el movimiento)
    public void UnlockCamera()
    {
        cameraLocked = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            var camVision = cam.GetComponent<CamaraVision>();
            if (camVision != null)
                camVision.enabled = true;
        }
    }

    //DIALOG CAMERA INDICATORS

    private float originalSensitivity = CamaraVision.sensibilidad;
    private float dialogSensitivity = 0.05f; // Sensibilidad muy baja para diálogos

    public void UnlockCursorAndLowerSensitivity()
{
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    // Si tienes un script de cámara con sensibilidad, ajústalo aquí
    Camera cam = GetComponentInChildren<Camera>();
    if (cam != null)
    {
        var camVision = cam.GetComponent<CamaraVision>();
        if (camVision != null)
        {
            // Guarda la sensibilidad original solo la primera vez
            if (Mathf.Approximately(originalSensitivity, 1f))
                originalSensitivity = CamaraVision.sensibilidad;

            CamaraVision.sensibilidad = dialogSensitivity;
        }
    }
}

// Llama a esto para volver a bloquear el ratón y restaurar la sensibilidad
public void LockCursorAndRestoreSensitivity()
{
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    Camera cam = GetComponentInChildren<Camera>();
    if (cam != null)
    {
        var camVision = cam.GetComponent<CamaraVision>();
        if (camVision != null)
        {
            CamaraVision.sensibilidad = originalSensitivity;
        }
    }
}

// Dibuja puntos en el centro de la pantalla para indicar objetos agarrables o interactuables
    void OnGUI()
{
    if (showGrabPoint)
    {
        float size = 16f;
        Rect rect = new Rect((Screen.width - size) / 2, (Screen.height - size) / 2, size, size);
        Color prevColor = GUI.color;
        GUI.color = grabPointColor;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = prevColor;
    }
    if (showInteractablePoint)
    {
        float size = 16f;
        Rect rect = new Rect((Screen.width - size) / 2, (Screen.height - size) / 2, size, size);
        Color prevColor = GUI.color;
        GUI.color = interactablePointColor;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = prevColor;
    }
}
}
