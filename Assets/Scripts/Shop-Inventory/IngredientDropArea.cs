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

    private Vector3 initialPosition;
    private Rigidbody2D rb;
    public float mass = 1.5f; // Peso del cóctel
    public float shakeMultiplier = 0.00005f; // Ajusta la sensibilidad del shake (más bajo = más lento)

    public BeerMinigameScript beerMinigameScript; // Asigna en el inspector

    //Logica de Sprites
    public Sprite closedShakerSprite;
    public Sprite openShakerSprite;
    public GameObject shakerSprite;

    public AudioSource playerAudioSource;
    public AudioClip shakerFillSoundClip;
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
        rb.linearDamping = 10f;
        initialPosition = transform.position;

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
                    isDragging = false;

                    if (beerMinigameScript != null)
                {
                    Debug.Log("Llamando a OnMinigameComplete desde IngredientDropArea");
                    beerMinigameScript.OnMinigameComplete(addedIngredientIDs);
                }
                    else
                    {
                    Debug.LogError("beerMinigameScript no está asignado en IngredientDropArea");
                    }  

                }
            }
            lastPosition = transform.position;
        }

        // --- Cambia el sprite según si hay un IngredientButton cerca ---
    bool ingredientNear = false;
    float checkRadius = 100f; // Ajusta según tu escala de UI

    // Busca todos los IngredientButton activos en la escena
    var allButtons = FindObjectsOfType<IngredientButton>();
    Vector2 dropAreaScreenPos = RectTransformUtility.WorldToScreenPoint(null, transform.position);

    foreach (var btn in allButtons)
    {
        if (btn != null && btn.gameObject.activeInHierarchy)
        {
            Vector2 btnScreenPos = RectTransformUtility.WorldToScreenPoint(null, btn.transform.position);
            float dist = Vector2.Distance(btnScreenPos, dropAreaScreenPos);
            if (dist < checkRadius)
            {
                ingredientNear = true;
                break;
            }
        }
    }

        if (shakerSprite != null)
        {
       var img = shakerSprite.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = ingredientNear ? openShakerSprite : closedShakerSprite;
        }
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
    // Estructura para guardar info del botón original
    [System.Serializable]
    public class IngredientButtonInfo
    {
        public int ingredientId;
        public Transform buttonTransform;
        public Transform originalParent;
        public int originalSiblingIndex;
        public Vector3 originalLocalPosition;
    }
    public System.Collections.Generic.List<IngredientButtonInfo> movedIngredientButtons = new System.Collections.Generic.List<IngredientButtonInfo>();

    // Devuelve true si se añadió, false si no se puede añadir
    public bool AddIngredient(int id, Sprite sprite)
    {
        if (InventorySystem.Instance.GetItemCount(id) < 1)
        {
            Debug.Log("No hay suficiente cantidad en el inventario para el ingrediente " + id);
            return false;
        }

        var data = ItemDatabase.Instance.GetItemById(id);
        if (data == null)
        {
            Debug.Log("No se encontró el item en la base de datos: " + id);
            return false;
        }

            // --- Reproducir sonido al añadir ingrediente ---
        if (playerAudioSource != null && shakerFillSoundClip != null)
        playerAudioSource.PlayOneShot(shakerFillSoundClip);

        string tipo = data.ingredientType;
        foreach (int addedId in addedIngredientIDs)
        {
            var addedData = ItemDatabase.Instance.GetItemById(addedId);
            if (addedData != null && addedData.ingredientType == tipo)
            {
                Debug.Log("Ya hay un ingrediente de tipo " + tipo + " en el área.");
                return false;
            }
        }

        addedIngredientIDs.Add(id);

        // NO muevas ni ocultes el botón aquí

        if (playerAudioSource != null && shakerFillSoundClip != null)
        playerAudioSource.PlayOneShot(shakerFillSoundClip);

        // Solo añade el sprite visual al área
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

            isDragging = false;
            addedIngredientIDs.Clear();

        addedIngredientIDs.Clear();
        foreach (var info in movedIngredientButtons)
        {
            if (info.buttonTransform != null && info.originalParent != null)
            {
                var btn = info.buttonTransform.GetComponent<IngredientButton>();
                if (btn != null)
                {
                    btn.originalParent = info.originalParent;
                    btn.originalSiblingIndex = info.originalSiblingIndex;
                    btn.originalLocalPosition = info.originalLocalPosition;
                    btn.AnimateReturnToOriginal(true); // <-- instantáneo
                }
                else
                {
                    // Fallback por si no es un IngredientButton
                    info.buttonTransform.SetParent(info.originalParent, false);
                    info.buttonTransform.SetSiblingIndex(info.originalSiblingIndex);
                    info.buttonTransform.localPosition = info.originalLocalPosition;
                    info.buttonTransform.gameObject.SetActive(true);
                }
            }
        }
        movedIngredientButtons.Clear();
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
                // initialPosition ya no se reasigna aquí
            }
        }

        // Llamar a esto cada frame mientras se agita
        public void UpdateShaking(float shakeAmount)
        {
            if (isBeingShaken)
            {
                shakeProgress += shakeAmount;

                if (shakeProgress >= shakeRequired)
                {
                    shakeProgress = shakeRequired;
                    isBeingShaken = false;

                    // Vuelve a la posición inicial
                    transform.position = initialPosition;
                    rb.linearVelocity = Vector2.zero; 
                }
            }
        }

         // Recoloca el área en su posición inicial
    public void ResetToInitialPosition()
    {
        transform.position = initialPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            Debug.Log("Reset IngredientDropArea to initial position and zeroed velocities.");
        }
        else
        {
            Debug.Log("Reset IngredientDropArea to initial position. no Rigidbody found.");
        }
    }

    /*void OnGUI()  //VER EL RADIO DE DETECCIÓN DE INGREDIENTES CERCANOS
{
    // Visualiza el radio de detección en pantalla
    float checkRadius = 100f; // Usa el mismo valor que en Update
    Vector2 dropAreaScreenPos = RectTransformUtility.WorldToScreenPoint(null, transform.position);

    // Ajusta el círculo para que esté centrado
    Rect rect = new Rect(dropAreaScreenPos.x - checkRadius, 
                         Screen.height - dropAreaScreenPos.y - checkRadius, 
                         checkRadius * 2, 
                         checkRadius * 2);

    // Color y transparencia del círculo
    Color prevColor = GUI.color;
    GUI.color = new Color(0, 1, 0, 0.2f); // Verde translúcido

    // Dibuja el círculo (realmente es una textura cuadrada, pero sirve para debug)
    GUI.DrawTexture(rect, Texture2D.whiteTexture);

    GUI.color = prevColor;
}*/
}