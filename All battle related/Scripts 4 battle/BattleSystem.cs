using UnityEngine;
using System.Collections;   
using System.Collections.Generic;   
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;




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

    IEnumerator PlayerAttack()
    {

        enemyUnit.TakeDamage(playerUnit.damage);

        // Player attacks enemy
        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
        enemyHUD.SetHP(enemyUnit.currentHealth);
        dialogueText.text = "The attack is successful!";
        yield return new WaitForSeconds(2f);
        if (isDead)
        {
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
        dialogueText.text = "Choose an action:";
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
        yield return new WaitForSeconds(2f);
        // Enemy attacks player
        bool isDead = playerUnit.TakeDamage(enemyUnit.damage);
        playerHUD.SetHP(playerUnit.currentHealth);
        yield return new WaitForSeconds(2f);
        if (isDead)
        {
            state = BattleState.LOST;
            EndBattle();

        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }
}
