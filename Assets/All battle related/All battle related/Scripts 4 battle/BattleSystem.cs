using UnityEngine;
using System.Collections;   
using System.Collections.Generic;   
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;




public enum BattleResult { WIN, LOSE, FLEE }

public class BattleSystem : MonoBehaviour
{

    public GameObject[] enemyPrefab;
    //public GameObject skeletonPrefab;
    //public GameObject turtlePrefab;
    //public GameObject orcPrefab;
    //public GameObject golemPrefab;
    public GameObject playerPrefab;
    //public GameObject enemyPrefab;

    public Transform playerBattleStation;
    public Transform enemyBattleStation;
    Unit playerUnit;
    Unit enemyUnit; 
    //public  BattleState state;
    //public TextMeshProUGUI dialogueText;
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;
    [Header("Player Action Menu")]
    //public List<Button> actionButtons; // Assign your UI buttons here in the inspector
    //private int currentSelectedButtonIndex = 0;
    public string enemy_name = "Skeleton";

    public static BattleSystem Instance;

    //void Start()
    //{
    //    state = BattleState.START;
    //    playerHUD.s = 1; enemyHUD.s = 0;
    //    StartCoroutine(setupBattle(enemy_name));
    //}
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Main battle entry point
    public IEnumerator StartBattle(PlayerData player, Transform enemyTileSpot, string enemyName, System.Action<BattleResult> callback)
    {
        // Cleanup old enemy if exists
        if (enemyUnit != null)
            Destroy(enemyUnit.gameObject);

        // Spawn enemy at the enemy spot
        enemyBattleStation = enemyTileSpot;
        GameObject enemyGO = Instantiate(GetEnemyPrefab(enemyName), enemyBattleStation.position, Quaternion.identity);
        enemyUnit = enemyGO.GetComponent<Unit>();

        // Make enemy face the player
        Vector3 lookDirection = player.transform.position - enemyGO.transform.position;
        lookDirection.y = 0; // keep enemy upright
        if (lookDirection != Vector3.zero) // avoid zero-length vector
            enemyGO.transform.rotation = Quaternion.LookRotation(lookDirection);

        yield return DialogManager.Instance.ShowMessageAndWait($"You are battling a {enemyUnit.unitName}!");
        yield return new WaitForSeconds(0.5f);

        bool battleOver = false;
        BattleResult result = BattleResult.LOSE;

        while (!battleOver)
        {
            yield return new WaitForSeconds(0.5f);
            // ---------------- Player Turn ----------------
            yield return DialogManager.Instance.ShowMessageAndWait($"Your turn!\nEnemy HP: {enemyUnit.currentHealth}/{enemyUnit.maxHealth}");
            yield return new WaitForSeconds(0.5f);

            string playerAction = "";
            yield return DialogManager.Instance.ShowBattleMenuAndWait("Attack", "Heal", "Retreat", (action) =>
            {
                playerAction = action;
            });

            if (playerAction == "Attack")
            {
                //player.PlayAttackAnimation();
                yield return new WaitForSeconds(1f);

                int dmg = 0;
                yield return DiceRoller.Instance.RollDiceVisual(6, player.level, (total) => dmg = total);
                if (dmg >= 20)
                {
                    SoundManager.Instance.Play("generic_heavyblow");
                }
                else if (dmg >= 10)
                {
                    SoundManager.Instance.Play("generic_slash");
                }
                else if (dmg >= 5)
                {
                    SoundManager.Instance.Play("generic_claw");
                }
                else
                {
                    SoundManager.Instance.Play("generic_bite");
                }
                ParticleManager.Instance.Play("dust", enemyBattleStation.position);

                bool enemyDead = enemyUnit.TakeDamage(dmg);
                if (enemyHUD != null) enemyHUD.SetHP(enemyUnit.currentHealth);

                yield return DialogManager.Instance.ShowMessageAndWait($"You attacked {enemyUnit.unitName} and dealt {dmg} damage!\nEnemy HP: {enemyUnit.currentHealth}/{enemyUnit.maxHealth}");

                if (enemyDead)
                {
                    ParticleManager.Instance.Play("explosion", enemyBattleStation.position);
                    enemyUnit.PlayDeathAnimation();
                    battleOver = true;
                    result = BattleResult.WIN;
                    break;
                }
            }
            else if (playerAction == "Heal")
            {
                int temp = 0;
                yield return DiceRoller.Instance.RollDiceVisual(6, 1, (total) => temp = total);
                SoundManager.Instance.Play("generic_heal");
                player.gainHealth(temp);
                yield return DialogManager.Instance.ShowMessageAndWait("You used a potion!");
                yield return new WaitForSeconds(0.5f);
            }
            else if (playerAction == "Retreat")
            {
                int roll = 0;
                yield return DiceRoller.Instance.RollDiceVisual(20, 1, (r) => roll = r);
                if (roll >= 10)
                {
                    yield return DialogManager.Instance.ShowMessageAndWait("You successfully escaped!");
                    battleOver = true;
                    result = BattleResult.FLEE;
                    yield return new WaitForSeconds(0.5f);
                    break;
                }
                else
                {
                    yield return DialogManager.Instance.ShowMessageAndWait("Retreat failed!");
                    yield return new WaitForSeconds(0.5f);
                }
            }

            // ---------------- Enemy Turn ----------------
            if (!battleOver)
            {
                yield return new WaitForSeconds(0.5f);
                yield return DialogManager.Instance.ShowMessageAndWait($"{enemyUnit.unitName}'s turn!\nEnemy HP: {enemyUnit.currentHealth}/{enemyUnit.maxHealth}");
                enemyUnit.PlayAttackAnimation();
                yield return new WaitForSeconds(1f);

                int dmg = 0;
                yield return DiceRoller.Instance.RollDiceVisual(6, enemyUnit.unitLevel, (total) => dmg = total);

                if (dmg >= 20)
                {
                    SoundManager.Instance.Play("generic_heavyblow");
                }
                else if (dmg >= 10)
                {
                    SoundManager.Instance.Play("generic_slash");
                }
                else if (dmg >= 5)
                {
                    SoundManager.Instance.Play("generic_claw");
                }
                else
                {
                    SoundManager.Instance.Play("generic_bite");
                }
                ParticleManager.Instance.Play("dust", player.transform.position);
                player.health -= dmg;
                yield return DialogManager.Instance.ShowMessageAndWait($"{enemyUnit.unitName} attacked for {dmg} damage!\nEnemy HP: {enemyUnit.currentHealth}/{enemyUnit.maxHealth}");

                if (player.health <= 0)
                {
                    ParticleManager.Instance.Play("explosion", player.transform.position);
                    //player.PlayDeathAnimation();
                    battleOver = true;
                    result = BattleResult.LOSE;
                }
            }

            yield return null;
        }

        yield return DialogManager.Instance.ShowMessageAndWait($"Battle ended: {result}");

        // Cleanup
        Destroy(enemyUnit.gameObject);

        // Return result to CombatManager
        callback?.Invoke(result);
    }

