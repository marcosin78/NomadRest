using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour, IInteractable
{

    private static PlayerController playerController;
    public DialogTree dialogTree;

    private static Canvas dialogCanvas;
    private static TextMeshProUGUI dialogText;

    private int currentNodeIndex = 0;
    private bool isDialogActive = false;

    public Transform interactingEntity;
    public Transform playerTransform;
    public float maxDialogDistance = 5f;
    public float maxDialogAngle = 70f;

    // Cambios aquí: static y búsqueda automática
    public static Transform decisionButtonsParent;
    public static Button[] decisionButtons;

    void Start()
    {
        // Busca el DecisionButtonsParent por tag
        if (decisionButtonsParent == null)
        {
            GameObject buttonsObj = GameObject.FindGameObjectWithTag("DecisionButtonsParent");
            if (buttonsObj != null)
            {
                decisionButtonsParent = buttonsObj.transform;
                Debug.Log("Encontrado DecisionButtonsParent: " + decisionButtonsParent.name);
            }
            else
            {
                Debug.LogWarning("No se encontró ningún objeto con el tag 'DecisionButtonsParent'.");
            }
        }
        if(playerController == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<PlayerController>();
            }
            else
            {
                Debug.LogWarning("No se encontró ningún objeto con el tag 'Player' para asignar PlayerController.");
            }

        }
            

        // Busca los botones hijos solo si el parent fue encontrado
        if (decisionButtons == null && decisionButtonsParent != null)
        {
            decisionButtons = decisionButtonsParent.GetComponentsInChildren<Button>(true);
            Debug.Log("Botones encontrados: " + decisionButtons.Length);
            foreach (var btn in decisionButtons)
                Debug.Log("Botón: " + btn.name);
        }

        if (dialogCanvas == null)
        {
            GameObject canvasObj = GameObject.FindGameObjectWithTag("PlayerDialogCanvas");
            if (canvasObj != null)
            {
                dialogCanvas = canvasObj.GetComponent<Canvas>();
                if (dialogCanvas != null && dialogText == null)
                {
                    Transform textTransform = dialogCanvas.transform.Find("DialogText");
                    if (textTransform != null)
                        dialogText = textTransform.GetComponent<TextMeshProUGUI>();
                    else
                        dialogText = dialogCanvas.GetComponentInChildren<TextMeshProUGUI>();
                }
            }
        }

        if (dialogCanvas != null)
            dialogCanvas.gameObject.SetActive(false);

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }
    }
    void Awake()
    {
      
    }

    public void StartDialog(DialogTree tree, Transform entity = null)
    {
        dialogTree = tree;
        currentNodeIndex = 0;
        isDialogActive = true;
        if (dialogCanvas != null)
            dialogCanvas.gameObject.SetActive(true);
        interactingEntity = entity;
        ShowCurrentNode();
    }

    void ShowCurrentNode()
    {
        HideAllDecisionButtons();

        if (dialogTree != null && dialogTree.nodes != null && currentNodeIndex < dialogTree.nodes.Length)
        {
            var node = dialogTree.nodes[currentNodeIndex];
            if (dialogText != null)
                dialogText.text = node.text;

            // Si hay decisiones, muestra los botones
            if (node.choices != null && node.choices.Length > 0 && decisionButtons != null)
            {
                //Desbloquea el raton para clicar
                if (playerController != null)
                    playerController.UnlockCursorAndLowerSensitivity();
                for (int i = 0; i < node.choices.Length && i < decisionButtons.Length; i++)
                {
                    var btn = decisionButtons[i];
                    btn.gameObject.SetActive(true);
                    var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null)
                        txt.text = node.choices[i].choiceText;

                    int nextIndex = node.choices[i].nextNodeIndex;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnDecisionSelected(nextIndex));
                    playerController.UnlockAll();
                }
            }
            else
            {
                // Bloquea el ratón/cámara si no hay decisiones
                if (playerController != null)
                playerController.LockCursorAndRestoreSensitivity();

            }
        }
        else
        {
            EndDialog();
            playerController.LockCursorAndRestoreSensitivity();
            Debug.Log("Dialog ended: no more nodes.");
        }
    }

    void HideAllDecisionButtons()
    {
        if (decisionButtons != null)
        {
            Debug.Log("Hiding decision buttons.");
            foreach (var btn in decisionButtons)
                btn.gameObject.SetActive(false);
        }
    }

    void OnDecisionSelected(int nextNode)
    {
        currentNodeIndex = nextNode;
        ShowCurrentNode();
    }

    void Update()
    {
        if (!isDialogActive) return;

        if (interactingEntity != null && playerTransform != null)
        {
            float distance = Vector3.Distance(playerTransform.position, interactingEntity.position);
            if (distance > maxDialogDistance)
            {
                EndDialog();
                Debug.Log("Dialog ended: too far from entity.");
                return;
            }

            Vector3 toEntity = (interactingEntity.position - playerTransform.position).normalized;
            Vector3 playerForward = playerTransform.forward;
            float angle = Vector3.Angle(playerForward, toEntity);
            if (angle > maxDialogAngle)
            {
                EndDialog();
                Debug.Log("Dialog ended: not looking at entity.");
                return;
            }
        }

        // Solo avanza con click si NO hay decisiones en el nodo actual
        if (Input.GetMouseButtonDown(0))
        {
            var node = dialogTree.nodes[currentNodeIndex];
            if (node.choices == null || node.choices.Length == 0)
            {
                currentNodeIndex++;
                ShowCurrentNode();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialog();
        }
    }

    public void EndDialog()
    {
        isDialogActive = false;
        if (dialogCanvas != null)
            dialogCanvas.gameObject.SetActive(false);
        if (dialogText != null)
            dialogText.text = "";
        HideAllDecisionButtons();
        interactingEntity = null;
    }

    public void OnInteract()
    {
        StartDialog(dialogTree, this.transform);
        Debug.Log("Dialog started.");
    }

    public string GetName()
    {
        throw new System.NotImplementedException();
    }
}
