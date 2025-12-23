using System.Collections;
using UnityEngine;

public class BedScript : MonoBehaviour, IInteractable
{
    private bool isSleeping = false; // Añadido para evitar múltiples ejecuciones

    public string GetName()
    {
        throw new System.NotImplementedException();
    }

    public void OnInteract()
    {
        if (isSleeping) return; // Evita múltiples ejecuciones

        if (ClockScript.Instance != null && ClockScript.Instance.ClosedBarTime && ClockScript.Instance.FallingAsleep)
        {
            StartCoroutine(SleepSequence());
        }
        else
        {
            Debug.Log("No es de noche, no puedes ir a dormir ahora.");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator SleepSequence()
    {
        isSleeping = true;
        yield return ScreenFader.Instance.FadeOut();
        // Aquí puedes poner animación de cerrar ojos, sonido, etc.
        ClockScript.Instance.NextDay();
        yield return new WaitForSeconds(0.5f); // Espera opcional
        yield return ScreenFader.Instance.FadeIn();
        isSleeping = false;
    }
}
