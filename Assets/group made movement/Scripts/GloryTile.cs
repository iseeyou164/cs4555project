using UnityEngine;

public class GloryTile : MonoBehaviour
{
    public int tile_id;
    public bool isActive = false; // Only one active at a time
    public int gloryCost = 20;

    [Header("Visuals")]
    public GameObject gloryChest; // assign the chest prefab in inspector
}
