using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;
    public Transform[] tiles;

    [Header("Player Object References")]
    public GameObject[] playerObjects;        // assign Player_0, Player_1, ...
    public GameObject[] playerStatusUI;       // assign UI 0, UI 1, ...

    public int round_limit = 10;
    public int round_limit_min = 5;
    public int round_limit_max = 99;

    //public int glory_to_win_min = 1;
    //public int glory_to_win = 3;
    //public int glory_to_win_max = 99;

    public int player_count_min = 2;
    public int player_count = 2;
    public int player_count_max = 4;

    public bool settingsApplied = false;

    void Awake()
    {
        Instance = this;
        //StartCoroutine(StartupRoutine());
    }

    void Start()
    {
        StartCoroutine(StartupRoutine());
        //StartCoroutine(DialogManager.Instance.ShowMainMenuAndWait(
        //    "Game Settings",
        //    "Player Count",
        //    "Round Limit",
        //    (result) =>
        //    {
        //        ApplyPlayerSettings();
        //    }));
    }

    public IEnumerator ShowSettingsMenuAgain()
    {
        //bool reset = false;
        for (int i = 0; i < playerObjects.Length; i++)
        {
            PlayerData pd = playerObjects[i].GetComponent<PlayerData>();
            pd.ResetForNewGame();
        }
        //yield return new WaitUntil(() => reset == true);

        DialogManager.Instance.ShowTop("Use [W,S,Up,Down] to traverse menu.\nUse [A,D,Left,Right] to adjust values.");

        Debug.Log($"RETURN TO SETTINGS");
        settingsApplied = false;
        TurnManager.Instance.gaming = false;
        yield return DialogManager.Instance.ShowMainMenuAndWait(
            "Game Settings:",
            "Player Count",
            "Round Count",
            (int pc) =>
            {
                player_count = pc;
                Debug.Log($"Player Count: {player_count}\n" +
                    $"Round Count: {round_limit}\n");
            }
            );

        ApplyPlayerSettings();
        //settingsApplied = true;
        //TurnManager.Instance.gaming = true;
        //TurnManager.Instance.StartAgain();
    }

    private IEnumerator StartupRoutine()
    {
        //PlayerData.Instance.players.ResetForNewGame();
        DialogManager.Instance.ShowTop("[W,S,Up,Down] to traverse menu.\n[A,D,Left,Right] to adjust values.\n[Space] on Start Game to continue.");
        settingsApplied = false;
        TurnManager.Instance.gaming = false;
        yield return DialogManager.Instance.ShowMainMenuAndWait(
            "Game Settings:",
            "Player Count",
            "Round Count",
            (int pc) =>
                {
                    player_count = pc;
                    Debug.Log($"Player Count: {player_count}\n" +
                        $"Round Count: {round_limit}");
                }
            );

        ApplyPlayerSettings();
        //settingsApplied = true;
        //TurnManager.Instance.gaming = true;
        //TurnManager.Instance.StartAgain();
    }

    //private IEnumerator Start()
    //{
    //    yield return DialogManager.Instance.ShowMainMenuAndWait(
    //        "Game Settings:",
    //        "Player Count",
    //        "Round Count",
    //        "Glory Count",
    //        (bool choice) =>
    //            {
    //                Debug.Log($"Player Count: {player_count}\n" +
    //                    $"Round Count: {round_limit}\n" +
    //                    $"Glory Count: {glory_to_win}\n");
    //            }
    //        );

    //    ApplyPlayerSettings();
    //}

    public void ApplyPlayerSettings()
    {

        PlayerManager.Instance.players.Clear();

        for (int i = 0; i < playerObjects.Length; i++)
        {
            bool enable = (i < player_count);



            playerObjects[i].SetActive(enable);
            playerStatusUI[i].SetActive(enable);

            if (enable)
            {
                // Ensure this PlayerData gets registered
                PlayerData pd = playerObjects[i].GetComponent<PlayerData>();
                if (pd != null)
                {
                    PlayerManager.Instance.RegisterPlayer(pd);

                    // Assign index to BoardWalk so it knows who it is
                    //BoardWalk bw = playerObjects[i].GetComponent<BoardWalk>();
                    //if (bw != null)
                    //    bw.playerIndex = index;
                }
            }
        }
        settingsApplied = true;
        TurnManager.Instance.gaming = true;
        TurnManager.Instance.StartAgain();
    }


    }
