using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class TurnManager : MonoBehaviour
{

    public static TurnManager Instance;

    [Header("Players")]
    public List<BoardWalk> players; // your player objects
    public int currentPlayerIndex = 0;

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

    public int current_round = 1;
    private int max_round = 10;
    private int max_glory = 10;
    private int player_count = 0;
    public bool gameOver = false;
    public bool gaming = false;

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
        //gaming = true;

    }

    private IEnumerator Start()
    {

        // Wait until GameSettings is fully initialized
        yield return new WaitUntil(() => GameSettings.Instance != null && GameSettings.Instance.settingsApplied);

        gaming = true;
        player_count = GameSettings.Instance.player_count;
        current_round = 1;
        max_round = GameSettings.Instance.round_limit;
        Debug.Log($"-> {player_count} players, {max_round} rounds total!");
        //max_glory = GameSettings.Instance.glory_to_win;

        //turnActive = false;
        focusCam = FindFirstObjectByType<FocusCamera>();
        turnMenu = FindFirstObjectByType<TurnMenu>();
        if (GameState.returningFromBattle)
        {
            Debug.Log("Returning from Battle...");
            currentPlayerIndex = GameState.currentPlayerIndex;
            GameState.returningFromBattle = false;
            NextTurn();
        }
        turnMenu.RefreshPlayer();
        StartCoroutine(GameLoop());
    }

    public IEnumerator StartAgain()
    {
        Debug.Log($"START AGAIN");
        // Wait until GameSettings is fully initialized
        yield return new WaitUntil(() => GameSettings.Instance != null && GameSettings.Instance.settingsApplied);

        gaming = true;
        player_count = GameSettings.Instance.player_count;
        current_round = 1;
        max_round = GameSettings.Instance.round_limit;
        Debug.Log($"-> {player_count} players, {max_round} rounds total!");
        //max_glory = GameSettings.Instance.glory_to_win;

        //turnActive = false;
        focusCam = FindFirstObjectByType<FocusCamera>();
        turnMenu = FindFirstObjectByType<TurnMenu>();
        turnMenu.RefreshPlayer();
        StartCoroutine(GameLoop());
    }

    //void Start()
    //{
    //    gaming = true;
    //    player_count = GameSettings.Instance.player_count;
    //    current_round = 1;
    //    max_round = GameSettings.Instance.round_limit;
    //    max_glory = GameSettings.Instance.glory_to_win;

    //    //turnActive = false;
    //    focusCam = FindFirstObjectByType<FocusCamera>();
    //    turnMenu = FindFirstObjectByType<TurnMenu>();
    //    turnMenu.RefreshPlayer();
    //    StartCoroutine(GameLoop());
    //}

    private IEnumerator GameLoop()
    {
        // Small setup wait
        yield return new WaitForSeconds(0.01f);

        while (gaming == true) // main turn loop
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
        BoardWalk currentPlayer = players[currentPlayerIndex];
        //yield return new WaitWhile(() => !currentPlayer.isMoving);
        PlayerData playerData = PlayerManager.Instance.GetPlayer(currentPlayerIndex);

        DialogManager.Instance.ShowTop($"Round {current_round} / {max_round}! It's {currentPlayer.name}’s turn!");
        yield return new WaitForSeconds(1.5f);
        turnActive = true;


        focusCam.SetTarget(players[currentPlayerIndex].transform);
        var poi = GameObject.Find("PointOfInterest").GetComponent<PointOfInterest>();
        poi.SetTarget(currentPlayer.transform);
        playerData.usedItem = false;
        yield return new WaitForSeconds(0.5f);
        SoundManager.Instance.Play("generic_ping");
        yield return DialogManager.Instance.ShowMessageAndWait($"It's {currentPlayer.name}’s turn!");

        //poison effect
        if (playerData.poisonDuration > 0)
        {
            //Debug.Log($"{currentPlayer.name} takes {playerData.poisonDuration} damage from poison. {playerData.poisonDuration-1} poison left!");
            yield return DialogManager.Instance.ShowMessageAndWait($"{currentPlayer.name} takes {playerData.poisonDuration} damage from poison. {playerData.poisonDuration - 1} poison left!");
            playerData.gainHealth(-playerData.poisonDuration);
            playerData.poisonDuration -= 1;
        }

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
        // Move to next player
        currentPlayerIndex = (currentPlayerIndex + 1) % GameSettings.Instance.player_count; ;
        Debug.Log($"Turn: {currentPlayerIndex+1}/{GameSettings.Instance.player_count}");

        // If we wrapped back to 0, all players finished a round
        if (currentPlayerIndex == 0)
        {
            current_round++;

            if (current_round <= max_round)
            {
                Debug.Log($"Round advanced: {current_round}/{max_round}");

                Debug.Log($"Turn switched to: {GetCurrentPlayer().playerName}");
                //currentPlayerIndex = (currentPlayerIndex + 1) % PlayerManager.Instance.players.Count;
                //Debug.Log($"Turn switched to: {GetCurrentPlayer().playerName}");
                turnActive = true;
                //StartCoroutine(GameLoop());
            }
            else
            {
                Debug.Log("Max rounds reached. Ending game.");
                StartCoroutine(EndGame());
                return;
            }
        }
    }

    //private IEnumerable goEndGame()
    //{
    //    yield return EndGame();
    //}

    public IEnumerator EndGame()
    {
        gaming = false;
        Debug.Log("GAME OVER — Max rounds reached!");
        // Stop the loop

        // Optionally show a final dialog
        DialogManager.Instance.ShowTop("Game Over!");

        PlayerData winner = null;
        int max_score = -1;
        int i_score;

        //get final score
        for (int i = 0; i < player_count; i++)
        {
            i_score = PlayerManager.Instance.GetPlayer(i).calculateScore();
            if (i_score > max_score)
            {
                winner = PlayerManager.Instance.GetPlayer(i);
                max_score = i_score;
            }
        }
        focusCam.SetTarget(winner.transform);
        var poi = GameObject.Find("PointOfInterest").GetComponent<PointOfInterest>();

        yield return DialogManager.Instance.ShowMessageAndWait($"Game Over! Winner: {winner.playerName} with {winner.glory} glory and {winner.gold} gold!");

        gaming = true;

        //StopAllCoroutines();
        current_round = 1;
        currentPlayerIndex = 0;
        turnActive = false;
        GameSettings.initialized = false;
        yield return StartCoroutine(GameSettings.Instance.ShowSettingsMenuAgain());
        gameOver = false;
        StartCoroutine(GameLoop());

    }

    //public IEnumerator EndGame(PlayerData winner)
    //{
    //    if (gameOver) yield break;
    //    gameOver = true;
    //    Debug.Log($"GAME OVER — {winner.playerName} reaches {max_glory} glory!");
    //    // Stop the loop
    //    turnActive = false;

    //    // Optionally show a final dialog
    //    DialogManager.Instance.ShowTop($"Game Over! {winner.playerName} reached {max_glory} glory!");

    //    //PlayerData winner = null;
    //    int max_score = -1;
    //    int i_score;

    //    //get final score
    //    for (int i = 0; i < player_count; i++)
    //    {
    //        i_score = PlayerManager.Instance.GetPlayer(i).calculateScore();
    //        if (i_score > max_score)
    //        {
    //            winner = PlayerManager.Instance.GetPlayer(i);
    //            max_score = i_score;
    //        }
    //    }

    //    focusCam.SetTarget(winner.transform);
    //    var poi = GameObject.Find("PointOfInterest").GetComponent<PointOfInterest>();

    //    yield return DialogManager.Instance.ShowMessageAndWait($"Game Over! Winner: {winner.playerName} with {winner.glory} glory and {winner.gold} gold!");

    //    //gaming = true;

    //    //StopAllCoroutines();
    //    current_round = 1;
    //    currentPlayerIndex = 0;
    //    turnActive = false;
    //    yield return StartCoroutine(GameSettings.Instance.ShowSettingsMenuAgain());
    //    gameOver = false;
    //    StartCoroutine(GameLoop());

    //}

}
