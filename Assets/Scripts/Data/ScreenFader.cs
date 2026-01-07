using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Script encargado de gestionar el fundido de pantalla (fade in/fade out) usando una imagen UI.
// Permite realizar transiciones suaves entre escenas o estados visuales del juego.
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    public Image fadeImage;
    public float fadeDuration = 1f;

    // Inicializa la instancia singleton y asigna la imagen de fade si no está asignada.
    void Awake()
    {
        Instance = this;
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();
    }

    // Inicia el fundido de pantalla hacia negro.
    public IEnumerator FadeOut()
    {
        yield return Fade(0f, 1f);
    }

    // Inicia el fundido de pantalla desde negro a transparente.
    public IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f);
    }

    // Rutina interna para interpolar el valor alfa de la imagen de fade.
    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(c.r, c.g, c.b, to);
    }
}