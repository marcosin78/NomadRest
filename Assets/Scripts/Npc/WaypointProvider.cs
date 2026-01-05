using UnityEngine;  

public interface IWaypointProvider
{
    bool IsAvailable { get; }
    Transform Waypoint { get; }
}