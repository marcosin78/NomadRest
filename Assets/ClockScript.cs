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

    public Transform playerStartPoint; // Asigna en el Inspector
    public GameObject player; // Asigna el objeto jugador en el Inspector

    private int hour;
    private int minute;
    private float timer;
    private int day = 1;

    public bool FallingAsleep { get; private set; } = false;

    public int Day => day;

    private bool hasSleptThisNight = false;
    public int Hour => hour;
    public int Minute => minute;

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
        timer += Time.deltaTime;
        if (timer >= realSecondsPerGameMinute)
        {
            timer = 0f;
            AddMinute();
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

         // Permite dormir solo entre las 17:00 y las 22:00
    if (hour >= 17 && hour <= 22)
    {
        FallingAsleep = true; // Puede dormir manualmente
    }
    else
    {
        FallingAsleep = false; // No puede dormir manualmente fuera de ese rango
    }

    // A las 22:15 se duerme automáticamente y pasa de día (solo una vez)
    if (hour == 22 && minute == 15 && !hasSleptThisNight)
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
    }

    void UpdateClockUI()
    {
        if (clockText != null)
            clockText.text = $"{hour:00}:{minute:00}";

            if (dayText != null)
            dayText.text = $"Día {day}";
    }

    // Puedes añadir métodos públicos para avanzar la hora manualmente o consultar la hora global
}
