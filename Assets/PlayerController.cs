using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    int movementSpeed = 5;
    Vector3 movement;
    private Rigidbody rb;

    public bool availableHands = true;
    public Transform HoldPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

    }

    // Update is called once per frame
    void Update()
    {

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

}
