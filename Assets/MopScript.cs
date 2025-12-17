using UnityEngine;

public class MopScript : MonoBehaviour, IGrabbable
{
    public bool grabbingMop = false;
    PlayerController playerController;


    public bool mopAttached = false;
    public GameObject currentDirt = null;
    public Vector3 attachDirection;
    public float detachAngle = 40f; // Ángulo máximo permitido antes de soltar la mopa

    void Start()
    {
        if (playerController == null)
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

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

    void EquipMop()
 {
    grabbingMop = true;
    playerController.availableHands = false;

    // Haz hijo de HoldPoint y coloca la mopa en la mano
    transform.SetParent(playerController.HoldPoint);

    //Valores ajustados para que quede bien en la mano
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

    bool IsPlayerNear()
    {
        // Puedes ajustar la distancia según tu juego
        float distance = Vector3.Distance(transform.position, playerController.transform.position);
        return distance < 2f;
    }
}

internal interface IGrabbable
{
}