    private GameObject GetEnemyPrefab(string enemyName)
    {
        switch (enemyName)
        {
            case "Skeleton": return enemyPrefab[0];
            case "Turtle": return enemyPrefab[1];
            case "Orc": return enemyPrefab[2];
            case "Golem": return enemyPrefab[3];
            default: return enemyPrefab[0];
        }
    }






    //public IEnumerator setupBattle(string enemy_name)
    //{
    //    Debug.Log("set up battle");
    //    GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);
    //    playerUnit = playerGO.GetComponent<Unit>();

    //    GameObject prefabToSpawn = enemyPrefab[0];

    //    if (enemy_name == "Skeleton")
    //    {
    //        prefabToSpawn = enemyPrefab[0];
    //    }
    //    else if (enemy_name == "Turtle")
    //    {
    //        prefabToSpawn = enemyPrefab[1];
    //    }
    //    else if (enemy_name == "Orc")
    //    {
    //        prefabToSpawn = enemyPrefab[2];
    //    }
    //    else if (enemy_name == "Golem")
    //    {
    //        prefabToSpawn = enemyPrefab[3];
    //    }

    //    GameObject enemyGO = Instantiate(prefabToSpawn, enemyBattleStation);
    //    enemyUnit = enemyGO.GetComponent<Unit>();

