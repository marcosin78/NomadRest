using System;
using UnityEngine;
using TMPro;

// Script encargado de mostrar diálogos genéricos en NPCs simples.
// Permite mostrar frases aleatorias en una burbuja de diálogo durante un tiempo determinado.
public class GenericDialogScript : MonoBehaviour
{
    [TextArea]
    public string[] dialogTexts = new string[3]
    {
        "Hola Generico1.",
        "Hola Generico2.",
        "Hola Generico3."
    };
    public TextMeshProUGUI dialogUIText;
    public Canvas dialogCanvas; // Asigna el Canvas de la burbuja en el inspector
    public float displayTime = 3f; // Segundos que se muestra el diálogo

    private float timer = 0f;
    private bool isDialogActive = false;

    void Start()
    {
        if (dialogCanvas != null)
            dialogCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isDialogActive)
        {
            timer += Time.deltaTime;
            if (timer >= displayTime)
            {
                HideDialog();
            }
        }
    }

    // Inicia el diálogo genérico mostrando una frase aleatoria en la UI
    public void StartGenericDialog()
    {
        string chosenText = dialogTexts.Length > 0
            ? dialogTexts[UnityEngine.Random.Range(0, dialogTexts.Length)]
            : "No hay frases definidas.";

        if (dialogUIText != null)
            dialogUIText.text = chosenText;

        if (dialogCanvas != null)
            dialogCanvas.gameObject.SetActive(true);

        timer = 0f;
        isDialogActive = true;

        Debug.Log("¡Has iniciado un diálogo genérico con este NPC! Frase: " + chosenText);
    }

    // Oculta la burbuja de diálogo y resetea el temporizador
    private void HideDialog()
    {
        if (dialogCanvas != null)
            dialogCanvas.gameObject.SetActive(false);

        isDialogActive = false;
        timer = 0f;
    }
}
