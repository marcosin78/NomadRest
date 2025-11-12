using System;
using UnityEngine;
using TMPro;

public class GenericDialogScript : MonoBehaviour
{
    [TextArea]
    public string[] dialogTexts = new string[3]
    {
        "Hola Guille.",
        "Hola Toni.",
        "Hola Mundo."
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

    private void HideDialog()
    {
        if (dialogCanvas != null)
            dialogCanvas.gameObject.SetActive(false);

        isDialogActive = false;
        timer = 0f;
    }
}
