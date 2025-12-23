using System.Collections;
using UnityEngine;

public class BedScript : MonoBehaviour, IInteractable
{
    public string GetName()
    {
        throw new System.NotImplementedException();
    }

    public void OnInteract()
    {
            if (ClockScript.Instance != null && ClockScript.Instance.ClosedBarTime && ClockScript.Instance.FallingAsleep)
        {
            ClockScript.Instance.NextDay();
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
    yield return ScreenFader.Instance.FadeOut();
    // Aquí puedes poner animación de cerrar ojos, sonido, etc.
    ClockScript.Instance.NextDay();
    yield return new WaitForSeconds(0.5f); // Espera opcional
    yield return ScreenFader.Instance.FadeIn();
}
}
