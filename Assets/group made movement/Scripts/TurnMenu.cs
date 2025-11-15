using JetBrains.Annotations;
using System.Collections;
using System.Security.Cryptography;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEditor.Rendering.CameraUI;
using static UnityEngine.Rendering.DebugUI;

public class TurnMenu : MonoBehaviour
{

    public enum MenuState { Main, Move, Item, Map, None }
    public MenuState currentState = MenuState.None;

    [Header("UI")]
    public TextMeshProUGUI moveText;
    public TextMeshProUGUI itemText;
    public TextMeshProUGUI mapText;

    [Header("UI2")]
    public TextMeshProUGUI turnMenuText;

    private int selectedOption = 0; // 0=Move, 1=Item, 2=Map
    private int selectedItem = 0;
    //private int item_count = 0;

    private PlayerData currentPlayer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //?
        //currentState = MenuState.Main;
        //selectedOption = 0;
        //currentPlayer = TurnManager.Instance.GetCurrentPlayer();
        //ShowMainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case MenuState.Main:
                HandleMainMenuInput();
                break;
            case MenuState.Move:
                HandleMoveMenuInput();
                break;
            case MenuState.Item:
                HandleItemMenuInput();
                break;
            case MenuState.Map:
                HandleMapMenuInput();
                break;
            case MenuState.None:
                ClearMenu();
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
                case 0: // MOVE
                    currentState = MenuState.Move;
                    Debug.Log("Move selected");
                    break;
                case 1: // ITEM
                    if (currentPlayer.usedItem == true || currentPlayer.ItemCount()==0)
                    {
                        Debug.Log("Item menu disabled. Player has already used item this turn or has no items.");
                    }
                    else
                    {
                        selectedItem = 0;
                        currentState = MenuState.Item;
                        Debug.Log("Item menu opened");
                        //pls work!
                    }
                    break;
                case 2: // MAP
                    currentState = MenuState.Map;
                    Debug.Log("Map menu opened");
                    break;
                default: //nothing
                    currentState = MenuState.None;
                    Debug.Log("Clear Text");
                    break;
            }
        }

        DisplayMainMenu();
    }

    //figure out this crap (goal: get a menu system working)
    void UpdateMenuUI()
    {
        moveText.color = (selectedOption == 0) ? Color.yellow : Color.white;
        itemText.color = (selectedOption == 1) ? Color.yellow : Color.white;
        mapText.color = (selectedOption == 2) ? Color.yellow : Color.white;
    }


    //Dice Roll! (add multiple dice functionality later. After rolling the first dice, disable esc key))
    void HandleMoveMenuInput()
    {
        turnMenuText.text = $"Press [Space] to roll dice.\nPress [Esc] to go back to menu.";

        //Debug.Log("Gonna Roll?");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentState = MenuState.None;
            StartCoroutine(RollDiceCoroutine());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentState = MenuState.Main;
            Debug.Log("Back to main menu");
        }
    }

    private IEnumerator RollDiceCoroutine()
    {

        Debug.Log($"{currentPlayer.name} is rolling the dice...");
        //Debug.Log($"{currentPlayer.minMoveRoll} minroll & {currentPlayer.maxMoveRoll} maxroll");
        turnMenuText.text = $"";

        bool resultReady = false;
        int roll = 0;

        if (currentPlayer.diceCount == -1)
        {
            StartCoroutine(LuckyDiceFlow((result) =>
            {
                roll = result;
                resultReady = true;
            }));
        }
        else
        {
            yield return StartCoroutine(DiceRoller.Instance.RollDiceVisual(6, currentPlayer.diceCount, result =>
            {
                roll = result;
                resultReady = true;
            }));
        }

        while (!resultReady)
            yield return null;

        //roll = currentPlayer.RollDice(6, currentPlayer.diceCount);
        //yield return new WaitForSeconds(0.5f); // add a brief pause for effect)

        //while (currentPlayer.diceCount > 0)
        //{
        //    // Simulate dice roll (press space once to roll one die)
        //    roll = currentPlayer.RollDice(6, currentPlayer.diceCount);
        //    currentPlayer.diceCount -= 1;
        //    if (currentPlayer.diceCount > 0)
        //    {
        //        Debug.Log($" Current Total = {roll}: {currentPlayer.name} has {currentPlayer.diceCount} dice left!");
        //    }

        //    yield return new WaitForSeconds(0.5f); // add a brief pause for effect)

        //}

        // Apply move bonus (from Pixie Dust, etc.)
        if (currentPlayer.moveBonus != 0)
        {
            Debug.Log($"{currentPlayer.name} rolled {roll} + {currentPlayer.moveBonus}");
            roll += currentPlayer.moveBonus;
            currentPlayer.moveBonus = 0;
        }
        else
        {
            Debug.Log($"{currentPlayer.name} rolled a {roll}");
        }

        // Report roll to TurnManager
        TurnManager.Instance.totalRollResult = roll;
        TurnManager.Instance.hasRolledThisTurn = true;

        // Reset dice count for next turn
        currentPlayer.diceCount = 1;
        //currentPlayer.GetComponent<BoardWalk>.isMoving = true;

        Debug.Log($"{currentPlayer.name} will move {roll} spaces!");

        // Clear text
        currentState = MenuState.None;
        //DisplayMainMenu();
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
                currentState = MenuState.None;
                StartCoroutine(UseItemFlow(item));   // <- Run wrapper coroutine
            }
            else
            {
                Debug.Log("No item in this slot.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentState = MenuState.Main;
        }

        DisplayItemMenu();
    }

    private IEnumerator UseItemFlow(string itemName)
    {
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(currentPlayer.UseItem(itemName));

        // After item completes:
        Debug.Log($"Finished using item: {itemName}");
        currentState = MenuState.Main;
        DisplayMainMenu();
    }
    private IEnumerator LuckyDiceFlow(System.Action<int> onChoiceMade)
    {
        int lucky = 0;
        yield return new WaitForSeconds(0.1f);
        //yield return StartCoroutine(currentPlayer.UseItem(itemName));
        yield return DialogManager.Instance.ShowAmountChoiceAndWait(
            "Rolling Lucky Dice. Choose dice roll from 1 to 10!",
            "-1!",
            "+1!",
            1,
            10,
            (int value) =>
            {
                lucky = value;
                //amount_to_gamble += value;
                //player.GetComponent<PlayerData>().AddGold(-amount_to_gamble);
            }
        );
        // After item completes:
        onChoiceMade?.Invoke(lucky);
        Debug.Log($"Finished using Lucky Dice");
    }

    void HandleMapMenuInput()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentState = MenuState.Main;
            Debug.Log("Back to main menu");
        }
    }
    public void ShowMainMenu()
    {
        Debug.Log("Turn menu opened");
        DisplayMainMenu();
    }

    void DisplayMainMenu()
    {
        //string[] options = { "MOVE", "ITEM", "MAP" };
        //string output = "Main Menu:\n";
        if (turnMenuText != null)
        {
            if (selectedOption == 0)
            {
                turnMenuText.text = $"> Roll Dice <\n Use Item\n View Map";
            }
            else if (selectedOption == 1)
            {
                turnMenuText.text = $"Roll Dice \n > Use Item <\n View Map";
            }
            else if (selectedOption == 2)
            {
                turnMenuText.text = $"Roll Dice \n Use Item\n > View Map <";
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
        //string output = "Items:\n";
        //for (int i = 0; i < currentPlayer.items.Length; i++)
        //{
        //    string item = currentPlayer.items[i] ?? "(empty)";
        //    if (i == selectedItem) output += $"> {item}\n";
        //    else output += $"  {item}\n";
        //}
        //Debug.Log(output);
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
        //?
    }

        //if (selectedOption == 0)
        //{
        //    turnMenuText.text = $"> {currentPlayer.items[0]} <\n {currentPlayer.items[1]} \n {currentPlayer.items[2]}";
        //}
        //else if (selectedOption == 1)
        //{
        //    turnMenuText.text = $"{currentPlayer.items[0]} \n > {currentPlayer.items[1]} < \n {currentPlayer.items[2]}";
        //}
        //else if (selectedOption == 2)
        //{
        //    turnMenuText.text = $"{currentPlayer.items[0]} \n {currentPlayer.items[1]} \n > {currentPlayer.items[2]} <";
        //}
        //else
        //{
        //    turnMenuText.text = $"";
        //}

    void ClearMenu()
    {
        turnMenuText.text = $"";
        turnMenuText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
        turnMenuText.enabled = false;
        turnMenuText.enabled = true;
    }

    //get the current turn player
    public void RefreshPlayer()
    {
        currentPlayer = TurnManager.Instance.GetCurrentPlayer();
    }

}
