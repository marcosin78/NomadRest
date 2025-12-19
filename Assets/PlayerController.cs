using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

        private bool movementLocked = false;
        private bool cameraLocked = false;
        private Vector3 initialCameraPosition;
        private Quaternion initialCameraRotation;

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
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null && rb != this.rb) // No agarrar el propio Rigidbody del jugador
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
        Debug.Log("Player movement and camera locked.");

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
        Debug.Log("Player movement and camera unlocked.");

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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
        Debug.Log("Camera locked.");

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
        Debug.Log("Camera unlocked.");

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
}
