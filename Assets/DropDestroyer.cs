using UnityEngine;

public class DropDestroyer : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BeerDrop"))
        {
            Destroy(other.gameObject);
            Debug.Log("Beer drop destroyed upon entering destroy zone.");
        }
    }

    
}