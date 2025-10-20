using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public List<PlayerData> players = new List<PlayerData>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterPlayer(PlayerData player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
        }
    }

    public PlayerData GetPlayer(int index)
    {
        if (index >= 0 && index < players.Count)
            return players[index];
        return null;
    }
}