    //    yield return DialogManager.Instance.ShowMessageAndWait($"You have started a battle against {enemyUnit.unitName}!");
    //    //dialogueText.text = "You are about to fight the " + enemyUnit.unitName + "\nwhat is your next move?";

    //    playerHUD.SetHUD(playerUnit);
    //    enemyHUD.SetHUD(enemyUnit);

    //    yield return new WaitForSeconds(1f);

    //    state = BattleState.PLAYERTURN;

    //    yield return Battle(playerUnit, enemyUnit, (bool result) =>
    //    {

    //    }
    //    );

    //}

    //public GameObject SearchEnemyByName(string enemy_name)
    //{
    //    GameObject selected_unit = null;
    //    for (int i = 0; i < enemyPrefab.Length; i++) { 
    //        if (enemyPrefab[i].name == enemy_name)
    //        {
    //            selected_unit = enemyPrefab[i];
    //        }
    //    }
    //    return selected_unit;
    //}

    //public IEnumerator Battle(Unit playerUnit, Unit enemyUnit, System.Action<bool> onChoiceMade)
    //{
    //    Debug.Log("battle");
    //    //game loop here
    //    bool isDead = false;
    //    while (enemyUnit.currentHealth > 0 && playerUnit.currentHealth > 0)
    //    {
    //        string player_action = "";

    //        if (playerUnit.currentHealth > 0)
    //        {
    //            //player starts
    //            yield return DialogManager.Instance.ShowMessageAndWait($"It's your turn!");
    //            yield return new WaitForSeconds(0.5f);
    //            yield return DialogManager.Instance.ShowBattleMenuAndWait(
    //                "Attack",
    //                "Items",
    //                "Retreat",
    //                (string action) =>
    //                {
    //                    player_action = action;
    //                }
    //                );

    //            if (player_action == "Attack")
    //            {
    //                yield return PlayerAttack();
    //                yield return new WaitForSeconds(0.5f);

    //            }
    //            else if (player_action == "Items")
    //            {
    //                //view items from playerData
    //                yield return PlayerItem();
    //                yield return new WaitForSeconds(0.5f);
    //            }
    //            else if (player_action == "Retreat")
    //            {
    //                yield return OnRetreat();
    //                if (state == BattleState.WON || state == BattleState.LOST)
    //                    break; // leaves battle
    //                yield return new WaitForSeconds(0.5f);
    //            }
    //            else
    //            {
    //                //what
    //            }
    //        }

    //        yield return new WaitForSeconds(1f);

    //        if (enemyUnit.currentHealth <= 0) break;

    //        yield return DialogManager.Instance.ShowMessageAndWait($"It's the enemy's turn!");
    //        if (enemyUnit.currentHealth > 0)
    //        {
    //            yield return EnemyTurn();
    //        }

    //        yield return new WaitForSeconds(1f);
    //    }


    //    onChoiceMade?.Invoke(true);
    //}



    //void Update()
    //{
    //    // Only allow menu navigation during the player's turn
    //    if (state != BattleState.PLAYERTURN)
    //        return;

    //    // --- Keyboard Navigation (W & S) ---
    //    if (Input.GetKeyDown(KeyCode.W))
    //    {
    //        currentSelectedButtonIndex--;
    //        // Wrap around to the bottom if we go past the top
    //        if (currentSelectedButtonIndex < 0)
    //            currentSelectedButtonIndex = actionButtons.Count - 1;

    //        HighlightButton();
    //    }
    //    else if (Input.GetKeyDown(KeyCode.S))
    //    {
    //        currentSelectedButtonIndex++;
    //        // Wrap around to the top if we go past the bottom
    //        if (currentSelectedButtonIndex >= actionButtons.Count)
    //            currentSelectedButtonIndex = 0;

    //        HighlightButton();
    //    }

