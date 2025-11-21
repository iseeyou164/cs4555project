using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardWalk : MonoBehaviour
{
    private Transform[] tiles;
    public float moveSpeed = 5f;

    /*For text display*/
    public TextMeshProUGUI stepLabel = null;

    public int currentTileIndex = 0;
    public bool isMoving = false;
    void Start()
    {
        if (GameSettings.Instance != null)
        {
            tiles = GameSettings.Instance.tiles;
        }
        else
        {
            Debug.LogError("GameSettings.Instance is NULL! Make sure GameSettings is in the scene.");
            return;
        }
        if (GameState.playerTileIndices.ContainsKey(gameObject.name))
        {
            currentTileIndex = GameState.playerTileIndices[gameObject.name];
            transform.position = tiles[currentTileIndex].position;
        }
        else
        {
            GameState.playerTileIndices.Add(gameObject.name, currentTileIndex);
        }
    }
    private void Awake()
    {
        tiles = GameSettings.Instance.tiles;
    }

    public IEnumerator MoveSteps(int steps)
    {
        isMoving = true;
        UpdateStepLabel(steps);

        while (steps > 0)
        {
            BoardWalk currentPlayer = TurnManager.Instance.CurrentPlayer;
            Transform currentTile = tiles[currentTileIndex];
            SplitTile split = currentTile.GetComponent<SplitTile>();
            ShopTile shop = currentTile.GetComponent<ShopTile>();
            GloryTile glory = currentTile.GetComponent<GloryTile>();

            if (split == null)
            {
                currentTileIndex++;
            }

            if(shop != null)
            {
                bool finishShop = false;
                Debug.Log($"shop!!!!!!!!");
                SoundManager.Instance.Play("generic_ping");
                yield return DialogManager.Instance.ShowMessageAndWait($"{currentPlayer.name} is browsing the shop!");
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(DialogManager.Instance.ShowItemShopAndWait(shop.stockA, shop.stockA_price, shop.stockB, shop.stockB_price
                    , shop.stockC, shop.stockC_price, shop.stockD, shop.stockD_price, (bool shoppy)=>
                    {
                        if (shoppy)
                        {
                            finishShop = true;
                        }
                        else
                        {
                            finishShop = true;
                        }
                    }
                    ));
                yield return new WaitUntil(() => finishShop = true);
                yield return new WaitForSeconds(0.25f);
            }

            if (glory != null && glory.isActive)
            {
                bool finishGlory = false;
                Debug.Log($"glory!!!!!!!!");
                yield return StartCoroutine(GloryManager.Instance.HandleGloryPurchase(this, glory,
                    (bool getGlory) =>
                    {
                        if (getGlory)
                        {
                            finishGlory = true;
                        }
                        else
                        {
                            finishGlory = true;
                        }
                    }));
                yield return new WaitUntil(() => finishGlory = true);
                // Optionally, glory.isActive = false if it moves to another tile after purchase
            }

            if (split != null && glory == null)
            {
                yield return StartCoroutine(SplitIDManager.Instance.ChooseSplit(this, split));
                yield return new WaitForSeconds(0.1f);
                //steps--;
            }
            //else
            //{
            //    currentTileIndex++;
            //    yield return new WaitForSeconds(0.1f);
            //}
            SoundManager.Instance.Play("generic_move");
            steps--;
            currentPlayer.GetComponent<PlayerData>().experience+=1;

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
        Vector3 midPos = (startPos + endPos) / 2f + Vector3.up * 3.0f;

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
        if (tile.TryGetComponent<GloryTile>(out GloryTile gloryTile))
        {
            if (gloryTile.isActive)
            {
                SoundManager.Instance.Play("generic_ping");
                Debug.Log("Landed on a Glory Tile!");

                bool finished = false;
                yield return GloryManager.Instance.HandleGloryPurchase(
                    this,
                    gloryTile,
                    (bool getGlory) => { finished = true; }
                );
                yield return new WaitUntil(() => finished);
            }
            else
            {
                SoundManager.Instance.Play("choice_move");
                /* Generic Tile? Maybe give gold when landed on (when that's added?) */
                BoardWalk currentPlayer = TurnManager.Instance.CurrentPlayer;
                yield return DialogManager.Instance.ShowMessageAndWait($"{currentPlayer.name} gained 3 gold!");
                currentPlayer.GetComponent<PlayerData>().AddGold(3);
                //currentPlayer.GetComponent<PlayerData>().UpdateStatusUI();
                Debug.Log($"{currentPlayer.GetComponent<PlayerData>().playerName} landed on blue tile +3 Gold!");
                EventManager.IsEventRunning = false;
            }
        }
        else if (tile.CompareTag("blue_tile"))
        {
            SoundManager.Instance.Play("choice_move");
            /* Generic Tile? Maybe give gold when landed on (when that's added?) */
            BoardWalk currentPlayer = TurnManager.Instance.CurrentPlayer;
            yield return DialogManager.Instance.ShowMessageAndWait($"{currentPlayer.name} gained 3 gold!");
            currentPlayer.GetComponent<PlayerData>().AddGold(3);
            //currentPlayer.GetComponent<PlayerData>().UpdateStatusUI();
            Debug.Log($"{currentPlayer.GetComponent<PlayerData>().playerName} landed on blue tile +3 Gold!");
            EventManager.IsEventRunning = false;

        }
        else if (tile.CompareTag("red_tile"))
        {
            SoundManager.Instance.Play("choice_error");

            CombatTile combat = tile.GetComponent<CombatTile>();

            if (combat == null)
            {
                Debug.LogError($"Red tile '{tile.name}' has no CombatTile component attached!");
                yield break;
            }

            Debug.Log("Landed on red tile. Combat ID: " + combat.combatID);
            yield return CombatManager.Instance.TriggerCombat(combat.combatID, combat.enemy_spot, this);
            //Debug.Log("Landed on red tile. Initiate combat ");

            //if (currentTileIndex < 8) GameState.enemyToSpawn = "Skeleton";
            //else if (currentTileIndex >= 8 && currentTileIndex < 20) GameState.enemyToSpawn = "Turtle";
            //else if (currentTileIndex >= 21 && currentTileIndex <= 50) GameState.enemyToSpawn = "Orc";
            //else if (currentTileIndex > 50) GameState.enemyToSpawn = "Golem";
            //foreach (var player in TurnManager.Instance.players)
            //{
            //    if (GameState.playerTileIndices.ContainsKey(player.name))
            //    {
            //        GameState.playerTileIndices[player.name] = player.currentTileIndex;
            //    }
            //    else
            //    {
            //        GameState.playerTileIndices.Add(player.name, player.currentTileIndex);
            //    }
            //}
            //GameState.currentPlayerIndex = TurnManager.Instance.currentPlayerIndex;
            //GameState.returningFromBattle = true;

            //SceneManager.LoadScene("battle scene");
        }
        else if (tile.CompareTag("green_tile"))
        {
            SoundManager.Instance.Play("generic_ping");
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
            SoundManager.Instance.Play("generic_ping2");
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
        SoundManager.Instance.Play("generic_teleport");
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
    //public void ResetPositionToStart()
    //{
    //    // Immediately teleport with no animation
    //    transform.position = GameSettings.Instance.tiles[0].position;

    //    // Reset the internal index (so movement works correctly)
    //    currentTileIndex = 0;

    //    // If you have a moving flag, clear it
    //    isMoving = false;
    //}


}
