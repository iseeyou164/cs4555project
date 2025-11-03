using System.Collections;
using UnityEngine;

public class TrapIDManager : MonoBehaviour
{
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
                yield return DodgeBoulderTrap(player);
                break;

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
            player.TeleportToTile(targetIndex);

        }
        EventManager.IsEventRunning = false;
        //player.EndTileEffect();
    }

    private IEnumerator MoveBackwards(BoardWalk player, int steps)
    {
        yield return DialogManager.Instance.ShowMessageAndWait($"A trap triggers! You stumble backward {steps} spaces!");
        // move back tiles
        player.MoveBackwards(steps);
        yield return null; // optional small delay
        EventManager.IsEventRunning = false;
        //player.EndTileEffect();
    }

    private IEnumerator DodgeBoulderTrap(BoardWalk player)
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
            int x = 5;
            yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You failed to dodge and take {x} damage. ");
            //player data?
            player.GetComponent<PlayerData>().gainHealth(-x);

        }
        EventManager.IsEventRunning = false;
        //player.EndTileEffect();



    }
}
