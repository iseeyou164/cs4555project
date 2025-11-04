using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Added for retreat example

public class BattleMenu : MonoBehaviour
{
    public enum BattleMenuState { Main, Attack, Item, Retreat, None }
    public BattleMenuState currentState = BattleMenuState.Main;

    [Header("UI")]
    public TextMeshProUGUI turnMenuText;

    private int selectedOption = 0;
    private int selectedItem = 0;
    private PlayerData currentPlayer;

    // --- 1. THIS IS THE FIX FOR THE NULLREFERENCEEXCEPTION ---
    void Start()
    {
        currentState = BattleMenuState.None;
        ClearMenu();
    }

    // --- 2. THIS IS THE FUNCTION YOUR ERROR SAYS IS MISSING ---
    // Your BattleManager script calls this to turn on the menu.
    public void InitializeMenu(PlayerData player)
    {
        currentPlayer = player;
        currentState = BattleMenuState.Main;
        selectedOption = 0;
        ShowMainMenu();
    }

    // --- (The rest of the script) ---
    void Update()
    {
        if (currentState == BattleMenuState.None) return;

        switch (currentState)
        {
            case BattleMenuState.Main:
                HandleMainMenuInput();
                break;
            case BattleMenuState.Attack:
                HandleAttackMenuInput();
                break;
            case BattleMenuState.Item:
                HandleItemMenuInput();
                break;
            case BattleMenuState.Retreat:
                HandleRetreatMenuInput();
                break;
        }
    }

    void HandleMainMenuInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) selectedOption = Mathf.Max(0, selectedOption - 1);
        if (Input.GetKeyDown(KeyCode.S)) selectedOption = Mathf.Min(2, selectedOption + 1);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            switch (selectedOption)
            {
                case 0: // ATTACK
                    currentState = BattleMenuState.Attack;
                    Debug.Log("Attack selected");
                    break;
                case 1: // ITEM
                    if (currentPlayer.usedItem == false)
                    {
                        selectedItem = 0;
                        currentState = BattleMenuState.Item;
                        Debug.Log("Item menu opened");
                    }
                    else
                    {
                        Debug.Log("Item menu disabled.");
                    }
                    break;
                case 2: // RETREAT
                    currentState = BattleMenuState.Retreat;
                    Debug.Log("Retreat selected");
                    break;
            }
        }

        DisplayMainMenu();
    }

    void HandleAttackMenuInput()
    {
        turnMenuText.text = $"Attacking!\n(Press Esc to go back)";

        // TODO: ADD YOUR ATTACK LOGIC HERE

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentState = BattleMenuState.Main;
        }
    }

    void HandleItemMenuInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) selectedItem = Mathf.Max(0, selectedItem - 1);
        if (Input.GetKeyDown(KeyCode.S)) selectedItem = Mathf.Min(2, selectedItem + 1);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            string item = currentPlayer.items[selectedItem];
            if (!string.IsNullOrEmpty(item))
            {
                currentPlayer.UseItem(item);
                currentState = BattleMenuState.Main;
            }
            else
            {
                Debug.Log("No item in this slot.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentState = BattleMenuState.Main;
        }

        DisplayItemMenu();
    }

    void HandleRetreatMenuInput()
    {
        turnMenuText.text = $"Trying to retreat...\n(Press Space to confirm)";

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Retreated! Loading main scene...");
            // TODO: Add your scene loading code here
            // SceneManager.LoadScene("YourMainSceneName");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentState = BattleMenuState.Main;
        }
    }

    public void ShowMainMenu()
    {
        Debug.Log("Battle menu opened");
        DisplayMainMenu();
    }

    void DisplayMainMenu()
    {
        if (turnMenuText != null)
        {
            if (selectedOption == 0)
            {
                turnMenuText.text = $"> Attack <\n Use Item\n Retreat";
            }
            else if (selectedOption == 1)
            {
                turnMenuText.text = $"Attack \n > Use Item <\n Retreat";
            }
            else if (selectedOption == 2)
            {
                turnMenuText.text = $"Attack \n Use Item\n > Retreat <";
            }
            else
            {
                turnMenuText.text = $"";
            }

            turnMenuText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            turnMenuText.enabled = false;
            turnMenuText.enabled = true;
        }
    }

    void DisplayItemMenu()
    {
        string output = "";
        for (int i = 0; i < currentPlayer.items.Length; i++)
        {
            if (selectedItem == i)
            {
                output += $"> {currentPlayer.items[i]} <\n";
            }
            else
            {
                output += $"{currentPlayer.items[i]}\n";
            }
        }
        turnMenuText.text = output;
    }

    void ClearMenu()
    {
        if (turnMenuText != null)
        {
            turnMenuText.text = $"";
            turnMenuText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            turnMenuText.enabled = false;
            turnMenuText.enabled = true;
        }
    }

    public void RefreshPlayer()
    {
        // This function is no longer used
    }
}