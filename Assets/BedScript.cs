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
            ClockScript.Instance.NextDay();
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
}
