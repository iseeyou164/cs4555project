using System.Collections;
using UnityEngine;

public class GloryManager : MonoBehaviour
{
    public static GloryManager Instance;
    public GloryTile[] gloryTiles;
    private GameObject activeChest;
    public GameObject gloryParticlePrefab;
    private GameObject activeParticle;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Find the already active glory tile on game start
        foreach (var tile in gloryTiles)
        {
            if (tile.isActive)
            {
                UpdateParticlePosition(tile.transform);
                break;
            }
        }
    }

    void Start()
    {
        // Pick initial glory tile
        MoveGloryToNewTile();
    }

    void UpdateChestVisual()
    {
        if (activeChest != null) activeChest.SetActive(false);

        foreach (var g in gloryTiles)
        {
            if (g.isActive && g.gloryChest != null)
            {
                activeChest = g.gloryChest;
                activeChest.SetActive(true);
                break;
            }
        }
    }

    public void MoveGloryToNewTile()
    {
        int idx = Random.Range(0, gloryTiles.Length);
        foreach (var g in gloryTiles) g.isActive = false;

        gloryTiles[idx].isActive = true;
        UpdateParticlePosition(gloryTiles[idx].transform);
        UpdateChestVisual();
    }

    public IEnumerator HandleGloryPurchase(BoardWalk player, GloryTile tile, System.Action<bool> gloryChoice)
    {
        Debug.Log($"Glory 2!!!!!!!");
        bool getGlory = false;
        PlayerData playerdata = player.GetComponent<PlayerData>();
        Debug.Log($"{player.GetComponent<PlayerData>()}/{tile.gloryCost}");
        if (playerdata.gold >= tile.gloryCost)
        {
            //bool getGlory = false;
            yield return DialogManager.Instance.ShowBinaryChoiceAndWait(
                $"Buy 1 Glory for {tile.gloryCost} gold?",
                "Yes", "No",
                (bool yes) => { getGlory = yes; }
            );

            if (getGlory)
            {
                playerdata.AddGold(-tile.gloryCost);
                playerdata.AddGlory(1);
                tile.isActive = false;

                MoveGloryToNewTile();

                yield return DialogManager.Instance.ShowMessageAndWait(
                    $"{playerdata.playerName} gained 1 Glory! [{playerdata.glory}]"
                );
            }

        }
        else
        {
            yield return DialogManager.Instance.ShowMessageAndWait(
                    $"{playerdata.playerName} does not have enough gold to buy glory!"
                );
        }
            gloryChoice?.Invoke(getGlory);
    }

    public bool IsGloryTile(Transform tileTransform)
    {
        GloryTile glory = tileTransform.GetComponent<GloryTile>();
        return glory != null && glory.isActive;
    }

    void UpdateParticlePosition(Transform tile)
    {
        if (gloryParticlePrefab == null) return;

        if (activeParticle == null)
        {
            activeParticle = Instantiate(gloryParticlePrefab);
        }

        activeParticle.transform.position = tile.position;

        activeParticle.SetActive(true);
    }
}