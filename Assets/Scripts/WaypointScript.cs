using UnityEngine;

public class WaypointScript : MonoBehaviour, IWaypointProvider
{
    [SerializeField]
    private bool isAvailable = true;
    public bool IsAvailable => isAvailable;
    public Transform Waypoint => transform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

            //Añadir en un futuro más condicionales para el script(De momento asumo que siempre esta disponible)

    }

    void OnTriggerEnter(Collider other)
    {

        if (other.GetComponent<BeerDrinkingScript>() != null)
        {
            SetAvailability(false);
            Debug.Log("Waypoint " + gameObject.name + " ocupado por " + other.name);

            // Suscribirse al evento de destrucción del NPC
            BeerDrinkingScript npc = other.GetComponent<BeerDrinkingScript>();
            if (npc != null)
            {
                npc.OnDestroyed += OnNpcDestroyed;
            }
        }
    }
     // Marca como disponible al salir el NPC
    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BeerDrinkingScript>() != null)
        {
            SetAvailability(true);
            Debug.Log("Waypoint " + gameObject.name + " libre por " + other.name);

            // Desuscribirse del evento de destrucción del NPC
            BeerDrinkingScript npc = other.GetComponent<BeerDrinkingScript>();
            if (npc != null)
            {
                npc.OnDestroyed -= OnNpcDestroyed;
            }
        }
    }
    public void SetAvailability(bool availability)
    {
        isAvailable = availability;
    }
    // Este método se llama cuando el NPC es destruido
    private void OnNpcDestroyed()
    {
        SetAvailability(true);
        
    }
}
