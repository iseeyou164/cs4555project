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

    public void TriggerTrap(int trapID, BoardWalk player)
    {
        /* gonna use a switch to determine which trap to activate */
        switch (trapID)
        {
            case 0:
                Debug.Log("Trap 0: Return to start!");
                player.StartCoroutine(TeleportTrap(player, 0));
                break;

            case 1:
                Debug.Log("Trap 1: Move back 3 tiles");
                player.StartCoroutine(MoveBackwards(player, 3));
                break;

            default:
                Debug.Log("Trap ?");
                player.EndTileEffect();
                break;
        }
    }

    private IEnumerator TeleportTrap(BoardWalk player, int targetIndex)
    {
        player.TeleportToTile(targetIndex);
        yield return null; // optional small delay
        player.EndTileEffect();
    }

    private IEnumerator MoveBackwards(BoardWalk player, int steps)
    {
        // move back tiles
        player.MoveBackwards(steps);
        yield return null; // optional small delay
        player.EndTileEffect();
    }



}
