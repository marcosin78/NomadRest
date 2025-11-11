using UnityEngine;

public class DialogScript : MonoBehaviour
{
    public Transform focusPoint;
    private Camera mainCamera;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    private bool isDialogActive = false;

    public bool hasSpecialDialog = false;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null) return;

        if (isDialogActive && focusPoint != null)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, focusPoint.position, Time.unscaledDeltaTime * 5f);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, focusPoint.rotation, Time.unscaledDeltaTime * 5f);
        }

        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q)) && isDialogActive)
        {
            EndDialog();
        }
    }

    public void StartDialog()
    {
        Debug.Log("Dialog started with: " + gameObject.name);
        Time.timeScale = 0f;
        if (mainCamera != null)
        {
            originalCamPosition = mainCamera.transform.position;
            originalCamRotation = mainCamera.transform.rotation;
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
    }
}