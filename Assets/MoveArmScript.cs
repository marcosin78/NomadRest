using UnityEngine;

public class MoveArmScript : MonoBehaviour
{
    public Rigidbody ballRb; // Asigna el Rigidbody de la bola en el inspector
    public float forceMultiplier = 50f;
    public float maxDistance = 50f;
    public float coneAngle = 120f;
    public Camera cam;
    public Vector3 coneOriginOffset = new Vector3(0, 0, -2f); // -2 en Z para que nazca más atrás

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
            ballRb.linearVelocity = Vector3.Lerp(ballRb.linearVelocity, Vector3.zero, 5f * Time.deltaTime);
        }

        lastMousePosition = Input.mousePosition;
    }

    void LateUpdate()
    {
        // Limita la bola al rango de visión de la cámara (cono delante de la cámara)
        if (cam != null && ballRb != null)
        {
            Vector3 coneOrigin = originPosition + cam.transform.TransformDirection(coneOriginOffset);

            Vector3 camForward = cam.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 toBall = ballRb.transform.position - coneOrigin;
            toBall.y = 0; // Solo plano XZ

            // Limita la distancia máxima
            if (toBall.magnitude > maxDistance)
            {
                toBall = toBall.normalized * maxDistance;
                Vector3 clampedPos = coneOrigin + toBall;
                ballRb.transform.position = new Vector3(clampedPos.x, ballRb.transform.position.y, clampedPos.z);
                ballRb.linearVelocity = Vector3.zero;
            }

            // Limita el ángulo del cono respecto a la cámara
            float angle = Vector3.Angle(camForward, toBall);
            if (angle > coneAngle)
            {
                Vector3 edgeDir = Quaternion.AngleAxis(coneAngle * Mathf.Sign(Vector3.SignedAngle(camForward, toBall, Vector3.up)), Vector3.up) * camForward;
                Vector3 clampedPos = coneOrigin + edgeDir.normalized * toBall.magnitude;
                ballRb.transform.position = new Vector3(clampedPos.x, ballRb.transform.position.y, clampedPos.z);
                ballRb.linearVelocity = Vector3.zero;

                Debug.Log("Bola limitada al borde del cono de la cámara.");
            }
        }
    }

    void ApplyPhysics()
    {
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

    void OnDrawGizmos()
    {
        if (cam == null)
            return;

        Vector3 coneOrigin = originPosition + cam.transform.TransformDirection(coneOriginOffset);

        // Dibuja el origen
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(coneOrigin, 0.1f);

        // Dibuja el cono
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 origin = coneOrigin;
        float angleStep = 5f;
        int segments = Mathf.CeilToInt(coneAngle * 2 / angleStep);

        Gizmos.color = Color.yellow;
        for (int i = 0; i <= segments; i++)
        {
            float angle = -coneAngle + i * angleStep;
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * camForward;
            Vector3 end = origin + dir.normalized * maxDistance;
            Gizmos.DrawLine(origin, end);
        }

        // Dibuja el arco del borde del cono
        Vector3 prevPoint = origin + Quaternion.AngleAxis(-coneAngle, Vector3.up) * camForward * maxDistance;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -coneAngle + i * angleStep;
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * camForward;
            Vector3 nextPoint = origin + dir.normalized * maxDistance;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}