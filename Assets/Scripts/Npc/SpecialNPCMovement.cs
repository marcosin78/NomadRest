using UnityEngine;

public class SpecialNPCMovement : MonoBehaviour
{
    public Vector3 initialTarget;
    public Vector3 exitTarget;
    public float speed = 2f;

    private bool canInteract = false;
    private bool isExiting = false;
    private Rigidbody rb;
    private Vector3 currentTarget;
    private bool moving = false;

    public delegate void InteractionStateChanged(bool interactable);
    public event InteractionStateChanged OnInteractionStateChanged;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Buscar el punto inicial por tag
        GameObject targetPoint = GameObject.FindGameObjectWithTag("SpecialNPCPoint");
        if (targetPoint != null)
        {
            initialTarget = targetPoint.transform.position;
            canInteract = false;
            MoveTo(initialTarget, () =>
            {
                canInteract = true;
                OnInteractionStateChanged?.Invoke(true);
            });
        }
        else
        {
            Debug.LogWarning("No se encontró ningún GameObject con el tag 'SpecialNPCPoint' para el NPC especial.");
        }
    }

    void FixedUpdate()
    {
        if (!moving) return;

        Vector3 direction = (currentTarget - transform.position);
        direction.y = 0; // Opcional: ignora altura
        float distance = direction.magnitude;

        if (distance < 0.1f)
        {
            rb.linearVelocity = Vector3.zero;
            moving = false;
            onArriveCallback?.Invoke();
            onArriveCallback = null;
            return;
        }

        Vector3 velocity = direction.normalized * speed;
        rb.linearVelocity = velocity;
        // Opcional: rotar hacia el movimiento
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(velocity, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.fixedDeltaTime);
        }
    }

    private System.Action onArriveCallback;

    void MoveTo(Vector3 target, System.Action onArrive)
    {
        currentTarget = target;
        onArriveCallback = onArrive;
        moving = true;
    }

    public bool IsInteractable()
    {
        return canInteract;
    }

    public void OnConversationEnded()
    {
        if (!isExiting)
        {
            canInteract = false;
            OnInteractionStateChanged?.Invoke(false);
            isExiting = true;
            MoveTo(exitTarget, () =>
            {
                Destroy(gameObject);
            });
        }
    }
}
