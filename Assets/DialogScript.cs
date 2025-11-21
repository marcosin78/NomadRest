using UnityEngine;
using TMPro;

public class DialogScript : MonoBehaviour
{
    public DialogTree dialogTree;
    public Canvas dialogCanvas;
    public Canvas decisionCanvas;
    public TextMeshProUGUI dialogUIText;
    public GameObject decisionButtonPrefab;

    public bool hasSpecialDialog = false;
    public Transform decisionButtonParent;

    private bool pendingChoices = false;
    private DialogChoice[] pendingChoicesArray;

    private int currentNodeIndex = 0;
    public bool isDialogActive = false;
    private bool hasRotatedToPlayer = false;

    public Rigidbody characterRigidbody;
    public Transform playerTransform;
    void Start()
    {
        if(playerTransform == null)
        {
            playerTransform = GameObject.FindWithTag("Player")?.transform;
        }
        if(characterRigidbody == null)
            characterRigidbody = GetComponent<Rigidbody>();
        if (dialogCanvas != null)
            dialogCanvas.gameObject.SetActive(false);
        if (decisionCanvas != null)
            decisionCanvas.gameObject.SetActive(false);
    }

    public void StartDialog()
    {
        currentNodeIndex = 0;
        isDialogActive = true;
         // Rota el Rigidbody hacia el jugador antes de pausar el tiempo
      if (playerTransform != null && characterRigidbody != null)
      {
        Vector3 lookDir = playerTransform.position - transform.position;
        lookDir.y = 0;
     if (lookDir.sqrMagnitude > 0.001f)
     {
        Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized);
        // Aplica la rotación directamente para que sea inmediata
        characterRigidbody.transform.rotation = targetRotation;
     }
    }

        ShowNode(currentNodeIndex);
        dialogCanvas.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    void ShowNode(int nodeIndex)
    {
        if (dialogTree == null || nodeIndex < 0 || nodeIndex >= dialogTree.nodes.Length)
    {
        EndDialog();
        return;
    }

    var node = dialogTree.nodes[nodeIndex];
    dialogUIText.text = node.text;

    if (node.choices != null && node.choices.Length > 0)
    {
        // Muestra el texto y espera un clic antes de mostrar las decisiones
        dialogCanvas.gameObject.SetActive(true);
        decisionCanvas.gameObject.SetActive(false);
        pendingChoices = true;
        pendingChoicesArray = node.choices;
    }
    else
    {
        dialogCanvas.gameObject.SetActive(true);
        decisionCanvas.gameObject.SetActive(false);
        pendingChoices = false;
        pendingChoicesArray = null;
    }
    }

    void ShowChoices(DialogChoice[] choices)
    {
        // Limpia botones anteriores
        foreach (Transform child in decisionButtonParent)
            Destroy(child.gameObject);

        for (int i = 0; i < choices.Length; i++)
        {
            int idx = i;
            var btnObj = Instantiate(decisionButtonPrefab, decisionButtonParent);
            var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = choices[i].choiceText;
            btnObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
            Debug.Log("Button clicked for choice index: " + idx);
            OnChoiceSelected(idx);
             });
            Debug.Log("Created choice button: " + choices[i].choiceText);
        }
        decisionCanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnChoiceSelected(int choiceIndex)
    {
        var node = dialogTree.nodes[currentNodeIndex];
        int nextNode = node.choices[choiceIndex].nextNodeIndex;

        decisionCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (nextNode == -1)
        {
            EndDialog();
        }
        else
        {
            currentNodeIndex = nextNode;
            ShowNode(currentNodeIndex);
        }
    }

    public void EndDialog()
    {
        isDialogActive = false;
        Time.timeScale = 1f;
        dialogCanvas.gameObject.SetActive(false);
        decisionCanvas.gameObject.SetActive(false);
        dialogUIText.text = "";
    }

    void Update()
    {
       if (!isDialogActive) return;

        
     if (dialogCanvas.gameObject.activeSelf && Input.GetMouseButtonDown(0))
     {
        if (pendingChoices && pendingChoicesArray != null)
        {
            // Ahora muestra las decisiones
            dialogCanvas.gameObject.SetActive(false);
            ShowChoices(pendingChoicesArray);
            pendingChoices = false;
            pendingChoicesArray = null;
        }
        else
        {
            var node = dialogTree.nodes[currentNodeIndex];
            if (node.choices == null || node.choices.Length == 0)
            {
                if (node.nextNodeIndex != -1)
                {
                    currentNodeIndex = node.nextNodeIndex;
                    ShowNode(currentNodeIndex);
                    Debug.Log("Avanzando al siguiente nodo del diálogo: " + currentNodeIndex);
                }
                else
                {
                    EndDialog();
                    Debug.Log("Diálogo terminado.");
                }
            }
        }
     }
     if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q)) && isDialogActive)
     {
        EndDialog();
     }
    }
}