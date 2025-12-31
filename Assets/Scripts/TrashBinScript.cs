using UnityEngine;

public class TrashBinScript : MonoBehaviour
{
     DirtynessScript dirtyness;

    void Start()
    {
                
        GameObject dirtynessObj = GameObject.FindObjectOfType<DirtynessScript>()?.gameObject;
        if (dirtynessObj != null)
        {
            dirtyness = dirtynessObj.GetComponent<DirtynessScript>();
        }

    }
    void OnTriggerEnter(Collider other)
 {
    if (other.CompareTag("Dirt") || other.CompareTag("Trash"))
    {
        if (dirtyness != null)
            dirtyness.CleanDirt(other.gameObject);

        // Destroy(other.gameObject); // Ya lo destruye CleanDirt
    }
 }
}
