using System.Collections;
using TMPro;
using UnityEngine;

public class BoardWalk : MonoBehaviour
{
    public Transform[] tiles;
    public float moveSpeed = 5f;

    /*For text display*/
    public TextMeshProUGUI stepLabel = null;

    private int currentTileIndex = 0;
    public bool isMoving = false;

    //void Update()
    //{

    //    if (Input.GetKeyDown(KeyCode.Space) && !isMoving)
    //    {
    //        /* random value from 1 to 6 */
    //        int steps = Random.Range(1, 7);
    //        UpdateStepLabel(steps);
    //        StartCoroutine(MoveSteps(steps));
    //    }
    //}

    public IEnumerator MoveSteps(int steps)
    {
        isMoving = true;
        UpdateStepLabel(steps);
        while (steps > 0)
        {
            Debug.Log((currentTileIndex + 1) + " / " + (tiles.Length));
            if (currentTileIndex < tiles.Length - 1)
            {
                currentTileIndex++;
            }
            else
            {
                // Loop back to start or tile 2 (for now)
                currentTileIndex = 2;
            }

            // Move toward the next tile
            Vector3 startPos = transform.position;
            Vector3 endPos = tiles[currentTileIndex].position;
            float t = 0f;
            float duration = 0.3f;
            duration *= moveSpeed;

            while (t < duration)
            {
                transform.position = Vector3.Lerp(startPos, endPos, t / duration);
                t += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;
            steps--;
            UpdateStepLabel(steps);
        }

        yield return StartCoroutine(Land(tiles[currentTileIndex]));
        isMoving = false;
    }

    IEnumerator MoveToTile(Transform targetTile)
    {
        while (Vector3.Distance(transform.position, tiles[currentTileIndex].position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetTile.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetTile.position;
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

        }
        else if (tile.CompareTag("red_tile"))
        {
            /* Combat or Obstacle Tile? Could start a fight?*/
            Debug.Log("Landed on red tile. Initiate combat ");
        }
        else if (tile.CompareTag("green_tile"))
        {
            /* Event Tile (run some dialog and stuff happens?) */
            Debug.Log("Landed on green tile. Event ID:  ");
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

    //private IEnumerator HandleTrap(Transform tile)
    //{
    //    isMoving = true;

    //    TrapTile trap = tile.GetComponent<TrapTile>();
    //    if (trap != null)
    //    {
    //        Debug.Log("Landed on yellow tile. Trap ID: " + trap.trapID);
    //        yield return TrapIDManager.Instance.TriggerTrap(trap.trapID, this);
    //    }

    //    // end when trap is finished
    //    EndTileEffect();
    //}


    // Call this when the effect is fully done
    public void EndTileEffect()
    {
        isMoving = false;

    }

    public void TeleportToTile(int tileIndex)
    {
        if(tileIndex >= 0 && tileIndex < tiles.Length)
        {
            StopAllCoroutines();
            currentTileIndex = tileIndex;
            transform.position = tiles[currentTileIndex].position;
            StartCoroutine(MoveToTile(tiles[currentTileIndex]));
        }
    }

    public void MoveBackwards(int steps)
    {
        isMoving = true;

        Debug.Log((currentTileIndex + 1) + " / " + (tiles.Length));
        currentTileIndex -= steps;
        if (currentTileIndex < 0)
        {
            currentTileIndex = 0;
        }
        TeleportToTile(currentTileIndex);

        isMoving = false;
    }

}
