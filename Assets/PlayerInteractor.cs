using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
   [Tooltip("Camera transform used as ray origin. If null will use Camera.main.")]
    public Transform cameraTransform;
    public float interactDistance = 3f;
    public LayerMask hitMask = ~0; // default: everything

    // Last detected interactable component (null when none)
    public IInteractable CurrentInteractable { get; private set; }
    public GameObject CurrentHitObject { get; private set; }

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        CurrentInteractable = null;
        CurrentHitObject = null;

        if (cameraTransform == null)
            return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            CurrentHitObject = hit.collider.gameObject;
            // Try interface first
            IInteractable i = hit.collider.GetComponent<IInteractable>();
            if (i != null)
            {
                CurrentInteractable = i;
            }
            else
            {      
                    // Optional: use a small helper component to hold name/behaviour
                    CurrentHitObject = hit.collider.gameObject;
                
            }
        }

        // Example: press E to interact
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (CurrentInteractable != null)
            {
                CurrentInteractable.OnInteract();
            }
            else if (CurrentHitObject != null && CurrentHitObject.CompareTag("Interactable"))
            {
                Debug.Log("Interacted with tag-based object: " + CurrentHitObject.name);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Transform t = cameraTransform ? cameraTransform : (Camera.main ? Camera.main.transform : null);
        if (t == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(t.position, t.forward * interactDistance);
    }
}

