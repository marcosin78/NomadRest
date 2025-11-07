using UnityEngine;
using System.Collections;
public class BeerThrowerScript : MonoBehaviour
{
    public GameObject beerLiquidPrefab;
    BeerDispenserScript BeerDispenserScript;

    PlayerController player;
    MoveArmScript moveArm;
    public float throwForce = 0.3f;
    public int throwBeerQuantity = 1;
    public float angleRange = 60f; // Rango máximo de ángulo en grados
    public int angleOffset = 0;
    public float destroyDelay = 3f; // Segundos antes de eliminar cada cubo
    public float cooldownTime = 0.5f; // Cambia a float para mayor precisión
    public float directionLerpTime = 0.5f; // Tiempo para interpolar la dirección    


    private Vector3 currentDirection = Vector3.down;
    private Coroutine throwRoutine;
    private Coroutine directionRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        BeerDispenserScript = FindObjectOfType<BeerDispenserScript>();
        player = FindObjectOfType<PlayerController>();
        moveArm = player.GetComponentInChildren<MoveArmScript>(true);

        if (BeerDispenserScript != null)
        {
            Debug.Log("BeerDispenserScript found.");
        }
        else
        {
            Debug.LogWarning("BeerDispenserScript not found.");
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (BeerDispenserScript.isDispensing)
        {
            if (throwRoutine == null && directionRoutine == null)
            {
                throwRoutine = StartCoroutine(ThrowBeerCoroutine());
                directionRoutine = StartCoroutine(DirectionCoroutine());
            }
            BeerDispenserScript.isDispensing = false;
            Debug.Log("Dispensando cerveza...");
        }

    }


    //Rutina para lanzar la cerveza
    IEnumerator ThrowBeerCoroutine()
    {
        if (beerLiquidPrefab != null)
        {

            //Bloqueando camara
            player.LockCamera();

            moveArm.ActivateArm();

            int thrown = 0;
            
            while (thrown < throwBeerQuantity)
            {
                GameObject beerInstance = Instantiate(beerLiquidPrefab, transform.position, transform.rotation);
                Rigidbody rb = beerInstance.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddForce(currentDirection * throwForce, ForceMode.VelocityChange);
                }

                StartCoroutine(DestroyAfterDelay(beerInstance, destroyDelay));
                thrown++;
                yield return new WaitForSeconds(cooldownTime);
            }
                
            // Cuando termina, para la rutina de dirección
            if (directionRoutine != null)
            {
                StopCoroutine(directionRoutine);
                directionRoutine = null;
            }
            throwRoutine = null;


            //Desactivar brazo
            moveArm.DeactivateArm();

            // Desbloquea la cámara al finalizar
            player.UnlockCamera();
        }
        else
        {
            Debug.LogWarning("beerLiquidPrefab is not assigned.");
        }
    }
    
    //Redirecciona la cerveza lanzada cada cierto tiempo
    IEnumerator DirectionCoroutine()
    {
        while (true)
        {
            // Genera una nueva dirección aleatoria
            float randomAngleX = Random.Range(-angleRange, angleRange);
            float randomAngleZ = Random.Range(-angleRange, angleRange);
            Vector3 targetDirection = Quaternion.Euler(randomAngleX, 0, randomAngleZ) * Vector3.down;

            // Interpola suavemente desde la dirección actual a la nueva
            float elapsed = 0f;
            Vector3 startDirection = currentDirection;
            while (elapsed < directionLerpTime)
            {
                currentDirection = Vector3.Slerp(startDirection, targetDirection, elapsed / directionLerpTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            currentDirection = targetDirection;
        }
    }

    //Destruye el objeto después de un retraso
    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            Destroy(obj);
    }
}

