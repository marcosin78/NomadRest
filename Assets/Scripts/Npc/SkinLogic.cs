using UnityEngine;

// Script encargado de gestionar la lógica visual de los NPCs mediante sprites orientados.
// Cambia el sprite mostrado según la dirección relativa entre el NPC y el objetivo (normalmente el jugador).
// Permite interpolar la rotación visual para suavizar el cambio de dirección y dibuja sectores de orientación en el editor.
public class SkinLogic : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite frontSprite;
    public Sprite frontLeftSprite;
    public Sprite leftSprite;
    public Sprite backLeftSprite;
    public Sprite backSprite;
    public Sprite backRightSprite;
    public Sprite rightSprite;
    public Sprite frontRightSprite;

    public Transform lookTarget;
    public float maxSpriteRotation = 30f;
    public float rotationLerpSpeed = 10f;

    // Ángulos para 8 direcciones (en grados)
    private float[] spriteAngles = new float[] { -22.5f, 22.5f, 67.5f, 112.5f, 157.5f, -157.5f, -112.5f, -67.5f };

    private float currentVisualAngle = 0f;

    // Busca el objetivo a mirar (por defecto el jugador) al iniciar
    void Start()
    {
        if (lookTarget == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
                lookTarget = player.transform;
        }
    }

    // Actualiza el sprite y la rotación visual cada frame según la dirección al objetivo
    void Update()
    {
        if (lookTarget == null || spriteRenderer == null)
            return;

        // Vector hacia el jugador (plano XZ)
        Vector3 toTarget = lookTarget.position - transform.position;
        toTarget.y = 0;
        if (toTarget == Vector3.zero) toTarget = transform.forward;

        // Billboard: rota el SpriteRenderer para mirar siempre al jugador (manteniendo el eje Y)
        Vector3 lookDir = lookTarget.position - spriteRenderer.transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDir);
            Vector3 euler = lookRotation.eulerAngles;
            spriteRenderer.transform.rotation = Quaternion.Euler(0, euler.y, 0);
        }

        Vector3 npcForward = transform.forward;
        npcForward.y = 0;

        float realAngle = Vector3.SignedAngle(npcForward, toTarget, Vector3.up);

        // Interpola el ángulo visual para suavizar la rotación
        currentVisualAngle = Mathf.LerpAngle(currentVisualAngle, realAngle, Time.deltaTime * rotationLerpSpeed);

        // Decide el sprite según el ángulo real (8 direcciones)
        Sprite targetSprite = frontSprite;
        if (realAngle >= -22.5f && realAngle < 22.5f)
            targetSprite = frontSprite;
        else if (realAngle >= 22.5f && realAngle < 67.5f)
            targetSprite = frontRightSprite;
        else if (realAngle >= 67.5f && realAngle < 112.5f)
            targetSprite = rightSprite;
        else if (realAngle >= 112.5f && realAngle < 157.5f)
            targetSprite = backRightSprite;
        else if (realAngle >= 157.5f || realAngle < -157.5f)
            targetSprite = backSprite;
        else if (realAngle >= -157.5f && realAngle < -112.5f)
            targetSprite = backLeftSprite;
        else if (realAngle >= -112.5f && realAngle < -67.5f)
            targetSprite = leftSprite;
        else if (realAngle >= -67.5f && realAngle < -22.5f)
            targetSprite = frontLeftSprite;

        spriteRenderer.sprite = targetSprite;
        spriteRenderer.flipX = false;

        // Aplica la inclinación visual (rotación Z) según el ángulo visual interpolado
        float visualZ = Mathf.Clamp(currentVisualAngle, -maxSpriteRotation, maxSpriteRotation);
        spriteRenderer.transform.localEulerAngles = new Vector3(0, spriteRenderer.transform.localEulerAngles.y, visualZ);
    }

    // Dibuja en el editor los sectores de orientación y las líneas de referencia
    void OnDrawGizmosSelected()
    {
        if (spriteRenderer == null)
            return;

        Vector3 origin = transform.position;
        Vector3 npcForward = transform.forward;
        npcForward.y = 0;
        npcForward.Normalize();

        float[] angles = new float[] { -22.5f, 22.5f, 67.5f, 112.5f, 157.5f, -157.5f, -112.5f, -67.5f };
        Color[] colors = new Color[] {
            Color.green, Color.yellow, Color.cyan, Color.magenta,
            Color.red, Color.blue, Color.gray, Color.white
        };

        for (int i = 0; i < angles.Length; i++)
        {
            Gizmos.color = colors[i % colors.Length];
            Gizmos.DrawLine(origin, origin + Quaternion.AngleAxis(angles[i], Vector3.up) * npcForward * 2f);
        }

        // Dibuja arcos para los sectores
        DrawSectorArc(origin, npcForward, -22.5f, 22.5f, 2f, Color.green); // Frente
        DrawSectorArc(origin, npcForward, 22.5f, 67.5f, 2f, Color.yellow); // Frente-derecha
        DrawSectorArc(origin, npcForward, 67.5f, 112.5f, 2f, Color.cyan); // Derecha
        DrawSectorArc(origin, npcForward, 112.5f, 157.5f, 2f, Color.magenta); // Atrás-derecha
        DrawSectorArc(origin, npcForward, 157.5f, -157.5f, 2f, Color.red); // Atrás
        DrawSectorArc(origin, npcForward, -157.5f, -112.5f, 2f, Color.blue); // Atrás-izquierda
        DrawSectorArc(origin, npcForward, -112.5f, -67.5f, 2f, Color.gray); // Izquierda
        DrawSectorArc(origin, npcForward, -67.5f, -22.5f, 2f, Color.white); // Frente-izquierda
    }

    // Dibuja un arco de sector en el editor para visualizar la orientación
    void DrawSectorArc(Vector3 origin, Vector3 forward, float startAngle, float endAngle, float radius, Color color)
    {
        Gizmos.color = color;
        int segments = 20;
        Vector3 prevPoint = origin + Quaternion.AngleAxis(startAngle, Vector3.up) * forward * radius;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            Vector3 nextPoint = origin + Quaternion.AngleAxis(angle, Vector3.up) * forward * radius;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}