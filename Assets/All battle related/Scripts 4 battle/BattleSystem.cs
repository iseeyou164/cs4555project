using UnityEngine;
using System.Collections;   
using System.Collections.Generic;   
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;




public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    public Transform playerBattleStation;
    public Transform enemyBattleStation;
    Unit playerUnit;
    Unit enemyUnit; 
    public  BattleState state;
    public TextMeshProUGUI dialogueText;
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;
    [Header("Player Action Menu")]
    public List<Button> actionButtons; // Assign your UI buttons here in the inspector
    private int currentSelectedButtonIndex = 0;

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(setupBattle());
    }

    IEnumerator setupBattle()
    {
        GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = playerGO.GetComponent<Unit>();
        GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
        enemyUnit = enemyGO.GetComponent<Unit>();

        dialogueText.text = "You are about to fight the " + enemyUnit.unitName + "\nwhat is your next move?";

        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();

    }

    void Update()
    {
        // Only allow menu navigation during the player's turn
        if (state != BattleState.PLAYERTURN)
            return;

        // --- Keyboard Navigation (W & S) ---
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentSelectedButtonIndex--;
            // Wrap around to the bottom if we go past the top
            if (currentSelectedButtonIndex < 0)
                currentSelectedButtonIndex = actionButtons.Count - 1;

            HighlightButton();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            currentSelectedButtonIndex++;
            // Wrap around to the top if we go past the bottom
            if (currentSelectedButtonIndex >= actionButtons.Count)
                currentSelectedButtonIndex = 0;

            HighlightButton();
        }

        // --- Keyboard Selection (Space) ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // "Click" the currently highlighted button
            actionButtons[currentSelectedButtonIndex].onClick.Invoke();
        }
    }

    void HighlightButton()
    {
        // Use Unity's Event System to set the currently selected UI element
        EventSystem.current.SetSelectedGameObject(null); // Clear previous selection
        EventSystem.current.SetSelectedGameObject(actionButtons[currentSelectedButtonIndex].gameObject);
    }

    IEnumerator PlayerAttack()
    {
        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        enemyUnit.TakeDamage(playerUnit.damage);

        // Player attacks enemy
        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
        enemyHUD.SetHP(enemyUnit.currentHealth);
        dialogueText.text = "The attack is successful!";
        yield return new WaitForSeconds(2f);
        if (isDead)
        {
            enemyUnit.PlayDeathAnimation();
            state = BattleState.WON;
            EndBattle();
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }
    IEnumerator PlayerItem()
    {
        playerUnit.item(20);
        playerHUD.SetHP(playerUnit.currentHealth);
        dialogueText.text = "You used a Health Potion!";
        yield return new WaitForSeconds(2f);
        state = BattleState.ENEMYTURN;  
        StartCoroutine(EnemyTurn());
    }
    void PlayerTurn()
    {
        dialogueText.text = "What will you do?:";

        // Select the first button by default
        currentSelectedButtonIndex = 0;
        HighlightButton();
    }

    public void OnAttack()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerAttack());
    }

    public void OnItem()
    {
        if (state == BattleState.PLAYERTURN)
        {
            dialogueText.text = "You have no items to use!";
        }

        StartCoroutine(PlayerItem());
    }
    IEnumerator EndBattle()
    {
        if (state == BattleState.WON)
        {
            dialogueText.text = "You won the battle!";
            SceneManager.LoadScene("BigIsland");
        }
        else if (state == BattleState.LOST)
        {
            dialogueText.text = "You were defeated.";
            SceneManager.LoadScene("BigIsland");

        }
        yield return new WaitForSeconds(5f);
        // Here you can add code to transition back to the main game scene
        SceneManager.LoadScene("BigIsland");
    }

    IEnumerator EnemyTurn()
    {
        dialogueText.text = enemyUnit.unitName + " is attacking!";
        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(2f);
        // Enemy attacks player
        bool isDead = playerUnit.TakeDamage(enemyUnit.damage);
        playerHUD.SetHP(playerUnit.currentHealth);
        yield return new WaitForSeconds(2f);
        if (isDead)
        {
            playerUnit.PlayDeathAnimation();
            state = BattleState.LOST;
            EndBattle();

        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    public void OnRetreat()
    {
        if (state != BattleState.PLAYERTURN)
            return;
        dialogueText.text = "You fled the battle!";
        StartCoroutine(EndBattle());
    }
}
