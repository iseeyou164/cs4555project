using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleScene : MonoBehaviour
{
    public Transform playerSpawn;
    public Transform enemySpawn;

    void Start()
    {
        
        if (BattleData.playerPrefab = null)
            Instantiate(BattleData.playerPrefab, playerSpawn.position, Quaternion.identity);

        if (BattleData.enemyPrefab = null)
            Instantiate(BattleData.enemyPrefab, enemySpawn.position, Quaternion.identity);

        
        SceneManager.LoadScene("BigIsland");
    }
}