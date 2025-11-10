using UnityEngine;

public class DialogScript : MonoBehaviour, IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnInteract()
    {
        Debug.Log("Interacted with dialog object: " + gameObject.name);
        // Aquí puedes agregar la lógica para iniciar un diálogo
    }
    public string GetName()
    {
        return "Dialog Object";
    }
}
