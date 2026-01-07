using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

// Script encargado de gestionar el movimiento de los NPCs por el escenario.
// Utiliza pathfinding en grilla, evita obstáculos con spherecast y permite que los NPCs salgan por el LeavePoint.
// El NPC sigue una lista de waypoints y puede cambiar su destino dinámicamente.
[RequireComponent(typeof(Collider))]
public class NPCWalkingScript : MonoBehaviour
{
    public float speed = 3f;
    public float stopDistance = 0.3f;
    public float repathInterval = 1f;

    // Parámetros de evasión de obstáculos
    public LayerMask obstacleMask = ~0;
    public float avoidDistance = 1f;
    public float avoidRadius = 0.3f;
    public float avoidWeight = 2f;
    public float avoidHeight = 0.5f; // Altura del spherecast desde el suelo

    public IWaypointProvider provider;

    private Transform leavePoint;
    public bool isLeaving = false;
    Pathfinding pathfinder;
    Rigidbody rb;

    List<Vector3> path = new List<Vector3>();
    int index = 0;
    float repathTimer = 0f;

    void Start()
    {
        pathfinder = FindObjectOfType<Pathfinding>();
        rb = GetComponent<Rigidbody>();
        RequestPath();
    }

    // Llama al LeavePoint y marca el NPC como saliendo
    public void GoToLeavePoint()
    {
        GameObject leaveObj = GameObject.FindWithTag("LeavePoint");
        if (leaveObj != null)
        {
            leavePoint = leaveObj.transform;
            provider = new StaticWaypointProvider(leavePoint); // StaticWaypointProvider implementa IWaypointProvider
            isLeaving = true;
        }
        else
        {
            Debug.LogWarning("No LeavePoint found in the scene!");
        }
    }

    void Update()
    {
        if (provider == null || !provider.IsAvailable) return;

        repathTimer += Time.deltaTime;
        if (repathTimer >= repathInterval)
        {
            repathTimer = 0f;
            RequestPath();
        }

        if (path == null || path.Count == 0) return;

        Vector3 target = path[index];
        // Mantiene la altura actual del NPC
        target.y = transform.position.y;

        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f;
        if (toTarget.magnitude <= stopDistance)
        {
            if (index < path.Count - 1) index++;
            return;
        }

        Vector3 dir = toTarget.normalized;

        // Evasión local: spherecast hacia adelante, si hay obstáculo, esquiva
        Vector3 castOrigin = transform.position + Vector3.up * avoidHeight;
        if (Physics.SphereCast(castOrigin, avoidRadius, dir, out RaycastHit hit, avoidDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            // Calcula dirección de evasión: reflect + giro lateral para no quedarse atascado
            Vector3 reflect = Vector3.Reflect(dir, hit.normal).normalized;
            Vector3 side = Vector3.Cross(hit.normal, Vector3.up).normalized;
            // Elige el lado que apunta lejos del obstáculo hacia el objetivo
            if (Vector3.Dot(side, dir) < 0) side = -side;
            Vector3 avoidDir = (reflect + side * 0.5f).normalized;

            // Mezcla hacia la dirección de evasión
            dir = Vector3.Lerp(dir, avoidDir, Mathf.Clamp01(avoidWeight * Time.deltaTime)).normalized;
        }

        Vector3 move = dir * speed * Time.deltaTime;
        if (rb != null) rb.MovePosition(rb.position + move);
        else transform.position += move;

        if (move.sqrMagnitude > 0.0001f)
        {
            Quaternion rot = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
        }
    }

    // Solicita un nuevo path al pathfinder
    void RequestPath()
    {
        if (pathfinder == null || provider == null || provider.Waypoint == null) return;

        // Convierte posiciones world a grid
        Vector2Int startGrid = pathfinder.WorldToGrid(transform.position);
        Vector2Int endGrid = pathfinder.WorldToGrid(provider.Waypoint.position);

        // Si start o end están fuera de la grilla, asigna path directo al waypoint
        if (!pathfinder.IsInsideGrid(startGrid) || !pathfinder.IsInsideGrid(endGrid))
        {
            path = new List<Vector3> { provider.Waypoint.position };
            index = 0;
            return;
        }
        // Solicita el path al sistema de pathfinding
        var nodes = pathfinder.FindPath(startGrid, endGrid);
        if (nodes != null && nodes.Count > 0)
        {
            path.Clear();
            float y = transform.position.y;
            foreach (var n in nodes)
            {
                path.Add(new Vector3(n.worldPosition.x, y, n.worldPosition.z));
            }
            index = 0;
        }
        else
        {
            path = new List<Vector3> { provider.Waypoint.position };
            index = 0;
        }
    }

    // Detecta triggers con waypoints y LeavePoint
    void OnTriggerEnter(Collider other)
    {
        var provider = other.transform.GetComponent<IWaypointProvider>();

        Debug.Log($"Trigger with: {other.gameObject.name}, provider: {provider}, my provider: {this.provider}");
      
        if (provider != null && provider == this.provider)
        {
            // Si el waypoint es el destino actual, lo marcamos como no disponible
            if (provider is WaypointScript ws)
            {
                ws.SetAvailability(false);
                Debug.Log("NPC ha llegado a su waypoint: " + other.gameObject.name);
            }
        }
        // Si es el LeavePoint y estamos saliendo, destruye el NPC
        if (other.CompareTag("LeavePoint") && isLeaving)
        {
            Debug.Log("NPC destroyed on LeavePoint trigger: " + gameObject.name);
            Destroy(gameObject);
            return;
        }
    }

    // Implementación de proveedor de waypoint estático
    public class StaticWaypointProvider : IWaypointProvider
    {
        public Transform Waypoint { get; private set; }
        public bool IsAvailable => true;
        public StaticWaypointProvider(Transform t) { Waypoint = t; }
    }

    // Dibuja el path y la esfera de evasión en el editor
    void OnDrawGizmosSelected()
    {
        if (path == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < path.Count; i++)
        {
            Gizmos.DrawSphere(path[i], 0.1f);
            if (i + 1 < path.Count) Gizmos.DrawLine(path[i], path[i + 1]);
        }

        // Dibuja el ray de evasión
        Gizmos.color = Color.red;
        Vector3 castOrigin = transform.position + Vector3.up * avoidHeight;
        Gizmos.DrawWireSphere(castOrigin, avoidRadius);
        Gizmos.DrawLine(castOrigin, castOrigin + transform.forward * avoidDistance);
    }
}