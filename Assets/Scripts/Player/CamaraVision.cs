using UnityEngine;

public class CamaraVision : MonoBehaviour
{

    public static float sensibilidad=400f;
    public Transform playerBody; // Assign the player object in the inspector
    float xRotation = 0f;
    float yRotation = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {

       float mouseY = Input.GetAxis("Mouse Y") * sensibilidad * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X") * sensibilidad * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Only set the camera's local rotation once
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player horizontally
        playerBody.Rotate(Vector3.up * mouseX);


    }
}