    //    // --- Keyboard Selection (Space) ---
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        // "Click" the currently highlighted button
    //        actionButtons[currentSelectedButtonIndex].onClick.Invoke();
    //    }
    //}

    //void HighlightButton()
    //{
    //    // Use Unity's Event System to set the currently selected UI element
    //    EventSystem.current.SetSelectedGameObject(null); // Clear previous selection
    //    EventSystem.current.SetSelectedGameObject(actionButtons[currentSelectedButtonIndex].gameObject);
    //}

    //IEnumerator PlayerAttack()
    //{
    //    playerUnit.PlayAttackAnimation();
    //    yield return new WaitForSeconds(1f);

    //    bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
    //    enemyHUD.SetHP(enemyUnit.currentHealth);

    //    yield return DialogManager.Instance.ShowMessageAndWait("Your attack hits!");

    //    if (isDead)
    //    {
    //        enemyUnit.PlayDeathAnimation();
    //        state = BattleState.WON;
    //        yield return EndBattle();
    //    }
    //}
    //IEnumerator PlayerItem()
    //{
    //    // Temporary: always heal 20
    //    playerUnit.item(20);
    //    playerHUD.SetHP(playerUnit.currentHealth);

    //    yield return DialogManager.Instance.ShowMessageAndWait("You used a Health Potion!");
    //}
    //void PlayerTurn()
    //{
    //    dialogueText.text = "What will you do?:";

    //    // Select the first button by default
    //    currentSelectedButtonIndex = 0;
    //    HighlightButton();
    //}

    //public void OnAttack()
    //{
    //    if (state != BattleState.PLAYERTURN)
    //        return;

    //    StartCoroutine(PlayerAttack());
    //}

    //public IEnumerator OnItem()
    //{
    //    if (state == BattleState.PLAYERTURN)
    //    {
    //        yield return DialogManager.Instance.ShowMessageAndWait("You have no items!");
    //    }

    //    StartCoroutine(PlayerItem());
    //}
    //IEnumerator EndBattle()
    //{
    //    if (state == BattleState.WON)
    //    {
    //        yield return DialogManager.Instance.ShowMessageAndWait("You won the battle!");
    //        //SceneManager.LoadScene("BigIsland");
    //        SceneManager.UnloadSceneAsync("battle scene");
    //    }
    //    else if (state == BattleState.LOST)
    //    {
    //        yield return DialogManager.Instance.ShowMessageAndWait("You were defeated...");
    //        //SceneManager.LoadScene("BigIsland");
    //        SceneManager.UnloadSceneAsync("battle scene");

    //    }
    //    yield return new WaitForSeconds(5f);
    //    // Here you can add code to transition back to the main game scene
    //    //SceneManager.LoadScene("BigIsland");
    //    SceneManager.UnloadSceneAsync("battle scene");
    //}

    //IEnumerator EnemyTurn()
    //{
    //    yield return DialogManager.Instance.ShowMessageAndWait($"{enemyUnit.unitName} attacks!");

    //    enemyUnit.PlayAttackAnimation();
    //    yield return new WaitForSeconds(1.2f);

    //    bool isDead = playerUnit.TakeDamage(enemyUnit.damage);
    //    playerHUD.SetHP(playerUnit.currentHealth);

    //    if (isDead)
    //    {
    //        playerUnit.PlayDeathAnimation();
    //        state = BattleState.LOST;
    //        yield return EndBattle();
    //    }
    //}

    //public IEnumerator OnRetreat()
    //{
    //    yield return DialogManager.Instance.ShowMessageAndWait("You attempt to retreat!");

    //    int roll = 0;
    //    yield return DiceRoller.Instance.RollDiceVisual(20, 1, result => roll = result);

    //    if (roll >= 10)
    //    {
    //        yield return DialogManager.Instance.ShowMessageAndWait($"You escaped!");
    //        state = BattleState.FLEE;
    //        yield return EndBattle();
    //    }
    //    else
    //    {
    //        yield return DialogManager.Instance.ShowMessageAndWait($"Failed to escape!");
    //    }
    //}
}
