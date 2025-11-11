using System;
using UnityEngine;

public class GenericDialogScript : MonoBehaviour
{

    public Transform bubblePoint;
    public String dialogText = "Hello, this is a generic dialog.";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void StartGenericDialog()
    {
        Debug.Log("¡Has iniciado un diálogo genérico con este NPC!");
    }
}
