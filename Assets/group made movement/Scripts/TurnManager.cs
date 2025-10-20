using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class TurnManager : MonoBehaviour
{

    public static TurnManager Instance;

    [Header("Players")]
    public List<BoardWalk> players; // your player objects
    private int currentPlayerIndex = 0;

    [Header("Dice")]
    public int diceCount = 1;
    public int diceMaxValue = 6;
    public int diceMinValue = 1;

    [Header("State")]
    public bool turnActive = false; // is a player currently taking a turn?

    [HideInInspector] public bool hasRolledThisTurn = false;
    [HideInInspector] public int totalRollResult = 0;

    private FocusCamera focusCam;
    private TurnMenu turnMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        focusCam = FindFirstObjectByType<FocusCamera>();
        turnMenu = FindFirstObjectByType<TurnMenu>();
        turnMenu.RefreshPlayer();
        StartCoroutine(StartTurn());
    }

   private IEnumerator StartTurn()
    {
        yield return new WaitForSeconds(1.5f);
        turnActive = true;
        BoardWalk currentPlayer = players[currentPlayerIndex];
        PlayerData playerData = PlayerManager.Instance.GetPlayer(currentPlayerIndex);
        focusCam.SetTarget(players[currentPlayerIndex].transform);
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"It’s {currentPlayer.name}’s turn!");
        playerData.usedItem = false;

        // Refresh the menu for the new player
        TurnMenu turnMenu = FindFirstObjectByType<TurnMenu>();
        turnMenu.RefreshPlayer();
        turnMenu.currentState = TurnMenu.MenuState.Main;
        //turnMenu.ShowMainMenu();

        // Now wait until the TurnMenu triggers the roll
        yield return new WaitUntil(() => hasRolledThisTurn);

        // Move the player (triggered by TurnMenu after roll)
        yield return StartCoroutine(currentPlayer.MoveSteps(totalRollResult));

        // Wait for tile effects (e.g., blue/gold tiles)
        while (currentPlayer.isMoving)
            yield return null;

        // Reset flags
        hasRolledThisTurn = false;
        totalRollResult = 0;


        // Turn finished, go to next player!
        NextTurn();
        StartCoroutine(StartTurn());

    }

    public BoardWalk CurrentPlayer
    {
        get { return players[currentPlayerIndex]; }
    }

    public PlayerData GetCurrentPlayer()
    {
        return PlayerManager.Instance.GetPlayer(currentPlayerIndex);
    }

    public void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % PlayerManager.Instance.players.Count;
        Debug.Log($"Turn switched to: {GetCurrentPlayer().playerName}");
    }
}
