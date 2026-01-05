using UnityEngine;

public class FollowPlayerScript : MonoBehaviour
{

    public Transform target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target not assigned in FollowPlayerScript.");
        }else
        {
            transform.LookAt(target);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
       transform.LookAt(target);

    }
}
