using UnityEngine;

public class TrashBinScript : MonoBehaviour
{
     void OnTriggerEnter(Collider other)
    {
        // Solo elimina si el objeto es basura (puedes usar tag o componente)
        if (other.CompareTag("Dirt") || other.CompareTag("Trash"))
        {
            Destroy(other.gameObject);
            Debug.Log("Basura eliminada: " + other.gameObject.name);
        }
    }
}
