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
        //turnActive = false;
        focusCam = FindFirstObjectByType<FocusCamera>();
        turnMenu = FindFirstObjectByType<TurnMenu>();
        turnMenu.RefreshPlayer();
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        // Small setup wait
        yield return new WaitForSeconds(0.01f);

        while (true) // main turn loop
        {
            Debug.Log($"Next turn");
            yield return StartCoroutine(StartTurn());
            NextTurn();
        }
    }

    private IEnumerator StartTurn()
    {
        //yield return new WaitWhile(() => turnActive=false);
        yield return new WaitWhile(() => EventManager.IsEventRunning);
        yield return new WaitForSeconds(1.5f);
        turnActive = true;
        BoardWalk currentPlayer = players[currentPlayerIndex];
        //yield return new WaitWhile(() => !currentPlayer.isMoving);
        PlayerData playerData = PlayerManager.Instance.GetPlayer(currentPlayerIndex);
        focusCam.SetTarget(players[currentPlayerIndex].transform);
        var poi = GameObject.Find("PointOfInterest").GetComponent<PointOfInterest>();
        poi.SetTarget(currentPlayer.transform);
        yield return new WaitForSeconds(0.5f);

        //poison effect
        if (playerData.poisonDuration > 0)
        {
            Debug.Log($"It’s {currentPlayer.name} takes {playerData.poisonDuration} damage from poison. {playerData.poisonDuration-1} poison left!");
            playerData.gainHealth(-playerData.poisonDuration);
            playerData.poisonDuration -= 1;
        }

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

        // Wait for tile effects (e.g., green/gold tiles)
        //while (currentPlayer.isMoving)
            //yield return null;

        yield return new WaitUntil(() => !currentPlayer.isMoving);
        // Turn finished, go to next player!
        hasRolledThisTurn = false;
        totalRollResult = 0;
        turnActive = false;
        //NextTurn();
        //StartCoroutine(StartTurn());

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
        turnActive = true;
        //StartCoroutine(GameLoop());
    }
}
