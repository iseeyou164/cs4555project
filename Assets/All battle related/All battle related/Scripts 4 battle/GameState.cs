using System.Collections.Generic;

public static class GameState
{
    public static Dictionary<string, int> playerTileIndices = new Dictionary<string, int>();

    public static int currentPlayerIndex = 0;

    public static bool returningFromBattle = false;

    public static string enemyToSpawn = "Skeleton";
}