using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI eventDialogue;
    public Image dialogueBackground;
    [SerializeField] private TextMeshProUGUI messageDialogue;
    public Image messageBackground;
    //[SerializeField] private TextMeshProUGUI mainMenu;
    //public Image mainMenuBackground;
    [SerializeField] private TextMeshProUGUI mainSettings;
    public Image mainSettingsBackground;

    [Header("Settings")]
    public float padding = 10f; // padding around text

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

    public void ShowTop(string message)
    {
        if (messageDialogue == null)
        {
            Debug.LogError("Message_Dialogue reference missing in DialogManager!");
            return;
        }

        GameSettings settings = GameSettings.Instance;
        //PlayerData currentPlayer = GameSettings.Instance;
        messageDialogue.text = message;

        messageBackground.gameObject.SetActive(true);
        ResizeTopBackground();

    }

    public IEnumerator ShowMessageAndWait(string message)
    {
        ShowMessage(message);
        dialogueBackground.gameObject.SetActive(true);
        ResizeBackground();
        waitingForInput = true;

        // Wait until player presses Space or Enter to continue
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        SoundManager.Instance.Play("choice_move");
        waitingForInput = false;
        ClearMessage();
    }

    public IEnumerator ShowBinaryChoiceAndWait(
     string message, string choiceA, string choiceB, System.Action<bool> onChoiceMade)
    {
        ShowMessage(message +
            $"\n[Space]: {choiceA}\n[Z]: {choiceB}");

        dialogueBackground.gameObject.SetActive(true);
        ResizeBackground();
        waitingForInput = true;

        bool choiceMade = false;
        bool choiceASelected = false;

        while (!choiceMade)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SoundManager.Instance.Play("choice_confirm");
                choiceASelected = true;
                choiceMade = true;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                SoundManager.Instance.Play("choice_back");
                choiceASelected = false;
                choiceMade = true;
            }
            yield return null;
        }

        waitingForInput = false;
        ClearMessage();
        onChoiceMade?.Invoke(choiceASelected);
    }

    public IEnumerator ShowAmountChoiceAndWait(
    string message, string choiceA, string choiceB,
    int min_amount, int max_amount, System.Action<int> onChoiceMade)
    {
        int value = min_amount;

        ShowMessage(
            $"{message}\n" +
            $"[A]: {choiceA}\n" +
            $"[D]: {choiceB}\n" +
            $"[Space]: Confirm.\n" +
            $"Value: {value}"
        );

        dialogueBackground.gameObject.SetActive(true);
        ResizeBackground();
        waitingForInput = true;

        bool choiceMade = false;

        while (!choiceMade)
        {
            // decrement
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SoundManager.Instance.Play("choice_move");
                if (value > min_amount)
                {
                    value--;
                    Debug.Log($"Decrement to {value}");
                    ClearMessage();
                    yield return new WaitForSeconds(0.02f);
                    dialogueBackground.gameObject.SetActive(true);
                    ShowMessage(
                        $"{message}\n" +
                        $"Press [A] to {choiceA}\n" +
                        $"Press [D] to {choiceB}\n" +
                        $"Press [Space] to confirm.\n" +
                        $"Value: {value}"
                    );
                    yield return new WaitForSeconds(0.1f);
                }
            }
            // increment
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                SoundManager.Instance.Play("choice_move");
                if (value < max_amount)
                {
                    value++;
                    Debug.Log($"Increment to {value}");
                    ClearMessage();
                    yield return new WaitForSeconds(0.02f);
                    dialogueBackground.gameObject.SetActive(true);
                    ShowMessage(
                        $"{message}\n" +
                        $"Press [A] to {choiceA}\n" +
                        $"Press [D] to {choiceB}\n" +
                        $"Press [Space] to confirm.\n" +
                        $"Value: {value}"
                    );
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // refresh dialog
            //if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            //{
            //    Debug.Log($"refresh dialog");
            //    ClearMessage();
            //    yield return new WaitForSeconds(0.1f);
            //    ShowMessage(
            //        $"{message}\n" +
            //        $"Press [A] to {choiceA}\n" +
            //        $"Press [D] to {choiceB}\n" +
            //        $"Press [Space] to confirm.\n" +
            //        $"Value: {value}"
            //    );
            //    yield return new WaitForSeconds(0.1f);
            //}

            // confirm
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SoundManager.Instance.Play("choice_confirm");
                choiceMade = true;
            }

            yield return null;
        }

        waitingForInput = false;
        ClearMessage();
        onChoiceMade?.Invoke(value);
    }

    public IEnumerator ShowMainMenuAndWait(
    string message, string choiceA, string choiceB,
    System.Action<int> onChoiceMade)
    {
        int player_count = GameSettings.Instance.player_count;
        int round_count = GameSettings.Instance.round_limit;
        //int glory_count = GameSettings.Instance.glory_to_win;

        int choice = 0; // 0=players, 1=rounds, 2=start

        dialogueBackground.gameObject.SetActive(true);

        void Redraw()
        {
            SoundManager.Instance.Play("choice_move"); //222
            string s =
                $"{message}\n" +
                $"{(choice == 0 ? "> " : "")}{choiceA} = {player_count}/{GameSettings.Instance.player_count_max}{(choice == 0 ? " <" : "")}\n" +
                $"{(choice == 1 ? "> " : "")}{choiceB} = {round_count}/{GameSettings.Instance.round_limit_max}{(choice == 1 ? " <" : "")}\n" +
                //$"{(choice == 2 ? "> " : "")}{choiceC} = {glory_count}/{GameSettings.Instance.glory_to_win_max}{(choice == 2 ? " <" : "")}\n" +
                $"{(choice == 2 ? "> Start Game <" : "Start Game")}\n";

            ShowMessage(s);
            ResizeBackground();
        }

        Redraw();

        bool done = false;

        while (!done)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                choice--;
                if (choice < 0) choice = 2;
                Redraw();
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                choice++;
                if (choice > 2) choice = 0;
                Redraw();
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (choice == 0 && player_count < GameSettings.Instance.player_count_max) player_count++;
                else if (choice == 1 && round_count < GameSettings.Instance.round_limit_max) round_count++;
                //else if (choice == 2 && glory_count < GameSettings.Instance.glory_to_win_max) glory_count++;
                Redraw();
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (choice == 0 && player_count > GameSettings.Instance.player_count_min) player_count--;
                else if (choice == 1 && round_count > GameSettings.Instance.round_limit_min) round_count--;
                //else if (choice == 2 && glory_count > GameSettings.Instance.glory_to_win_min) glory_count--;
                Redraw();
            }
            else if (Input.GetKeyDown(KeyCode.Space) && choice == 2)
            {
                SoundManager.Instance.Play("choice_confirm");
                done = true;
            }

            yield return null;
        }

        // Save results
        yield return GameSettings.Instance.player_count = player_count;
        yield return GameSettings.Instance.round_limit = round_count;
        //GameSettings.Instance.glory_to_win = glory_count;

        ClearMessage();
        onChoiceMade?.Invoke(player_count);
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
        dialogueBackground.rectTransform.sizeDelta = textSize + new Vector2(padding * 2, padding * 2);
        //dialogueBackground.rectTransform.sizeDelta = new Vector2(padding * 2, padding * 2);
    }

    private void ResizeTopBackground()
    {
        if (eventDialogue == null || dialogueBackground == null) return;

        // Force TextMeshPro to update its internal layout
        messageDialogue.ForceMeshUpdate();

        // Get the rendered text size
        Vector2 textSize = messageDialogue.GetRenderedValues(false);

        // Set the background size slightly larger than text
        messageBackground.rectTransform.sizeDelta = textSize + new Vector2(padding * 1, padding * 1);
        //dialogueBackground.rectTransform.sizeDelta = new Vector2(padding * 2, padding * 2);
    }

}
