using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Script encargado de gestionar el lanzamiento de la cerveza durante el minijuego.
// Controla la creación de gotas, la dirección y fuerza del lanzamiento, la recogida de cerveza y la entrega final al jugador.
public class BeerThrowerScript : MonoBehaviour
{
    public GameObject beerLiquidPrefab; // Prefab de la gota de cerveza a lanzar
    BeerDispenserScript BeerDispenserScript; // Referencia al dispensador de cerveza
    LiquidDetector liquidDetector; // Detector de líquido para medir el éxito del minijuego
    PlayerController player; // Referencia al jugador
    MoveArmScript moveArm; // Script para animar el brazo del jugador

    public float throwForce = 0.3f; // Fuerza con la que se lanza cada gota
    public int throwBeerQuantity = 1; // Número de gotas a lanzar
    public float angleRange = 60f; // Rango máximo de ángulo en grados para la dirección aleatoria
    public int angleOffset = 0;
    public float destroyDelay = 3f; // Segundos antes de eliminar cada gota
    public float cooldownTime = 0.5f; // Tiempo entre lanzamientos de gotas
    public float directionLerpTime = 0.5f; // Tiempo para interpolar la dirección de lanzamiento
    public float cancelDistance = 3f; // Distancia máxima permitida al dispensador para lanzar

    private Vector3 currentDirection = Vector3.down; // Dirección actual de lanzamiento
    private Coroutine throwRoutine;
    private Coroutine directionRoutine;

    // Inicializa referencias a los scripts necesarios al iniciar
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

    // Si el dispensador está en modo de servir, inicia las rutinas de lanzamiento y dirección
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

    // Rutina para lanzar las gotas de cerveza, comprobar distancia y entregar la bebida final al jugador
    IEnumerator ThrowBeerCoroutine()
    {
        if (beerLiquidPrefab != null)
        {
            Debug.Log("Iniciando lanzamiento de cerveza.");
            player.LockCamera();
            moveArm.ActivateArm();
            liquidDetector = FindObjectOfType<LiquidDetector>();
            liquidDetector.setTotalDrops(throwBeerQuantity);

            int thrown = 0;
            bool cancelled = false;
            GameObject beerInstance = null;

            while (thrown < throwBeerQuantity)
            {
                // Comprueba si el jugador se aleja demasiado del dispensador
                float distance = Vector3.Distance(player.transform.position, BeerDispenserScript.transform.position);
                if (distance > cancelDistance)
                {
                    Debug.Log("Demasiado lejos del dispensador, se cancela el lanzamiento.");
                    cancelled = true;
                    break;
                }

                beerInstance = Instantiate(beerLiquidPrefab, transform.position, transform.rotation);
                Rigidbody rb = beerInstance.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddForce(currentDirection * throwForce, ForceMode.VelocityChange);
                }

                // Asigna los ingredientes a la gota lanzada
                var data = beerInstance.AddComponent<BeerCocktailData>();
                data.ingredientIDs = new List<int>(BeerDispenserScript.lastUsedIngredients);

                StartCoroutine(DestroyAfterDelay(beerInstance, destroyDelay));
                thrown++;
                yield return new WaitForSeconds(cooldownTime);
            }

            if (directionRoutine != null)
            {
                StopCoroutine(directionRoutine);
                directionRoutine = null;
            }
            throwRoutine = null;
            Debug.Log("Ingredientes que se asignan al BeerCocktailData: " + string.Join(",", BeerDispenserScript.lastUsedIngredients));
            moveArm.DeactivateArm();
            player.UnlockCamera();

            if (!cancelled)
            {
                float fillPercent = liquidDetector.GetFillPercent();
                Debug.Log($"Porcentaje de gotas recogidas: {fillPercent * 100f}%");

                if (fillPercent < 0.2f)
                {
                    Debug.Log("El jugador ha fallado al recoger suficiente cerveza.");
                }
                else
                {
                    Debug.Log("El jugador ha recogido suficiente cerveza.");
                    if (BeerDispenserScript != null && BeerDispenserScript.drinkPrefabs.Length > BeerDispenserScript.selectedDrink)
                    {
                        // Instancia el objeto de cerveza real y lo entrega al jugador
                        GameObject beerObject = Instantiate(
                            BeerDispenserScript.drinkPrefabs[BeerDispenserScript.selectedDrink],
                            player.HoldPoint.position,
                            Quaternion.identity
                        );

                        var data = beerObject.GetComponent<BeerCocktailData>();
                        if (data == null)
                            data = beerObject.AddComponent<BeerCocktailData>();
                        data.ingredientIDs = new List<int>(BeerDispenserScript.lastUsedIngredients);

                        player.TakeItem(beerObject);
                        Debug.Log("Cerveza entregada al jugador con ingredientes: " + string.Join(",", data.ingredientIDs));
                    }
                }
            }
            else
            {
                Debug.Log("Lanzamiento cancelado por distancia.");
            }
        }
        else
        {
            Debug.LogWarning("beerLiquidPrefab is not assigned.");
        }
    }

    // Rutina que cambia la dirección de lanzamiento de forma aleatoria e interpolada
    IEnumerator DirectionCoroutine()
    {
        while (true)
        {
            float randomAngleX = Random.Range(-angleRange, angleRange);
            float randomAngleZ = Random.Range(-angleRange, angleRange);
            Vector3 targetDirection = Quaternion.Euler(randomAngleX, 0, randomAngleZ) * Vector3.down;

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

    // Destruye el objeto después de un retraso
    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            Destroy(obj);
    }
}

