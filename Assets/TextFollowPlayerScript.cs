using UnityEngine;

public class TextFollowPlayerScript : MonoBehaviour
{
    public Transform player; // Asigna el transform del jugador en el inspector

    void Start()
    {
        if (player == null)
        {
            PlayerController foundPlayer = FindAnyObjectByType<PlayerController>();
            if (foundPlayer != null)
                player = foundPlayer.transform;
            else
                Debug.LogError("Player transform not assigned in TextFollowPlayerScript.");
        }
    }

    void Update()
    {
        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                // Rota completamente hacia el jugador (incluyendo eje X)
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}