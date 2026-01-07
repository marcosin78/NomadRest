using UnityEngine;

// Script encargado de gestionar la lógica de la mopa en el juego.
// Permite al jugador equipar la mopa si está cerca, y soltarla cuando lo desee.
// Al equipar la mopa, se bloquean las manos del jugador y la mopa se coloca en la mano.
// Al soltarla, se reactiva el collider y el rigidbody, y la mopa se coloca delante del jugador.

public class MopScript : MonoBehaviour
{
    public bool grabbingMop = false;
    PlayerController playerController;

    public bool mopAttached = false;
    public GameObject currentDirt = null;
    public Vector3 attachDirection;
    public float detachAngle = 40f; // Ángulo máximo permitido antes de soltar la mopa

    // Inicializa la referencia al PlayerController
    void Start()
    {
        if (playerController == null)
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    // Controla el equipamiento y soltado de la mopa según la distancia y teclas pulsadas
    void Update()
    {
        // Equipar la mopa con E si está cerca y no la tienes ya
        if (!grabbingMop && Input.GetKeyDown(KeyCode.E) && IsPlayerNear())
        {
            EquipMop();
        }

        // Soltar la mopa con Q si la tienes equipada
        if (grabbingMop && Input.GetKeyDown(KeyCode.Q))
        {
            DropMop();
        }
    }

    // Equipa la mopa, la coloca en la mano y desactiva físicas y collider
    void EquipMop()
    {
        grabbingMop = true;
        playerController.availableHands = false;

        // Haz hijo de HoldPoint y coloca la mopa en la mano
        transform.SetParent(playerController.HoldPoint);

        // Valores ajustados para que quede bien en la mano
        transform.localPosition = new Vector3(1, -0.25f, -0.15f); 
        transform.localRotation = Quaternion.Euler(-72, -12, -15);

        // Desactiva el collider para que no interfiera
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Desactiva el Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Suelta la mopa, reactiva físicas y collider, y la coloca delante del jugador
    void DropMop()
    {
        grabbingMop = false;
        playerController.availableHands = true;

        // Suelta la mopa en la posición actual del jugador
        transform.SetParent(null);

        // Reactiva el collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // Reactiva el Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Coloca la mopa justo delante del jugador
        transform.position = playerController.transform.position + playerController.transform.forward * 1f;
    }

    // Comprueba si el jugador está lo suficientemente cerca para equipar la mopa
    bool IsPlayerNear()
    {
        // Puedes ajustar la distancia según tu juego
        float distance = Vector3.Distance(transform.position, playerController.transform.position);
        return distance < 2f;
    }
}