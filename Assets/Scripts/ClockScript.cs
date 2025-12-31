using UnityEngine;
using TMPro;
using System;

public class ClockScript : MonoBehaviour
{
    public static ClockScript Instance { get; private set; }

    [Header("Configuración de la hora")]
    [Range(0, 23)] public int startHour = 8;
    [Range(0, 59)] public int startMinute = 0;
    [Tooltip("Cuántos segundos reales tarda en pasar un minuto del juego")]
    public float realSecondsPerGameMinute = 1f;

    public bool OpenBarTime { get; private set; }
    public bool ClosedBarTime => !OpenBarTime;

    [Header("UI")]
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI dayText;

    public DirtynessScript dirtynessScript;
    public int dirtToSpawn = 3;
    public int stainsToSpawn = 2;

    public Transform playerStartPoint; // Asigna en el Inspector
    public GameObject player; // Asigna el objeto jugador en el Inspector

    private int hour;
    private int minute;
    private float timer;
    private int day = 0;

    public bool FallingAsleep { get; private set; } = false;

    public int Day => day;

    private bool hasSleptThisNight = false;
    public int Hour => hour;
    public int Minute => minute;

    private bool isClockFrozen = false;
    private bool TutorialVariable = false;
    private bool hasSleptAfterTutorial = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        hour = startHour;
        minute = startMinute;
        UpdateClockUI();

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj;

        var playerStartObj = GameObject.FindGameObjectWithTag("PlayerStartPoint");
        if (playerStartObj != null)
            playerStartPoint = playerStartObj.transform;

        var clockObj = GameObject.FindGameObjectWithTag("ClockText");
        if (clockObj != null)
            clockText = clockObj.GetComponent<TextMeshProUGUI>();

        var dayObj = GameObject.FindGameObjectWithTag("DayText");
        if (dayObj != null)
            dayText = dayObj.GetComponent<TextMeshProUGUI>();

        if(clockText == null)
            Debug.LogWarning("ClockText no está asignado en ClockScript.");
        if(dayText == null)
            Debug.LogWarning("DayText no está asignado en ClockScript.");
    }

    void Update()
    {
        CheckIfTutorial();

        // Permite dormir si TutorialVariable es true, sin importar la hora
        if (TutorialVariable)
        {
            FallingAsleep = true;
        }
        else
        {
            if (hour >= 17 && hour <= 22)
                FallingAsleep = true;
            else
                FallingAsleep = false;
        }

        // El reloj solo se congela si estamos en el Día 1
        if (day == 1 )
        {
            isClockFrozen = true;
        }
        else
        {
            isClockFrozen = false;
        }

        // Solo avanza el reloj si no está congelado
        if (!isClockFrozen)
        {
            timer += Time.deltaTime;
            if (timer >= realSecondsPerGameMinute)
            {
                timer = 0f;
                AddMinute();
            }
        }

        // Actualiza el estado de OpenBarTime según la hora
        if ((hour > 9 && hour < 17) ||
            (hour == 9 && minute >= 0) ||
            (hour == 17 && minute == 0))
        {
            OpenBarTime = true;
        }
        else
        {
            OpenBarTime = false;
        }

        // El sueño automático a las 22:15 solo si el reloj no está congelado
        if (hour == 22 && minute == 15 && !hasSleptThisNight && !isClockFrozen)
        {
            FallingAsleep = true;
            Debug.Log("Es 22:15, el personaje se queda dormido automáticamente.");
            NextDay();
            hasSleptThisNight = true;
        }

        // Resetea el flag al empezar un nuevo día
        if (hour == startHour && minute == startMinute && hasSleptThisNight)
        {
            hasSleptThisNight = false;
        }
    }

    void AddMinute()
    {
        minute++;
        if (minute >= 60)
        {
            minute = 0;
            hour++;
            if (hour >= 24)
                hour = 0;
        }
        UpdateClockUI();
    }

    public void NextDay()
    {
        day++;
        hour = startHour;
        minute = startMinute;
        UpdateClockUI();
        Debug.Log("Nuevo día: " + day);

        // Mueve al jugador al punto de inicio
        if (player != null && playerStartPoint != null)
        {
            player.transform.position = playerStartPoint.position;
            player.transform.rotation = playerStartPoint.rotation;
        }

        // Spawnea suciedad y manchas
        if (dirtynessScript != null)
        {
            for (int i = 0; i < dirtToSpawn; i++)
                dirtynessScript.SpawnRandomDirt();

            for (int i = 0; i < stainsToSpawn; i++)
                dirtynessScript.SpawnRandomStain();
        }
    }

    void UpdateClockUI()
    {
        if (clockText != null)
            clockText.text = $"{hour:00}:{minute:00}";

        if (dayText != null)
            dayText.text = $"Día {day}";
    }

    // Llama a este método cuando el jugador duerme manualmente
    public void PlayerSleep()
    {
        // Si duerme después de terminar el tutorial, marca que ya no es tutorial nunca más
        if (TutorialVariable && GameConditions.Instance.HasCondition("PlayerHasFinishedTutorial"))
        {
            hasSleptAfterTutorial = true;
            TutorialVariable = false;
        }
        NextDay();
        Debug.Log("El jugador ha dormido (PlayerSleep).");
    }

    private void CheckIfTutorial()
    {
        if (hasSleptAfterTutorial)
        {
            TutorialVariable = false;
            return;
        }

        bool notTalked = GameConditions.Instance.HasCondition("PlayerNotTalkedToTutorialBird");
        bool talked = GameConditions.Instance.HasCondition("PlayerHasTalkedToTutorialBird");
        bool finished = GameConditions.Instance.HasCondition("PlayerHasFinishedTutorial");

        // Lista de condiciones intermedias del tutorial
        bool anyOther =
            GameConditions.Instance.HasCondition("PlayerHasSleeptWithTutorialBird") ||
            GameConditions.Instance.HasCondition("PlayerGoingToSaloonWithTutorialBird") ||
            GameConditions.Instance.HasCondition("PlayerPendingOfCleaningSaloonWithTutorialBird") ||
            GameConditions.Instance.HasCondition("PlayerHasCleanedSaloonWithTutorialBird") ||
            GameConditions.Instance.HasCondition("PlayerShowingTheCocktailsWithTutorialBird") ||
            GameConditions.Instance.HasCondition("BirdShowingTheShop") ||
            GameConditions.Instance.HasCondition("PendingOfCheckShop") ||
            GameConditions.Instance.HasCondition("PlayerHasCheckedShop") ||
            GameConditions.Instance.HasCondition("PlayerPendingOfCheckCocktailsWithTutorialBird") ||
            GameConditions.Instance.HasCondition("PlayerHasCheckedCocktailsWithTutorialBird");

        // Solo permite dormir si SOLO las dos primeras están activas y ninguna otra
        if ((notTalked || talked) && !anyOther && !finished)
        {
            TutorialVariable = true;
        }
        // Cuando termina el tutorial, permite dormir una vez más
        else if (finished && !hasSleptAfterTutorial)
        {
            TutorialVariable = true;
        }
        else
        {
            TutorialVariable = false;
        }
    }

    public void ForceUnfreezeClock()
    {
        isClockFrozen = false;
        Debug.Log("Reloj descongelado temporalmente.");
    }

    public void RecalculateFreezeState()
    {
        // Llama a la lógica que tienes en Update para congelar/descongelar el reloj
        if ((day == 0 || day == 1) && !hasSleptAfterTutorial)
        {
            isClockFrozen = true;
            Debug.Log("Reloj congelado por RecalculateFreezeState().");

        }
            
        else
            isClockFrozen = false;
    }
}
