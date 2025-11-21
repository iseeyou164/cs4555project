using UnityEngine;

public class ShopTile : MonoBehaviour
{
    public int tile_id;
    public bool isActive = false; // Only one active at a time

    [Header("Items")]
    public string stockA;
    public int stockA_price;
    public string stockB;
    public int stockB_price;
    public string stockC;
    public int stockC_price;
    public string stockD;
    public int stockD_price;
}
