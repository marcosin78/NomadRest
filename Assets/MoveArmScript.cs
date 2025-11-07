using UnityEngine;

public class MoveArmScript : MonoBehaviour
{
    public Rigidbody ballRb; // Asigna el Rigidbody de la bola en el inspector
    public float forceMultiplier = 50f;
    public float maxDistance = 30f;
    public float coneAngle = 90f;
    public Camera cam;

    public float mouseForceBoost = 5f;
    private Vector3 originPosition;
    private Vector3 lastMousePosition;

    void Start()
    {
        if (ballRb == null)
            ballRb = GetComponent<Rigidbody>();
        originPosition = ballRb.transform.position;
        lastMousePosition = Input.mousePosition;

        if (cam == null)
            cam = GetComponentInParent<Camera>();
    }

    void Update()
    {
        if (cam == null || ballRb == null)
            return;

        ApplyPhysics();

         // Si no se está moviendo el ratón (no se pulsa), reduce la velocidad gradualmente
        if (!Input.GetMouseButton(0))
        {
            // Reduce la velocidad del Rigidbody suavemente
            ballRb.linearVelocity = Vector3.Lerp(ballRb.linearVelocity, Vector3.zero, 5f * Time.deltaTime);
        }


        lastMousePosition = Input.mousePosition;
    }

    void ApplyPhysics()
    {
        // Usa el movimiento del ratón en vez de mousePosition
        float mouseVertical = Input.GetAxis("Mouse Y");
        float mouseHorizontal = Input.GetAxis("Mouse X");

        Vector3 forceDir = Vector3.zero;

        // Adelante/atrás según movimiento vertical
        if (Mathf.Abs(mouseVertical) > 0.01f)
        {
            Vector3 forward = cam.transform.forward;
            forward.y = 0;
            forward.Normalize();
            forceDir += forward * mouseVertical;
        }

        // Derecha/izquierda según movimiento horizontal
        if (Mathf.Abs(mouseHorizontal) > 0.01f)
        {
            Vector3 right = cam.transform.right;
            right.y = 0;
            right.Normalize();
            forceDir += right * mouseHorizontal;
        }

        if (forceDir != Vector3.zero)
        {
            float totalForce = forceMultiplier * mouseForceBoost * Time.deltaTime;
            ballRb.AddForce(forceDir.normalized * totalForce, ForceMode.Force);
            Debug.Log($"Fuerza aplicada: {forceDir.normalized * totalForce}, mouseVertical={mouseVertical}, mouseHorizontal={mouseHorizontal}");
        }
    }

    // Activa el brazo (GameObject y script)
    public void ActivateArm()
    {
        gameObject.SetActive(true);
        enabled = true;
        if (ballRb != null)
            ballRb.isKinematic = false; // Permite física al activar
        Debug.Log("MoveArmScript activado.");
    }

    // Desactiva el brazo (GameObject y script)
    public void DeactivateArm()
    {
        enabled = false;
        if (ballRb != null)
            ballRb.isKinematic = true; // Opcional: bloquea física al desactivar
        gameObject.SetActive(false);
        Debug.Log("MoveArmScript desactivado.");
    }
}