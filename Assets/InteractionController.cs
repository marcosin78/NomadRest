using UnityEngine;

public class NPCInteractionController : MonoBehaviour, IInteractable
{
    public DialogScript dialogScript;
    public BeerDrinkingScript beerDrinkingScript;
    public GenericDialogScript genericDialogScript;

    public enum InteractionType { Beer, Dialog }
    public InteractionType currentInteraction;

    public void OnInteract()
{
    // Caso 1: Cerveza
    if (currentInteraction == InteractionType.Beer &&
        beerDrinkingScript != null &&
        beerDrinkingScript.askingBeer)
    {
        beerDrinkingScript.GiveBeer();
        Debug.Log("Interacted with NPC for beer: INTERACTION CONTROLLER " + gameObject.name);
        return;
    }

    // Caso 2: Diálogo especial
    if (currentInteraction == InteractionType.Dialog &&
        dialogScript != null &&
        dialogScript.hasSpecialDialog)
    {
        dialogScript.StartDialog();
        Debug.Log("Interacted with NPC for special dialog: INTERACTION CONTROLLER " + gameObject.name);
        return;
    }

    // Caso 3: Diálogo genérico (por defecto)
    if (genericDialogScript != null)
    {
        genericDialogScript.StartGenericDialog();
        Debug.Log("Interacted with NPC for generic dialog: INTERACTION CONTROLLER " + gameObject.name);
    }
}
    public string GetName()
    {
        return gameObject.name;
    }
}
