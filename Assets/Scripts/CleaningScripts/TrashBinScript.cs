using UnityEngine;

// Script encargado de gestionar el contenedor de basura en el juego.
// Permite limpiar objetos de suciedad o basura cuando estos entran en el área del cubo.
// Al detectar la entrada de un objeto con tag "Dirt" o "Trash", llama a DirtynessScript para eliminarlo del bar.

public class TrashBinScript : MonoBehaviour
{
    DirtynessScript dirtyness;

    // Busca la referencia al DirtynessScript al iniciar
    void Start()
    {
        GameObject dirtynessObj = GameObject.FindObjectOfType<DirtynessScript>()?.gameObject;
        if (dirtynessObj != null)
        {
            dirtyness = dirtynessObj.GetComponent<DirtynessScript>();
        }
    }

    // Cuando un objeto entra en el trigger del cubo de basura, lo limpia si es suciedad o basura
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
