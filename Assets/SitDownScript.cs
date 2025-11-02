using UnityEngine;
using UnityEngine.XR;

public class SitDownScript : MonoBehaviour
{

    Transform targetSeat; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
       


    }

    // Update is called once per frame
    void Update()
    {

    }

    void HandleContact(Transform targetSeat)
    {

        if(targetSeat.CompareTag("SEAT"))
        {
            Debug.Log("Colliding with: " + targetSeat.name);
        }

    }
    void OnCollisionEnter(Collision collision)
    {

        HandleContact(collision.transform);
    }
    void OnTriggerEnter(Collider other)
    {
        HandleContact(other.transform);
    }
}
