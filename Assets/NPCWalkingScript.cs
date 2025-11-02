using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class NPCWalkingScript : MonoBehaviour
{
    public float speed = 3f;
    public float stopDistance = 0.3f;
    public float repathInterval = 1f;

    // Avoidance
    public LayerMask obstacleMask = ~0;
    public float avoidDistance = 1f;
    public float avoidRadius = 0.3f;
    public float avoidWeight = 2f;
    public float avoidHeight = 0.5f; // spherecast height from ground

    public IWaypointProvider provider;
    Pathfinding pathfinder;
    Rigidbody rb;

    List<Vector3> path = new List<Vector3>();
    int index = 0;
    float repathTimer = 0f;

    void Start()
    {
         // NO reasignes provider aquí, solo usa el que ya tiene
         pathfinder = FindObjectOfType<Pathfinding>();
         rb = GetComponent<Rigidbody>();
        RequestPath();
        
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
        // maintain NPC current height
        target.y = transform.position.y;

        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f;
        if (toTarget.magnitude <= stopDistance)
        {
            if (index < path.Count - 1) index++;
            return;
        }

        Vector3 dir = toTarget.normalized;

        // local avoidance: spherecast forward along dir, if hit steer around
        Vector3 castOrigin = transform.position + Vector3.up * avoidHeight;
        if (Physics.SphereCast(castOrigin, avoidRadius, dir, out RaycastHit hit, avoidDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            // compute avoidance direction: reflect + side steer so we don't get stuck
            Vector3 reflect = Vector3.Reflect(dir, hit.normal).normalized;
            Vector3 side = Vector3.Cross(hit.normal, Vector3.up).normalized;
            // pick side direction that points away from obstacle toward target
            if (Vector3.Dot(side, dir) < 0) side = -side;
            Vector3 avoidDir = (reflect + side * 0.5f).normalized;

            // blend toward avoidance direction
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

    void RequestPath()
    {
        if (pathfinder == null || provider == null || provider.Waypoint == null) return;

        Vector2Int startGrid = pathfinder.WorldToGrid(transform.position);
        Vector2Int endGrid = pathfinder.WorldToGrid(provider.Waypoint.position);

        if (!pathfinder.IsInsideGrid(startGrid) || !pathfinder.IsInsideGrid(endGrid))
        {
            path = new List<Vector3> { provider.Waypoint.position };
            index = 0;
            return;
        }

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
    void OnTriggerEnter(Collider other)
    {
        var provider = other.transform.GetComponent<IWaypointProvider>();

      
        if (provider != null && provider == this.provider)
        {
            // Si el waypoint es el destino actual, lo marcamos como no disponible
            if (provider is WaypointScript ws)
            {
                ws.SetAvailability(false);

                Debug.Log("NPC ha llegado a su waypoint: " + other.gameObject.name);


            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (path == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < path.Count; i++)
        {
            Gizmos.DrawSphere(path[i], 0.1f);
            if (i + 1 < path.Count) Gizmos.DrawLine(path[i], path[i + 1]);
        }

        // draw avoidance debug ray
        Gizmos.color = Color.red;
        Vector3 castOrigin = transform.position + Vector3.up * avoidHeight;
        Gizmos.DrawWireSphere(castOrigin, avoidRadius);
        Gizmos.DrawLine(castOrigin, castOrigin + transform.forward * avoidDistance);
    }

   
}