using System.Collections;
using TMPro;
using UnityEngine;

public class BoardWalk : MonoBehaviour
{
    public Transform[] tiles;
    public float moveSpeed = 5f;

    /*For text display*/
    public TextMeshProUGUI stepLabel = null;

    public int currentTileIndex = 0;
    public bool isMoving = false;

    public IEnumerator MoveSteps(int steps)
    {
        isMoving = true;
        UpdateStepLabel(steps);

        while (steps > 0)
        {

            Transform currentTile = tiles[currentTileIndex];
            SplitTile split = currentTile.GetComponent<SplitTile>();

            if (split != null)
            {
                yield return StartCoroutine(SplitIDManager.Instance.ChooseSplit(this, split));
                yield return new WaitForSeconds(0.1f);
                steps--;
            }
            else
            {
                // No split tile, just go to the next
                currentTileIndex++;
                steps--;
            }

            // Move to currentTileIndex with your MoveToTile coroutine
            float duration = 1f / moveSpeed; // higher speed = shorter duration
            yield return StartCoroutine(MoveToTile(transform.position, tiles[currentTileIndex].position, duration));

            Debug.Log($"Tile {currentTileIndex} / {tiles.Length}");
            UpdateStepLabel(steps);
            yield return new WaitForSeconds(0.1f);
        }

        yield return StartCoroutine(Land(tiles[currentTileIndex]));
        isMoving = false;
    }

    private IEnumerator MoveToTile(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;

        // midpoint lifted slightly for arc effect
        Vector3 midPos = (startPos + endPos) / 2f + Vector3.up * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // move in a simple curved path
            Vector3 curvedPos = Vector3.Lerp(
                Vector3.Lerp(startPos, midPos, t),
                Vector3.Lerp(midPos, endPos, t),
                t
            );

            transform.position = curvedPos;

            yield return null;
        }

        transform.position = endPos;
    }

    void UpdateStepLabel(int steps)
    {
        if(stepLabel != null)
        {
            if (steps > 0)
            {
                stepLabel.text = steps.ToString();
            }
            else
            {
                stepLabel.text = "";
            }
         

        }
    }

    public IEnumerator Land(Transform tile)
    {
        yield return new WaitForSeconds(0.25f);
        if (tile.CompareTag("blue_tile"))
        {
            /* Generic Tile? Maybe give gold when landed on (when that's added?) */
            BoardWalk currentPlayer = TurnManager.Instance.CurrentPlayer;
            currentPlayer.GetComponent<PlayerData>().AddGold(3);
            //currentPlayer.GetComponent<PlayerData>().UpdateStatusUI();
            Debug.Log($"{currentPlayer.GetComponent<PlayerData>().playerName} landed on blue tile +3 Gold!");
            EventManager.IsEventRunning = false;

        }
        else if (tile.CompareTag("red_tile"))
        {
            /* Combat or Obstacle Tile? Could start a fight?*/
            Debug.Log("Landed on red tile. Initiate combat ");
            EventManager.IsEventRunning = false;
        }
        else if (tile.CompareTag("green_tile"))
        {
            Debug.Log("Landed on green tile. Reward ID: ");
            RewardTile reward = tile.GetComponent<RewardTile>();
            if (reward != null)
            {
                Debug.Log("Landed on green tile. Reward ID: " + reward.rewardID);
                yield return RewardIDManager.Instance.TriggerReward(reward.rewardID, this);
            }
        }
        else if (tile.CompareTag("yellow_tile"))
        {
            //isMoving = true;
            TrapTile trap = tile.GetComponent<TrapTile>();
            if (trap != null)
            {
                Debug.Log("Landed on yellow tile. Trap ID: " + trap.trapID);
                yield return TrapIDManager.Instance.TriggerTrap(trap.trapID, this);
            }
        }
        //wait till event's over
        yield return new WaitWhile(() => EventManager.IsEventRunning);
        EndTileEffect();
    }

    // Call this when the effect is fully done
    public void EndTileEffect()
    {
        Debug.Log("End Tile Effect");
        EventManager.IsEventRunning = false;
        isMoving = false;
        
    }

    //public void TeleportToTile(int tileIndex)
    //{
    //    if (tileIndex >= 0 && tileIndex < tiles.Length)
    //    {
    //        StopAllCoroutines();
    //        currentTileIndex = tileIndex;
    //        transform.position = tiles[currentTileIndex].position;
    //        //StartCoroutine(MoveToTile(tiles[currentTileIndex]));
    //        yield return StartCoroutine(MoveToTile(transform.position, nextTile.position, moveSpeed));
    //    }
    //}

    public IEnumerator TeleportToTile(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= tiles.Length) yield break;

        currentTileIndex = tileIndex;

        Vector3 startPos = transform.position;
        Vector3 endPos = tiles[currentTileIndex].position;

        float duration = 0.5f; // smooth teleport animation
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        transform.position = endPos;
    }

    private IEnumerator TeleportRoutine(int tileIndex)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = tiles[tileIndex].position;

        currentTileIndex = tileIndex;
        isMoving = true;

        // Smooth curved movement using your MoveToTile function
        yield return StartCoroutine(MoveToTile(startPos, endPos, 0.5f));

        isMoving = false;
    }

    //public void MoveBackwards(int steps)
    //{
    //    isMoving = true;

    //    Debug.Log((currentTileIndex + 1) + " / " + (tiles.Length));
    //    currentTileIndex -= steps;
    //    if (currentTileIndex < 0)
    //    {
    //        currentTileIndex = 0;
    //    }
    //    TeleportToTile(currentTileIndex);

    //    isMoving = false;
    //}
    public void MoveBackwards(int steps)
    {
        if (isMoving) return;
        isMoving = true;

        currentTileIndex -= steps;
        if (currentTileIndex < 0)
            currentTileIndex = 0;

        StartCoroutine(TeleportRoutine(currentTileIndex));
    }


}
