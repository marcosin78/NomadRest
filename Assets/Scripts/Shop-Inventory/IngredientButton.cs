using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

// Script encargado de gestionar el botón de ingrediente en la UI del minijuego.
// Permite arrastrar el ingrediente, mostrar la cantidad disponible, animar el botón y la imagen,
// y añadir el ingrediente al área de preparación cuando se suelta.
public class IngredientButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int ingredientID;
    public Image ingredientImage;
    public TextMeshProUGUI cantidadText;

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public int originalSiblingIndex;
    [HideInInspector] public Vector3 originalLocalPosition;
    private Canvas canvas;
    private int initialCantidad = -1;
    private Vector3 originalScale;
    private IngredientDropArea currentDropArea;
    private Vector3 targetScale;
    public Image buttonImage;
    private Coroutine returnRoutine;

    public bool IsReturning => returnRoutine != null;

    // Inicializa referencias, carga datos del ingrediente y guarda la cantidad inicial
    void Start()
    {
        if (ingredientImage == null)
            ingredientImage = transform.Find("IngredientImage")?.GetComponent<Image>();
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        LoadDataFromDatabase();
        SaveInitialCantidad();
        originalScale = ingredientImage != null ? ingredientImage.rectTransform.localScale : Vector3.one;
        targetScale = originalScale;
    }

    // Actualiza la cantidad y anima la escala del botón e imagen
    void Update()
    {
        UpdateCantidad();

        if (ingredientImage != null)
        {
            ingredientImage.rectTransform.localScale = Vector3.Lerp(
                ingredientImage.rectTransform.localScale,
                targetScale,
                Time.deltaTime * 12f
            );
        }
        if (buttonImage != null)
        {
            buttonImage.rectTransform.localScale = Vector3.Lerp(
                buttonImage.rectTransform.localScale,
                targetScale,
                Time.deltaTime * 12f
            );
        }
    }

    // Carga los datos del ingrediente desde la base de datos y actualiza la imagen y cantidad
    public void LoadDataFromDatabase()
    {
        var data = ItemDatabase.Instance.GetItemById(ingredientID);
        gameObject.SetActive(true);
        if (data != null)
        {
            if (ingredientImage != null)
            {
                ingredientImage.sprite = data.spriteName != null ? Resources.Load<Sprite>(data.spriteName) : null;
                ingredientImage.enabled = true;
            }
            UpdateCantidad();
        }
        else
        {
            Debug.LogWarning($"No se encontró el item con ID {ingredientID} en la base de datos.");
            if (ingredientImage != null)
            {
                ingredientImage.sprite = null;
                ingredientImage.enabled = false;
            }
            if (cantidadText != null)
                cantidadText.text = "0";
        }
    }

    // Actualiza el texto de cantidad según el inventario
    private void UpdateCantidad()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("InventorySystem.Instance es nulo en IngredientButton.");
            if (cantidadText != null)
                cantidadText.text = "0";
            return;
        }

        int actualCantidad = InventorySystem.Instance.GetItemCount(ingredientID);
        if (cantidadText != null)
            cantidadText.text = actualCantidad.ToString();
    }

    // Guarda la posición, escala y padre originales al empezar a arrastrar
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (originalParent == null)
            originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalLocalPosition = transform.localPosition;
        originalScale = ingredientImage != null ? ingredientImage.rectTransform.localScale : Vector3.one;
        targetScale = originalScale;
        if (buttonImage != null)
            buttonImage.rectTransform.localScale = originalScale;

        // Reproduce el audio del ingrediente si está definido
        var data = ItemDatabase.Instance.GetItemById(ingredientID);
        if (data != null && !string.IsNullOrEmpty(data.audioName))
        {
            var clip = Resources.Load<AudioClip>(data.audioName);
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }

    // Actualiza la posición y escala del botón mientras se arrastra
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out pos
        );
        transform.position = canvas.transform.TransformPoint(pos);

        IngredientDropArea nearestArea = FindNearestDropArea();
        if (nearestArea != null && ingredientImage != null)
        {
            float minScale = 0.5f;
            float maxDistance = 200f;
            float dist = Vector3.Distance(transform.position, nearestArea.transform.position);
            float t = Mathf.Clamp01(1 - (dist / maxDistance));
            float scale = Mathf.Lerp(1f, minScale, t);
            targetScale = Vector3.one * scale;
        }
        else if (ingredientImage != null)
        {
            targetScale = originalScale;
        }
    }

    // Al soltar el botón, intenta añadir el ingrediente al área y anima el retorno
    public void OnEndDrag(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool added = false;

        foreach (var r in results)
        {
            var dropArea = r.gameObject.GetComponent<IngredientDropArea>();
            if (dropArea != null)
            {
                var data = ItemDatabase.Instance.GetItemById(ingredientID);
                Sprite sprite = data != null ? Resources.Load<Sprite>(data.spriteName) : null;
                dropArea.AddIngredient(ingredientID, sprite);
                added = true;
                break;
            }
        }

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);
        returnRoutine = StartCoroutine(SmoothReturn());
    }

    // Corrutina para animar el retorno del botón a su posición y escala original
    private IEnumerator SmoothReturn()
    {
        float duration = 0.25f;
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = originalParent.TransformPoint(originalLocalPosition);

        Vector3 startScale = ingredientImage != null ? ingredientImage.rectTransform.localScale : Vector3.one;
        Vector3 endScale = originalScale;

        Vector3 startBtnScale = buttonImage != null ? buttonImage.rectTransform.localScale : Vector3.one;

        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);

        while (elapsed < duration)
        {
            if (!gameObject.activeInHierarchy)
            {
                Debug.Log("IngredientButton desactivado durante la animación de retorno. Saliendo de la corrutina.");
                yield break;
            }

            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);

            if (ingredientImage != null)
                ingredientImage.rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            if (buttonImage != null)
                buttonImage.rectTransform.localScale = Vector3.Lerp(startBtnScale, endScale, t);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.position = endPos;
        if (ingredientImage != null)
            ingredientImage.rectTransform.localScale = endScale;
        if (buttonImage != null)
            buttonImage.rectTransform.localScale = endScale;

        targetScale = originalScale;
        returnRoutine = null;

        yield return StartCoroutine(BounceEffect());
    }

    // Efecto de rebote al terminar la animación de retorno
    private IEnumerator BounceEffect()
    {
        float bounceDuration = 0.12f;
        float elapsed = 0f;
        float bounceScale = 1.15f;

        Vector3 overshoot = originalScale * bounceScale;

        while (elapsed < bounceDuration / 2f)
        {
            float t = elapsed / (bounceDuration / 2f);
            if (ingredientImage != null)
                ingredientImage.rectTransform.localScale = Vector3.Lerp(originalScale, overshoot, t);
            if (buttonImage != null)
                buttonImage.rectTransform.localScale = Vector3.Lerp(originalScale, overshoot, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < bounceDuration / 2f)
        {
            float t = elapsed / (bounceDuration / 2f);
            if (ingredientImage != null)
                ingredientImage.rectTransform.localScale = Vector3.Lerp(overshoot, originalScale, t);
            if (buttonImage != null)
                buttonImage.rectTransform.localScale = Vector3.Lerp(overshoot, originalScale, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (ingredientImage != null)
            ingredientImage.rectTransform.localScale = originalScale;
        if (buttonImage != null)
            buttonImage.rectTransform.localScale = originalScale;
    }

    // Restaura el botón a su posición y estado original
    public void ResetButton()
    {
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalSiblingIndex);
        gameObject.SetActive(true);
    }

    // Guarda la cantidad inicial del ingrediente al iniciar el minijuego
    public void SaveInitialCantidad()
    {
        if (InventorySystem.Instance != null)
            initialCantidad = InventorySystem.Instance.GetItemCount(ingredientID);
    }

    // Restaura la cantidad inicial del ingrediente si ha cambiado
    public void RestoreInitialCantidad()
    {
        if (InventorySystem.Instance != null && initialCantidad != -1)
        {
            int actual = InventorySystem.Instance.GetItemCount(ingredientID);
            int diff = initialCantidad - actual;
            if (diff != 0)
            {
                InventorySystem.Instance.AddItem(ingredientID, diff);
                UpdateCantidad();
            }
        }
    }

    // Busca el área de ingredientes más cercana al botón
    private IngredientDropArea FindNearestDropArea()
    {
        IngredientDropArea[] areas = GameObject.FindObjectsOfType<IngredientDropArea>();
        IngredientDropArea nearest = null;
        float minDist = float.MaxValue;
        foreach (var area in areas)
        {
            float d = Vector3.Distance(transform.position, area.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = area;
            }
        }
        return nearest;
    }

    // Anima el retorno del botón a su posición original, instantáneo o con animación
    public void AnimateReturnToOriginal(bool instant = false)
    {
        gameObject.SetActive(true);

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);

        transform.localPosition = originalLocalPosition;
        if (ingredientImage != null)
            ingredientImage.rectTransform.localScale = originalScale;
        if (buttonImage != null)
            buttonImage.rectTransform.localScale = originalScale;
        targetScale = originalScale;

        if (!instant)
            returnRoutine = StartCoroutine(SmoothReturn());
    }

    // Detiene la animación de retorno si el botón se desactiva
    void OnDisable()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            Debug.Log("IngredientButton desactivado. Corrutina de retorno detenida.");
            returnRoutine = null;
        }
    }
}
