using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

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
    private Vector3 targetScale; // Añade esto arriba
    public Image buttonImage; // Añade esta línea arriba para referenciar la imagen del botón
    private Coroutine returnRoutine;

    public bool IsReturning => returnRoutine != null;

    void Start()
    {
        if (ingredientImage == null)
            ingredientImage = transform.Find("IngredientImage")?.GetComponent<Image>();
        if (buttonImage == null)
            buttonImage = GetComponent<Image>(); // Obtiene la imagen del propio botón
        canvas = GetComponentInParent<Canvas>();
        LoadDataFromDatabase();
        SaveInitialCantidad();
        originalScale = ingredientImage != null ? ingredientImage.rectTransform.localScale : Vector3.one;
        targetScale = originalScale; // Inicializa targetScale


        
    }
    void Update()
    {
        UpdateCantidad();

        // Animación suave de escala para ambas imágenes
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (originalParent == null)
            originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalLocalPosition = transform.localPosition;
        originalScale = ingredientImage != null ? ingredientImage.rectTransform.localScale : Vector3.one;
        targetScale = originalScale;
        // También resetea la escala de la imagen del botón
        if (buttonImage != null)
            buttonImage.rectTransform.localScale = originalScale;

        // --- Reproducir audio solo una vez por drag ---
        var data = ItemDatabase.Instance.GetItemById(ingredientID);
        if (data != null && !string.IsNullOrEmpty(data.audioName))
        {
            var clip = Resources.Load<AudioClip>(data.audioName);
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }

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

        // Escalado dinámico al acercarse a un IngredientDropArea
        IngredientDropArea nearestArea = FindNearestDropArea();
        if (nearestArea != null && ingredientImage != null)
        {
            float minScale = 0.5f; // Escala mínima al llegar al centro
            float maxDistance = 200f; // Distancia máxima para empezar a escalar (ajusta según tu UI)
            float dist = Vector3.Distance(transform.position, nearestArea.transform.position);
            float t = Mathf.Clamp01(1 - (dist / maxDistance));
            float scale = Mathf.Lerp(1f, minScale, t);
            targetScale = Vector3.one * scale; // Solo cambiamos la escala objetivo
        }
        else if (ingredientImage != null)
        {
            targetScale = originalScale;
        }
    }

    // Al soltar, restaura la escala
    public void OnEndDrag(PointerEventData eventData)
    {
        // Detecta el objeto bajo el puntero al soltar
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool added = false;

        foreach (var r in results)
        {
            var dropArea = r.gameObject.GetComponent<IngredientDropArea>();
            if (dropArea != null)
            {
                // Intenta añadir el ingrediente
                var data = ItemDatabase.Instance.GetItemById(ingredientID);
                Sprite sprite = data != null ? Resources.Load<Sprite>(data.spriteName) : null;
                dropArea.AddIngredient(ingredientID, sprite);
                added = true;
                break;
            }
        }

        // Inicia animación de retorno suave
        if (returnRoutine != null)
            StopCoroutine(returnRoutine);
        returnRoutine = StartCoroutine(SmoothReturn());
    }

    // Corrutina para volver suavemente a la posición y escala original
    private IEnumerator SmoothReturn()
    {
        float duration = 0.25f; // Duración de la animación
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
            // --- NUEVO: Si el objeto se desactiva, sal de la corrutina ---
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

        // --- Animación de rebote/expansión ---
        yield return StartCoroutine(BounceEffect());
    }

    private IEnumerator BounceEffect()
    {
        float bounceDuration = 0.12f;
        float elapsed = 0f;
        float bounceScale = 1.15f;

        Vector3 overshoot = originalScale * bounceScale;

        // Expande
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

        // Contrae
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

        // Asegura escala final
        if (ingredientImage != null)
            ingredientImage.rectTransform.localScale = originalScale;
        if (buttonImage != null)
            buttonImage.rectTransform.localScale = originalScale;
    }

    public void ResetButton()
    {
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalSiblingIndex);
        gameObject.SetActive(true);
    }

    public void SaveInitialCantidad()
    {
        if (InventorySystem.Instance != null)
            initialCantidad = InventorySystem.Instance.GetItemCount(ingredientID);
    }

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

    // Encuentra el IngredientDropArea más cercano al botón
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
        // Puedes poner un umbral si quieres limitar el efecto solo cuando está cerca
        return nearest;
    }
    public void AnimateReturnToOriginal(bool instant = false)
    {
        gameObject.SetActive(true); // Asegura que está activo antes de animar

        // Detén cualquier animación pendiente
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        // Cambia el padre antes de ajustar la posición local
        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);

        // Coloca la posición y escala originales
        transform.localPosition = originalLocalPosition;
        if (ingredientImage != null)
            ingredientImage.rectTransform.localScale = originalScale;
        if (buttonImage != null)
            buttonImage.rectTransform.localScale = originalScale;
        targetScale = originalScale;

        // No inicies animación si es instantáneo
        if (!instant)
            returnRoutine = StartCoroutine(SmoothReturn());
    }

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
