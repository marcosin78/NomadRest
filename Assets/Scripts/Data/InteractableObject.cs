using UnityEngine;

public interface IInteractable
{
    // Called when player interacts (e.g. presses E)
    void OnInteract();

    // Optional display name for UI
    string GetName();
}

