using System.Collections;
using UnityEngine;

public class TrapIDManager : MonoBehaviour
{ 
    public GameObject boulderPrefab;
    public GameObject poisonIvyEffectPrefab;

    public static TrapIDManager Instance;
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

    public IEnumerator TriggerTrap(int trapID, BoardWalk player)
    {
        EventManager.IsEventRunning = true;
        /* gonna use a switch to determine which trap to activate */
        switch (trapID)
        {
            case 0:
                Debug.Log("Trap 0: Return to start!");
                yield return TeleportTrap(player, 0);
                break;

            case 1:
                Debug.Log("Trap 1: Move back 3 tiles");
                yield return MoveBackwards(player, 3);
                break;

            case 2:
                Debug.Log("Trap 2: Dodge the boulder!");
                yield return BoulderTrap(player);
                break;

            case 3:
                Debug.Log("Trap 3: Avoid being pricked by poison ivy!");
                yield return PoisonIvyTrap(player);
                break;

            //case 4: mushroom launch pad: launches them down the cliff

            default:
                Debug.Log("Trap ?");
                player.EndTileEffect();
                break;
        }
    }

    private IEnumerator TeleportTrap(BoardWalk player, int targetIndex)
    {
        yield return DialogManager.Instance.ShowMessageAndWait($"You see yourself being pulled in into a portal!");
        yield return new WaitForSeconds(0.25f);
        yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Roll 10+ to escape!");

        bool finished = false;
        int result = 0;

        // Roll a visual d20
        yield return DiceRoller.Instance.StartCoroutine(
            DiceRoller.Instance.RollDiceVisual(20, 1, (total) =>
            {
                result = total;
                finished = true;
            })
        );

        yield return new WaitUntil(() => finished);

        if (result >= 10)
        {
            yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You dodged successfully!");
        }
        else
        {
            yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You failed to dodge and return to start!");
            yield return player.StartCoroutine(player.TeleportToTile(targetIndex));
            //player.TeleportToTile(targetIndex);

            //yield return new WaitForSeconds(0.25f);
            //yield return player.StartCoroutine(player.Land(player.tiles[targetIndex]));

        }
        Debug.Log("Trap Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

    private IEnumerator MoveBackwards(BoardWalk player, int steps)
    {
        yield return DialogManager.Instance.ShowMessageAndWait($"A trap triggers! You stumble backward {steps} spaces!");
        // move back tiles
        player.MoveBackwards(steps);
        yield return null; // optional small delay
        Debug.Log("Trap Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

    private IEnumerator BoulderTrap(BoardWalk player)
    {
        yield return DialogManager.Instance.ShowMessageAndWait("You see a boulder falling on top of you!");
        yield return new WaitForSeconds(0.25f);
        yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Roll 10+ to dodge!");

        bool finished = false;
        int result = 0;

        // Roll a visual d20
        yield return DiceRoller.Instance.StartCoroutine(
            DiceRoller.Instance.RollDiceVisual(20, 1, (total) =>
            {
                result = total;
                finished = true;
            })
        );

        yield return new WaitUntil(() => finished);

        if (result >= 10)
        {
            yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You dodged successfully!");
        }
        else
        {
            //int x = 5;
            //yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You failed to dodge and take {x} damage. ");
            ////player data?
            //player.GetComponent<PlayerData>().gainHealth(-x);
            int damage = 5;
            yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! A boulder falls on you!");

            //Spawn the boulder visually above the player
            if (boulderPrefab != null)
            {
                GameObject boulder = Instantiate(boulderPrefab, player.transform.position + Vector3.up * 10f, Quaternion.identity);

                yield return new WaitForSeconds(1.5f);
                yield return DialogManager.Instance.ShowMessageAndWait($"The boulder crushed you! You take {damage} damage!");

                player.GetComponent<PlayerData>().gainHealth(-damage);

                Destroy(boulder, 0.5f); // cleanup
            }
            else
            {
                Debug.LogWarning("Boulder prefab not assigned!");
                player.GetComponent<PlayerData>().gainHealth(-damage);
            }

        }
        Debug.Log("Trap Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

    private IEnumerator PoisonIvyTrap(BoardWalk player)
    {
        yield return DialogManager.Instance.ShowMessageAndWait("You attempt to walk through thick grass.");
        yield return new WaitForSeconds(0.25f);
        yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Roll 10+ to remain unscathed.");

        bool finished = false;
        int result = 0;

        // Roll a visual d20
        yield return DiceRoller.Instance.StartCoroutine(
            DiceRoller.Instance.RollDiceVisual(20, 1, (total) =>
            {
                result = total;
                finished = true;
            })
        );

        yield return new WaitUntil(() => finished);

        if (result >= 20)
        {
            yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Success!] You rolled {result}! You found {result} gold on the ground!");
            player.GetComponent<PlayerData>().AddGold(result);
        }
        if (result >= 10)
        {
            yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You remain unscathed by the grass!");
        }
        else
        {
            //int x = 5;
            //yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You failed to dodge and take {x} damage. ");
            ////player data?
            //player.GetComponent<PlayerData>().gainHealth(-x);
            int damage = 3;
            yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You take {damage} damage and poisoned for {damage} turn(s)!");

            //Spawn the boulder visually above the player
            if (poisonIvyEffectPrefab != null)
            {
                GameObject effect = Instantiate(poisonIvyEffectPrefab, player.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                yield return new WaitForSeconds(0.5f); // wait for particles to play
                Destroy(effect);

                player.GetComponent<PlayerData>().gainHealth(-damage);
                player.GetComponent<PlayerData>().ApplyEffect("Poison", damage);
            }
            else
            {
                Debug.LogWarning("Poison Ivy Effect prefab not assigned!");
                player.GetComponent<PlayerData>().gainHealth(-damage);
            }

        }
        Debug.Log("Trap Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

}
