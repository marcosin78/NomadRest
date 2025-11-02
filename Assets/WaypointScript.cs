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

    // Update is called once per frame
    void Update()
    {

        if(isAvailable == false)
        {
            //Podria añadir algun efecto visual para indicar que no esta disponible


        }

    }
    public void SetAvailability(bool availability)
    {
        isAvailable = availability;
    }
}
