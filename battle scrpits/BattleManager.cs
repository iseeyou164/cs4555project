using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    [Header("UI")]
    public BattleMenu battleMenu; // Drag your Battle_UI_Canvas here

    // Private references to the fighters
    private GameObject playerInstance;
    private GameObject enemyInstance;
    private PlayerData playerScript;
    // You'll likely want an EnemyData script later for the enemy

    // In BattleManager.cs, in your Start() function:

    void Start()
    {
        // 1. Check if data exists
        if (BattleData.playerPrefab == null || BattleData.enemyPrefab == null)
        {
            Debug.LogError("BATTLE DATA NOT FOUND! Cannot start battle.");
            Debug.LogWarning("Did you start this scene directly? Make sure to load from the main board scene.");
            return;
        }

        // --- THIS IS THE MODIFIED SECTION ---

        // 2. We no longer "Instantiate" the player. We just get the reference.
        playerInstance = BattleData.playerPrefab;
        // And move it to the spawn point
        playerInstance.transform.position = playerSpawnPoint.position;
        playerInstance.transform.rotation = playerSpawnPoint.rotation;

        // We still Instantiate the new enemy
        enemyInstance = Instantiate(BattleData.enemyPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);

        // --- END OF MODIFIED SECTION ---

        // 3. Get the PlayerData script from the player
        playerScript = playerInstance.GetComponent<PlayerData>();
        if (playerScript == null)
        {
            Debug.LogError("SPAWNED PLAYER IS MISSING PlayerData SCRIPT!");
            return;
        }

        // 4. Find the BattleMenu (if not dragged in)
        if (battleMenu == null)
        {
            battleMenu = FindFirstObjectByType<BattleMenu>();
        }

        // 5. Activate the menu
        battleMenu.InitializeMenu(playerScript);

        Debug.Log($"Battle Started! {playerScript.playerName} vs. {enemyInstance.name}");
    }
}