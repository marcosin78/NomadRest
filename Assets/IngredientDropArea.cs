using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class IngredientDropArea : MonoBehaviour
{


    //SCRIPT PARA EL ÁREA DONDE SE SUELTAN LOS INGREDIENTES Y SE AGITA EL CÓCTEL
    //PENDIENTE DE MODIFICAR EL COCTEL SE CREA MUY RAPIDO.

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 lastPosition;
    private Rigidbody2D rb;
    public float mass = 1.5f; // Peso del cóctel
    public float shakeMultiplier = 0.00005f; // Ajusta la sensibilidad del shake (más bajo = más lento)

    public BeerMinigameScript beerMinigameScript; // Asigna en el inspector
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.mass = mass;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearDamping = 10f; // Alto damping para frenar rápido
    }

    void Update()
    {
        // Solo permite agarrar si hay al menos 2 ingredientes
        if (CanGrabArea())
        {
            // Mouse 1 presionado sobre el área
            if (Input.GetMouseButtonDown(0))
            {
                // Comprobar si el mouse está sobre este objeto (UI)
                if (IsPointerOverUIElement())
                {
                    isDragging = true;
                    StartShaking();
                    // Calcular offset para que el área no salte al centro del ratón
                    Vector3 mousePos;
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        GetComponent<RectTransform>(),
                        Input.mousePosition,
                        null,
                        out mousePos);
                    offset = transform.position - mousePos;
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        if (isDragging)
        {
            Vector3 mousePos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                GetComponent<RectTransform>(),
                Input.mousePosition,
                null,
                out mousePos);
            Vector3 targetPos = mousePos + offset;
            Vector2 force = (targetPos - transform.position) * rb.mass * 50f; // Ajusta el factor para la "sensación" de peso
            rb.AddForce(force);

            // Calcular distancia movida este frame
            float distance = Vector3.Distance(transform.position, lastPosition);
            // Incrementar shakeProgress según distancia y fuerza
            if (isBeingShaken)
            {
                // El progreso depende solo de la distancia movida, no de la fuerza
                float shakeAmount = distance * shakeMultiplier;
                shakeProgress += shakeAmount;
                Debug.Log($"Shake Progress: {shakeProgress:F3}");
                if (shakeProgress >= shakeRequired)
                {
                    shakeProgress = shakeRequired;
                    isBeingShaken = false;
                    Debug.Log("Cocktail is ready!");

                    beerMinigameScript.OnMinigameComplete(addedIngredientIDs);
                }
            }
            lastPosition = transform.position;
        }
    }

    // Comprueba si el puntero está sobre este UI (DropArea)
    private bool IsPointerOverUIElement()
    {
        // Raycast UI para ver si el mouse está sobre este objeto
        var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = Input.mousePosition
        };
        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
        foreach (var r in results)
        {
            if (r.gameObject == gameObject)
                return true;
        }
        return false;
    }
    public System.Collections.Generic.List<int> addedIngredientIDs = new System.Collections.Generic.List<int>();

    // Devuelve true si se añadió, false si no se puede añadir
    public bool AddIngredient(int id, Sprite sprite)
    {
        // Obtener el tipo del ingrediente
        var data = ItemDatabase.Instance.GetItemById(id);
        if (data == null)
            return false;

        string tipo = data.ingredientType; // Usar el campo correcto de ItemData

        // Comprobar si ya hay un ingrediente de ese tipo
        foreach (int addedId in addedIngredientIDs)
        {
            var addedData = ItemDatabase.Instance.GetItemById(addedId);
            if (addedData != null && addedData.ingredientType == tipo)
            {
                // Ya hay uno de este tipo
                return false;
            }
        }

        addedIngredientIDs.Add(id);
        // Opcional: muestra el sprite en el área
        GameObject imgObj = new GameObject("AddedIngredient");
        imgObj.transform.SetParent(transform, false);
        var img = imgObj.AddComponent<Image>();
        img.sprite = sprite;
        imgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
        return true;
    }

    // Limpia los ingredientes y sprites visuales del área
    public void ClearIngredients()
    {
        addedIngredientIDs.Clear();
        // Elimina los GameObjects hijos creados para los sprites
        foreach (Transform child in transform)
        {
            if (child.name == "AddedIngredient")
                GameObject.Destroy(child.gameObject);
        }
    }
     // Porcentaje de movilidad (0 a 1) para simular el shake del cóctel
        [Range(0,1)]
        public float shakeProgress = 0f;
        public float shakeRequired = 1f; // 100% para completar
        public bool isBeingShaken = false;

        // Llamar a esto para intentar agarrar el área
        public bool CanGrabArea()
        {
            return addedIngredientIDs.Count >= 2;
        }

        // Llamar a esto para iniciar el movimiento de shake
        public void StartShaking()
        {
            if (CanGrabArea())
            {
                isBeingShaken = true;
                shakeProgress = 0f;
                lastPosition = transform.position;
            }
        }

        // Llamar a esto cada frame mientras se agita
        public void UpdateShaking(float shakeAmount)
        {
            if (isBeingShaken)
            {
                shakeProgress += shakeAmount;
                Debug.Log("Shake Progress: " + shakeProgress);
                if (shakeProgress >= shakeRequired)
                {
                    shakeProgress = shakeRequired;
                    isBeingShaken = false;
                    // Aquí puedes lanzar evento de "cóctel listo"
                    Debug.Log("Cocktail is ready!");
                }
            }
        }
}