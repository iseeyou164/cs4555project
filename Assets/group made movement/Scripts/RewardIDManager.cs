using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class RewardIDManager : MonoBehaviour
{
    public GameObject rewardEffectPrefab;

    [Header("Chest Prefabs")]
    public GameObject chestClosedPrefab;
    public GameObject chestOpenPrefab;

    public static RewardIDManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator TriggerReward(int rewardID, BoardWalk player)
    {
        EventManager.IsEventRunning = true;
        /* gonna use a switch to determine which trap to activate */
        switch (rewardID)
        {
            case 0:
                Debug.Log("Reward 0: Basic Chest");
                yield return BasicChestReward(player);
                break;
            case 1:
                Debug.Log("Reward 1: Suspicious Mushroom");
                yield return SuspiciousMushroom(player);
                break;
            case 2:
                Debug.Log("Reward 2: The Gambler");
                //asks the player to gamble any amount of gold. Default is 0. Use A to increment & D to decrement. Use Space to select.
                yield return Gambler(player);
                break;
            case 3:
                Debug.Log("Reward 3: Goblin Treasure");
                yield return GoblinCamp(player);
                break;
            case 4:
                Debug.Log("Reward 4: Broken Cart");
                yield return BrokenCart(player);
                break;
            default:
                Debug.Log("Reward ?");
                player.EndTileEffect();
                break;
        }
    }

    private IEnumerator BasicChestReward(BoardWalk player)
    {
        //find nearest closed_chest object
        GameObject[] closedChests = GameObject.FindGameObjectsWithTag("chest_closed");
        GameObject nearestChest = null;
        float minDist = float.MaxValue;

        foreach (GameObject chest in closedChests)
        {
            float dist = Vector3.Distance(player.transform.position, chest.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestChest = chest;
            }
        }

        yield return DialogManager.Instance.ShowMessageAndWait($"You found a chest!");

        if (rewardEffectPrefab != null)
        {
            GameObject effect = Instantiate(rewardEffectPrefab, player.transform.position + Vector3.up * 1f, Quaternion.identity);
            Destroy(effect, 0.5f);
        }

        yield return new WaitForSeconds(0.25f);

        int choice = -1;

        yield return DialogManager.Instance.ShowBinaryChoiceAndWait(
            "Do you want to open the chest?",
            "Yes",
            "No",
            (bool choiceASelected) =>
            {
               if (choiceASelected)
                {
                    Debug.Log("Player chose Yes");
                    choice = 0;
                }
                else
                {
                    Debug.Log("Player chose No");
                    choice = 1;
                }
               }
        );

        yield return new WaitUntil(() => choice == 0 || choice == 1);

        if (choice == 0)
        {
            yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Rewards are based on result!");

            bool finished = false;
            int result = 0;

            // Roll a visual d20
            yield return DiceRoller.Instance.StartCoroutine(
                DiceRoller.Instance.RollDiceVisual(20, 1, (total) =>
                {
                    result = total;
                    finished = true;
                })
            );

            yield return new WaitUntil(() => finished);

            // Swap chest visuals
            nearestChest.SetActive(false);

            GameObject chestOpen = null;
            Transform parent = nearestChest.transform.parent;

            // Check if chest_open already exists as a child
            Transform openChild = parent ? parent.Find("chest_open") : null;
            if (openChild != null) chestOpen = openChild.gameObject;

            if (chestOpen == null && chestOpenPrefab != null)
            {
                chestOpen = Instantiate(chestOpenPrefab, nearestChest.transform.position, nearestChest.transform.rotation, parent);
                chestOpen.name = "chest_open";
            }

            if (chestOpen != null)
                chestOpen.SetActive(true);

            //give rewards
            if (result >= 20)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Success!] You rolled {result}! You gained 1 Glory!");
                player.GetComponent<PlayerData>().AddGlory(1);
            }
            else if (result <= 1)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Failure!] You rolled {result}! The chest exploded! You lose 10 health!");
                //play some explosion effect?
                player.GetComponent<PlayerData>().gainHealth(-10);
            }
            else if (1 < result && result <= 5)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You gained nothing...");
            }
            else if (5 < result && result <= 10)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You gained {result} gold!");
                player.GetComponent<PlayerData>().AddGold(result);
            }
            else
            {
                //11->19
                string randomItem = string.Empty;
                if (result >= 18)
                {
                    randomItem = "Triple Dice";
                }
                else if (result >= 15)
                {
                    randomItem = "Double Dice";
                }
                else if (result >= 13)
                {
                    randomItem = "Potion";
                }
                else
                {
                    randomItem = "Pixie Dust";
                }
                yield return DialogManager.Instance.ShowMessageAndWait($"[Success] You rolled {result}! You gained a {randomItem}!");
                player.GetComponent<PlayerData>().AddItem(randomItem);
            }

            yield return new WaitForSeconds(0.5f);

            // Reset chest
            if (chestOpen != null) chestOpen.SetActive(false);
            nearestChest.SetActive(true);
        }
        else
        {
            yield return DialogManager.Instance.ShowMessageAndWait("You decided to ignore the chest.");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Reward Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }


    private IEnumerator SuspiciousMushroom(BoardWalk player)
    {

        yield return DialogManager.Instance.ShowMessageAndWait($"You picked up a weird mushroom!");

        if (rewardEffectPrefab != null)
        {
            GameObject effect = Instantiate(rewardEffectPrefab, player.transform.position + Vector3.up * 1f, Quaternion.identity);
            Destroy(effect, 0.5f);
        }

        yield return new WaitForSeconds(0.25f);

        int choice = -1;

        yield return DialogManager.Instance.ShowBinaryChoiceAndWait(
            "Do you want to eat it or throw it away?",
            "Eat",
            "Discard",
            (bool choiceASelected) =>
            {
                if (choiceASelected)
                {
                    Debug.Log("Player chose Yes");
                    choice = 0;
                }
                else
                {
                    Debug.Log("Player chose No");
                    choice = 1;
                }
            }
        );

        yield return new WaitUntil(() => choice == 0 || choice == 1);

        if (choice == 0)
        {
            yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Effects are based on result!");

            bool finished = false;
            int result = 0;

            // Roll a visual d20
            yield return DiceRoller.Instance.StartCoroutine(
                DiceRoller.Instance.RollDiceVisual(20, 1, (total) =>
                {
                    result = total;
                    finished = true;
                })
            );

            yield return new WaitUntil(() => finished);

            //give rewards
            if (result >= 20)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Success!] You rolled {result}! You recover to full health!");
                player.GetComponent<PlayerData>().gainHealth(20);
            }
            else if (result <= 1)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Failure!] You rolled {result}! The mushroom contained lethal poison! You take 20 damage!");
                player.GetComponent<PlayerData>().gainHealth(-20);
            }
            else if (1 < result && result <= 5)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! The mushroom contained poison! You are poisoned for 3 turns!");
                player.GetComponent<PlayerData>().ApplyEffect("Poison", 3);
            }
            else if (5 < result && result <= 10)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Fail] You rolled {result}! Nothing happened...");
            }
            else
            {
                //11->19
                yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You recover {result / 2} health!");
                player.GetComponent<PlayerData>().gainHealth(result/2);
            }

            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return DialogManager.Instance.ShowMessageAndWait("You decided to discard the mushroom.");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Reward Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

    private IEnumerator Gambler(BoardWalk player)
    {

        yield return DialogManager.Instance.ShowMessageAndWait($"You meet the man who calls himself the Gambler.");

        if (rewardEffectPrefab != null)
        {
            GameObject effect = Instantiate(rewardEffectPrefab, player.transform.position + Vector3.up * 1f, Quaternion.identity);
            Destroy(effect, 0.5f);
        }

        yield return new WaitForSeconds(0.25f);

        int choice = -1;

        yield return DialogManager.Instance.ShowBinaryChoiceAndWait(
            "Gambler: Would you like to gamble your hard-earned gold? It'll be worth it!",
            "Accept",
            "Decline",
            (bool choiceASelected) =>
            {
                if (choiceASelected)
                {
                    if (player.GetComponent<PlayerData>().gold == 0)
                    {
                        choice = 1;
                    }
                    else
                    {
                        Debug.Log("Player chose Yes");
                        choice = 0;
                    }
                }
                else
                {
                    Debug.Log("Player chose No");
                    choice = 1;
                }
            }
        );

        yield return new WaitUntil(() => choice == 0 || choice == 1);

        int amount_to_gamble = 0;

        if (choice == 0)
        {
            //Have a number in dialog manager? [D] to increment value. [A] to decrement value. [Space] to confirm and move on. Value cannot be below 0.
            //yield return DialogManager.Instance.ShowMessageAndWait("Choose the amount of gold to gamble! Press [D] to decrement and [A] to increment. Press [Space] to confirm.");
            yield return DialogManager.Instance.ShowAmountChoiceAndWait(
            "Gambler: Would you like to gamble your hard-earned gold? You can maybe double it!",
            "-1!",
            "+1!",
            1,
            player.GetComponent<PlayerData>().gold,
            (int value) =>
            {
                amount_to_gamble += value;
                player.GetComponent<PlayerData>().AddGold(-amount_to_gamble);
            }
        );

            bool finished = false;
            //int result = 0;
            int player_result = 0;
            int gambler_result = 0;

            // Compare Dice Rolls with the gambler
            yield return StartCoroutine(DiceRoller.Instance.RollPlayerVsGambler((player, gambler) =>
            {
                player_result = player;
                gambler_result = gambler;
            }));
            finished = true;

            yield return new WaitUntil(() => finished);
            if (player_result > gambler_result)
            {
                //win
                yield return DialogManager.Instance.ShowMessageAndWait($"Gambler: I rolled {gambler_result}!\nYou rolled {player_result}! You win {amount_to_gamble * 2} gold!");
                player.GetComponent<PlayerData>().AddGold(amount_to_gamble * 2);
            }
            else if (player_result < gambler_result)
            {
                //lose
                yield return DialogManager.Instance.ShowMessageAndWait($"Gambler: I rolled {gambler_result}!\nYou rolled {player_result}! You lost your gold!");
            }
            else
            {
                //tie
                yield return DialogManager.Instance.ShowMessageAndWait($"Gambler: I rolled {gambler_result}!\nYou rolled {player_result}! We tie! Take back your gold!");
                player.GetComponent<PlayerData>().AddGold(amount_to_gamble);
            }

            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return DialogManager.Instance.ShowMessageAndWait("You decided to not gamble.");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Reward Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

    private IEnumerator GoblinCamp(BoardWalk player)
    {

        yield return DialogManager.Instance.ShowMessageAndWait($"You spot a horde of treasure in the goblin camp.");

        if (rewardEffectPrefab != null)
        {
            GameObject effect = Instantiate(rewardEffectPrefab, player.transform.position + Vector3.up * 1f, Quaternion.identity);
            Destroy(effect, 0.5f);
        }

        yield return new WaitForSeconds(0.25f);

        int choice = -1;

        yield return DialogManager.Instance.ShowBinaryChoiceAndWait(
            "Do you want to try stealing it?",
            "Steal",
            "Ignore",
            (bool choiceASelected) =>
            {
                if (choiceASelected)
                {
                    Debug.Log("Player chose Yes");
                    choice = 0;
                }
                else
                {
                    Debug.Log("Player chose No");
                    choice = 1;
                }
            }
        );

        yield return new WaitUntil(() => choice == 0 || choice == 1);

        if (choice == 0)
        {
            yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Outcomes are based on result!");

            bool finished = false;
            int result = 0;
            int goblin_loot = 0;

            // Roll a visual d20
            yield return DiceRoller.Instance.StartCoroutine(
                DiceRoller.Instance.RollDiceVisual(20, 1, (total) =>
                {
                    result = total;
                    finished = true;
                })
            );

            yield return new WaitUntil(() => finished);

            //give rewards
            if (result >= 20)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Success!] You rolled {result}! You stole all their treasure!");
                goblin_loot = 4;
            }
            else if (result <= 1)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Failure!] You rolled {result}! You were brutally slain in the process!");
                player.GetComponent<PlayerData>().gainHealth(-20);
            }
            else if (1 < result && result <= 5)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You barely escaped, but lost {11 - result} health!");
                player.GetComponent<PlayerData>().gainHealth(-(11-result));
            }
            else if (5 < result && result <= 10)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Minor Fail] You rolled {result}! You barely escaped with a few treasures, but lost {11 - result} health!");
                player.GetComponent<PlayerData>().gainHealth(-(11-result));
                goblin_loot = 1;
            }
            else if (11 < result && result <= 15)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You managed to steal a few treasures!");
                goblin_loot = 1;
            }
            else if (15 < result && result <= 19)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Great Success!] You rolled {result}! You managed to steal most of their treasures!");
                goblin_loot = 2;
            }

            yield return new WaitForSeconds(0.5f);

            //get rewards
            for(int i = 0; i < goblin_loot; i++)
            {
                int loot = Random.Range(1, 7);
                switch (loot)
                {
                    case 1:
                        yield return DialogManager.Instance.ShowMessageAndWait($"You got 5 gold!");
                        player.GetComponent<PlayerData>().AddGold(5);
                        break;
                    case 2:
                        yield return DialogManager.Instance.ShowMessageAndWait($"You got a Potion!");
                        player.GetComponent<PlayerData>().AddItem("Potion");
                        break;
                    case 3:
                        yield return DialogManager.Instance.ShowMessageAndWait($"You got a Double Dice!");
                        player.GetComponent<PlayerData>().AddItem("Double Dice");
                        break;
                    case 4:
                        yield return DialogManager.Instance.ShowMessageAndWait($"You got a Triple Dice!");
                        player.GetComponent<PlayerData>().AddItem("Triple Dice");
                        break;
                    case 5:
                        yield return DialogManager.Instance.ShowMessageAndWait($"You got 10 gold!");
                        player.GetComponent<PlayerData>().AddGold(10);
                        break;
                    case 6:
                        yield return DialogManager.Instance.ShowMessageAndWait($"You a Greater Potion!");
                        player.GetComponent<PlayerData>().AddItem("Greater Potion");
                        break;
                }
            }

        }
        else
        {
            yield return DialogManager.Instance.ShowMessageAndWait("You decided to ignore the camp and sneak away.");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Reward Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

    private IEnumerator BrokenCart(BoardWalk player)
    {

        yield return DialogManager.Instance.ShowMessageAndWait($"You spot a broken cart with a person searching for something.");
        yield return DialogManager.Instance.ShowMessageAndWait($"Troubled Man: My cart crashed earlier. Mind helping me search for my stuff? I'll give you something in return.");

        if (rewardEffectPrefab != null)
        {
            GameObject effect = Instantiate(rewardEffectPrefab, player.transform.position + Vector3.up * 1f, Quaternion.identity);
            Destroy(effect, 0.5f);
        }

        yield return new WaitForSeconds(0.25f);

        int choice = -1;

        yield return DialogManager.Instance.ShowBinaryChoiceAndWait(
            "Do you want to help him out?",
            "Accept",
            "Decline",
            (bool choiceASelected) =>
            {
                if (choiceASelected)
                {
                    Debug.Log("Player chose Yes");
                    choice = 0;
                }
                else
                {
                    Debug.Log("Player chose No");
                    choice = 1;
                }
            }
        );

        yield return new WaitUntil(() => choice == 0 || choice == 1);

        if (choice == 0)
        {
            yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Outcomes are based on result!");

            bool finished = false;
            int result = 0;

            // Roll a visual d20
            yield return DiceRoller.Instance.StartCoroutine(
                DiceRoller.Instance.RollDiceVisual(20, 1, (total) =>
                {
                    result = total;
                    finished = true;
                })
            );

            yield return new WaitUntil(() => finished);

            //give rewards
            if (result >= 20)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Success!] You rolled {result}! You helped the man who is a Noble! Gain 1 Glory!");
                player.GetComponent<PlayerData>().AddGlory(1);
            }
            else if (result <= 1)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Failure!] You rolled {result}! You were tricked by a bandit and lost 10 gold!");
                player.GetComponent<PlayerData>().AddGold(-10);
            }
            else if (1 < result && result <= 5)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You were tricked by a bandit and lost {6-result} health while escaping!");
                player.GetComponent<PlayerData>().gainHealth(-(6 - result));
            }
            else if (5 < result && result <= 10)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Minor Success] You rolled {result}! You helped the man who is a Merchant! Gain {result} gold!");
                player.GetComponent<PlayerData>().AddGold(result);
            }
            else if (11 < result && result <= 15)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You helped the man who is a Priest! Gain a Greater Potion!");
                player.GetComponent<PlayerData>().AddItem("Greater Potion");
            }
            else if (15 < result && result <= 19)
            {
                yield return DialogManager.Instance.ShowMessageAndWait($"[Great Success!] You rolled {result}! You helped the man who is an Adventurer! Gain a Triple Dice!");
                player.GetComponent<PlayerData>().AddItem("Triple Dice");
            }

            yield return new WaitForSeconds(0.5f);

        }
        else
        {
            yield return DialogManager.Instance.ShowMessageAndWait("You decided to decline and move on.");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Reward Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }



}
