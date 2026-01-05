using UnityEngine;

public class LogObject : MonoBehaviour, IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    public void OnInteract()
    {
        Debug.Log("Interactuando con:  " + gameObject.name);
    }
    public string GetName()
    {
        return "Objeto de registro";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
