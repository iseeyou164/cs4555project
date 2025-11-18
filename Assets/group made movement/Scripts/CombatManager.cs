using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    void Awake()
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

    public IEnumerator TriggerCombat(int combatID, Transform enemy_spot, BoardWalk player)
    {
        EventManager.IsEventRunning = true;
        /* gonna use a switch to determine which trap to activate */
        switch (combatID)
        {
            case 0:
                Debug.Log("Combat 0: Skeleton!");
                yield return FightSkeleton(player, enemy_spot);
                break;

            case 1:
                Debug.Log("Combat 1: Turtle!");
                yield return FightTurtle(player, enemy_spot);
                break;

            case 2:
                Debug.Log("Combat 2: Orc!");
                yield return FightOrc(player, enemy_spot);
                break;

            case 3:
                Debug.Log("Combat 3: Golem!");
                yield return FightGolem(player, enemy_spot);
                break;

            //case 4:
            //    Debug.Log("Trap 4: Dodge the arrow from the goblin watchtower!");
            //    yield return GoblinTower(player);
            //    break;

            //case 4: mushroom launch pad: launches them down the cliff

            default:
                Debug.Log("Combat ?");
                player.EndTileEffect();
                break;
        }
    }

    private IEnumerator FightSkeleton(BoardWalk player, Transform enemy_spot)
    {
        GameState.enemyToSpawn = "Skeleton";
        setGameState();
        yield return new WaitForSeconds(0.2f);
        int i = 0;
        yield return StartCoroutine(BattleSystem.Instance.StartBattle(
            player.GetComponent<PlayerData>(),
            enemy_spot,
            GameState.enemyToSpawn,
            (BattleResult result) =>
            {
                if (result == BattleResult.WIN)
                {

                    Debug.Log("Player won!");
                    i = 1;

                }
                else if (result == BattleResult.LOSE)
                {
                    Debug.Log("Player lost!");
                    i = 2;
                }
                else
                {
                    Debug.Log("Player fled!");
                }

                // End tile effect and continue board game
            }
        ));

        if (i == 1)
        {
            SoundManager.Instance.Play("generic_glory");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Victory] You gain 1 Double Dice!");
            player.GetComponent<PlayerData>().AddItem("Double Dice");
        }else if (i == 2)
        {
            yield return player.GetComponent<PlayerData>().Die();

        }

        //SceneManager.LoadScene("battle scene", LoadSceneMode.Additive);
        //yield return BattleSystem.Instance.setupBattle(GameState.enemyToSpawn);
        //rewards: double dice
        Debug.Log("Combat Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }
    private IEnumerator FightTurtle(BoardWalk player, Transform enemy_spot)
    {
        GameState.enemyToSpawn = "Turtle";
        setGameState();
        yield return new WaitForSeconds(0.2f);
        int i = 0;
        yield return StartCoroutine(BattleSystem.Instance.StartBattle(
            player.GetComponent<PlayerData>(),
            enemy_spot,
            GameState.enemyToSpawn,
            (BattleResult result) =>
            {
                if (result == BattleResult.WIN)
                {
                    Debug.Log("Player won!");
                    i = 1;
                }
                else if (result == BattleResult.LOSE)
                {
                    Debug.Log("Player lost!");
                    i = 2;
                }
                else
                {
                    Debug.Log("Player fled!");
                }

                // End tile effect and continue board game
            }
        ));

        if (i == 1)
        {
            SoundManager.Instance.Play("generic_glory");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Victory] You gain 10 Gold!");
            player.GetComponent<PlayerData>().AddGold(10);
        }
        else if (i == 2)
        {
            yield return player.GetComponent<PlayerData>().Die();

        }
        Debug.Log("Combat Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();

        //SceneManager.LoadScene("battle scene", LoadSceneMode.Additive);
        //yield return BattleSystem.Instance.setupBattle(GameState.enemyToSpawn);
        //rewards: 5-10 gold
    }
    private IEnumerator FightOrc(BoardWalk player, Transform enemy_spot)
    {
        GameState.enemyToSpawn = "Orc";
        setGameState();
        yield return new WaitForSeconds(0.2f);
        int i = 0;
        yield return StartCoroutine(BattleSystem.Instance.StartBattle(
            player.GetComponent<PlayerData>(),
            enemy_spot,
            GameState.enemyToSpawn,
            (BattleResult result) =>
            {
                if (result == BattleResult.WIN)
                {
                    Debug.Log("Player won!");
                    i = 1;
                }
                else if (result == BattleResult.LOSE)
                {
                    Debug.Log("Player lost!");
                    i = 2;
                }
                else
                {
                    Debug.Log("Player fled!");
                }

                // End tile effect and continue board game
            }
        ));
        //rewards: 7-15 gold
        if (i == 1)
        {
            SoundManager.Instance.Play("generic_glory");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Victory] You gain a Triple Dice!");
            player.GetComponent<PlayerData>().AddItem("Triple Dice");
            player.GetComponent<PlayerData>().level += 1;
        }
        else if (i == 2)
        {
            yield return player.GetComponent<PlayerData>().Die();

        }
        Debug.Log("Combat Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();

    }
    private IEnumerator FightGolem(BoardWalk player, Transform enemy_spot)
    {
        GameState.enemyToSpawn = "Golem";
        setGameState();
        yield return new WaitForSeconds(0.2f);
        int i = 0;
        yield return StartCoroutine(BattleSystem.Instance.StartBattle(
            player.GetComponent<PlayerData>(),
            enemy_spot,
            GameState.enemyToSpawn,
            (BattleResult result) =>
            {
                if (result == BattleResult.WIN)
                {
                    Debug.Log("Player won!");
                    i = 1;
                }
                else if (result == BattleResult.LOSE)
                {
                    Debug.Log("Player lost!");
                    i = 2;
                }
                else
                {
                 Debug.Log("Player fled!");
                }

        // End tile effect and continue board game
    }
        ));
        if (i == 1)
        {
            SoundManager.Instance.Play("generic_glory2");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Victory] You gain 1 Glory!");
            player.GetComponent<PlayerData>().AddGlory(1);
        }
        else if (i == 2)
        {
            yield return player.GetComponent<PlayerData>().Die();

        }
        Debug.Log("Combat Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();

        //SceneManager.LoadScene("battle scene", LoadSceneMode.Additive);
        //yield return BattleSystem.Instance.setupBattle(GameState.enemyToSpawn);
        //rewards: triple dice
    }


    public void setGameState()
    {
        foreach (var player in TurnManager.Instance.players)
        {
            if (GameState.playerTileIndices.ContainsKey(player.name))
            {
                GameState.playerTileIndices[player.name] = player.currentTileIndex;
            }
            else
            {
                GameState.playerTileIndices.Add(player.name, player.currentTileIndex);
            }
        }
        GameState.currentPlayerIndex = TurnManager.Instance.currentPlayerIndex;
        GameState.returningFromBattle = true;
    }
}
