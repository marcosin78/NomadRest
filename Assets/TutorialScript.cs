using UnityEngine;
using System.Collections;

public class TutorialScript : MonoBehaviour
{
    public Transform teleportPositionsParent; // Asigna el EmptyObject en el inspector
    public ParticleSystem teleportParticlesPrefab; // Asigna el prefab de partículas en el inspector
    public float particleDuration = 1.5f; // Duración de la animación de partículas

    /// <summary>
    /// Llama a esto desde el DialogNode para hacer el efecto y teletransportar.
    /// </summary>
    public void TeleportWithParticles(Transform target, int positionIndex)
    {
        StartCoroutine(TeleportWithParticlesCoroutine(target, positionIndex));
    }

    private IEnumerator TeleportWithParticlesCoroutine(Transform target, int positionIndex)
    {
        // Instancia las partículas en la posición actual del target
        if (teleportParticlesPrefab != null && target != null)
        {
            ParticleSystem particles = Instantiate(teleportParticlesPrefab, target.position, Quaternion.identity);
            particles.Play();
            yield return new WaitForSeconds(particleDuration);
            Destroy(particles.gameObject);
        }

        // Teletransporta después de la animación
        TeleportToPosition(target, positionIndex);
    }

    /// <summary>
    /// Teletransporta el objeto dado a la posición del hijo con el índice indicado.
    /// </summary>
    public void TeleportToPosition(Transform target, int positionIndex)
    {
        if (teleportPositionsParent == null)
        {
            Debug.LogWarning("No se ha asignado teleportPositionsParent.");
            return;
        }

        if (positionIndex < 0 || positionIndex >= teleportPositionsParent.childCount)
        {
            Debug.LogWarning("Índice de posición fuera de rango.");
            return;
        }

        Transform targetPosition = teleportPositionsParent.GetChild(positionIndex);
        target.position = targetPosition.position;
        target.rotation = targetPosition.rotation; // Opcional: también rota al objetivo
    }
}
