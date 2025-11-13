using System.Collections;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI eventDialogue;
    public Image dialogueBackground;

    [Header("Settings")]
    public float padding = 50f; // padding around text

    private bool waitingForInput = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowMessage(string message)
    {
        if (eventDialogue == null)
        {
            Debug.LogError("Event_Dialogue reference missing in DialogManager!");
            return;
        }

        eventDialogue.text = message;
    }

    public IEnumerator ShowMessageAndWait(string message)
    {
        ShowMessage(message);
        dialogueBackground.gameObject.SetActive(true);
        ResizeBackground();
        waitingForInput = true;

        // Wait until player presses Space or Enter to continue
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        waitingForInput = false;
        ClearMessage();
    }

    public IEnumerator ShowBinaryChoiceAndWait(
     string message, string choiceA, string choiceB, System.Action<bool> onChoiceMade)
    {
        ShowMessage(message +
            $"\nPress [Space] to {choiceA}\nPress [Z] to {choiceB}");

        dialogueBackground.gameObject.SetActive(true);
        ResizeBackground();
        waitingForInput = true;

        bool choiceMade = false;
        bool choiceASelected = false;

        while (!choiceMade)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                choiceASelected = true;
                choiceMade = true;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                choiceASelected = false;
                choiceMade = true;
            }
            yield return null;
        }

        waitingForInput = false;
        ClearMessage();
        onChoiceMade?.Invoke(choiceASelected);
    }

    public void ClearMessage()
    {
        if (eventDialogue != null)
            eventDialogue.text = "";
        dialogueBackground.gameObject.SetActive(false);
    }
    private void ResizeBackground()
    {
        if (eventDialogue == null || dialogueBackground == null) return;

        // Force TextMeshPro to update its internal layout
        eventDialogue.ForceMeshUpdate();

        // Get the rendered text size
        Vector2 textSize = eventDialogue.GetRenderedValues(false);

        // Set the background size slightly larger than text
        dialogueBackground.rectTransform.sizeDelta = textSize + new Vector2(padding * 2, padding * 1);
        //dialogueBackground.rectTransform.sizeDelta = new Vector2(padding * 2, padding * 2);
    }

}
