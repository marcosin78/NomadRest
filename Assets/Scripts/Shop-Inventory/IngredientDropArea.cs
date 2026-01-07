using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// Script para el área donde se sueltan los ingredientes y se agita el cóctel.
// Permite añadir ingredientes, detectar el shake/agitado, cambiar sprites, reproducir sonidos y comunicar el resultado al minijuego.
public class IngredientDropArea : MonoBehaviour
{
    // Variables de movimiento y shake
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 lastPosition;
    private Vector3 initialPosition;
    private Rigidbody2D rb;
    public float mass = 1.5f; // Peso del cóctel
    public float shakeMultiplier = 0.00005f; // Sensibilidad del shake
    public BeerMinigameScript beerMinigameScript; // Asigna en el inspector

    // Sprites de la coctelera
    public Sprite closedShakerSprite;
    public Sprite openShakerSprite;
    public GameObject shakerSprite;

    // Sonido al añadir ingrediente
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
            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOverUIElement())
                {
                    isDragging = true;
                    StartShaking();
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
            Vector2 force = (targetPos - transform.position) * rb.mass * 50f;
            rb.AddForce(force);

            float distance = Vector3.Distance(transform.position, lastPosition);
            if (isBeingShaken)
            {
                float shakeAmount = distance * shakeMultiplier;
                shakeProgress += shakeAmount;
                Debug.Log($"Shake Progress: {shakeProgress:F3}");
                if (shakeProgress >= shakeRequired)
                {
                    shakeProgress = shakeRequired;
                    isBeingShaken = false;
                    isDragging = false;

                    // Restar del inventario cada ingrediente añadido
                    if (InventorySystem.Instance != null)
                    {
                        foreach (int id in addedIngredientIDs)
                        {
                            InventorySystem.Instance.RemoveItem(id, 1);
                        }
                    }

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

        // Cambia el sprite según si hay un IngredientButton cerca
        bool ingredientNear = false;
        float checkRadius = 100f;
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

    // Añade un ingrediente al área si hay suficiente cantidad y no hay otro del mismo tipo
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

        if (playerAudioSource != null && shakerFillSoundClip != null)
            playerAudioSource.PlayOneShot(shakerFillSoundClip);

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
                    btn.AnimateReturnToOriginal(true);
                }
                else
                {
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

    [Range(0,1)]
    public float shakeProgress = 0f;
    public float shakeRequired = 1f;
    public bool isBeingShaken = false;

    // Devuelve true si se puede agarrar el área (mínimo 2 ingredientes)
    public bool CanGrabArea()
    {
        return addedIngredientIDs.Count >= 2;
    }

    // Inicia el movimiento de shake
    public void StartShaking()
    {
        if (CanGrabArea())
        {
            isBeingShaken = true;
            shakeProgress = 0f;
            lastPosition = transform.position;
        }
    }

    // Actualiza el shake manualmente (no usado en este flujo)
    public void UpdateShaking(float shakeAmount)
    {
        if (isBeingShaken)
        {
            shakeProgress += shakeAmount;

            if (shakeProgress >= shakeRequired)
            {
                shakeProgress = shakeRequired;
                isBeingShaken = false;
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
}