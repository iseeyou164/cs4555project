using UnityEngine;
using System.Collections;

public class SplitIDManager : MonoBehaviour
{
    public static SplitIDManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator ChooseSplit(BoardWalk player, SplitTile splitTile)
    {
        if (splitTile == null)
        {
            Debug.LogError("SplitTile is null in ChooseSplit!");
            yield break;
        }

        // One exit — no choice needed
        if (splitTile.optionAIndex == splitTile.optionBIndex)
        {
            Debug.Log($"SplitTile {splitTile.name} has only one path — going directly to {splitTile.optionAIndex}");
            player.currentTileIndex = splitTile.optionAIndex-1;
            yield break;
        }

        if (DialogManager.Instance == null)
        {
            Debug.LogError("DialogManager.Instance is missing!");
            yield break;
        }

        int chosenIndex = -1;

        // Ask player which direction to go
        yield return DialogManager.Instance.ShowBinaryChoiceAndWait(
            "Choose your path!",
            "Path A",
            "Path B",
            (bool choiceASelected) =>
            {
                chosenIndex = choiceASelected ? splitTile.optionAIndex : splitTile.optionBIndex;
            }
        );

        // Wait until choice is made
        yield return new WaitUntil(() => chosenIndex != -1);

        player.currentTileIndex = chosenIndex-1;
        
    }
}
