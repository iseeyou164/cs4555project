using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static TurnMenu;
using static UnityEditor.Rendering.CameraUI;

public class PlayerData : MonoBehaviour
{
    [Header("Stats")]
    public int gold = 0;
    public int glory = 0;
    public int maxHealth = 20;
    public int health = 20;

    [Header("Items")]
    public string[] items = new string[3];

    [Header("Gear")]
    // [0] = Weapon
    // [1] = Armor
    public string[] gear = new string[2];

    [Header("UI")]
    public TextMeshProUGUI statsText;

    [Header("Player Info")]
    public string playerName;

    [Header("Other Data")]
    public int minMoveRoll = 1;
    public int maxMoveRoll = 6;

    [Header("Temporary Effects")]
    public int moveBonus = 0;
    public int diceCount = 1;
    public bool usedItem = false;
    public int poisonDuration = 0;

    [Header("Sprites")]
    public TMP_SpriteAsset spriteAsset;


    void Start()
    {
        diceCount = 1;
        maxHealth = 20;
        health = 20;
        gold = 10;
        glory = 0;
        usedItem = false;
        items = new string[3];

        PlayerManager.Instance.RegisterPlayer(this);

        if (statsText == null)
        {
            statsText = GameObject.Find($"{playerName}_StatsText").GetComponent<TextMeshProUGUI>();
        }

        statsText.spriteAsset = spriteAsset;
        UpdateStatusUI();

        //PlayerManager.Instance.GetPlayer(0).AddGold(50); <- to give player 1 50 gold
    }

    private void Update()
    {
        UpdateStatusUI();
    }

    // Call this instead of AddGold() for animated increment
    public void AddGold(int amount)
    {
        StartCoroutine(AddGoldCoroutine(amount));
    }

    private IEnumerator AddGoldCoroutine(int amount)
    {
        int target = gold + amount;

        // Prevent gold from going below 0
        if (target < 0)
            target = 0;

        int step = amount > 0 ? 1 : -1;   // +1 when adding, -1 when subtracting

        // Animate until we hit target
        while (gold != target)
        {
            gold += step;

            UpdateStatusUI();
            yield return new WaitForSeconds(0.05f); // animation speed
        }

        Debug.Log($"{playerName} gold updated by {amount}. New total: {gold}");
    }

    public void AddGlory(int amount)
    {
        if (amount > 0)
        {
            for (int i = 0; i < amount; i++)
            {
                glory += 1;
            }

            // POP UP DIALOGUE
            Debug.Log($"Player gained {amount} glory. Total: {glory}");

        }
        else if (amount < 0)
        {
            for (int i = 0; i < amount; i++)
            {
                if (glory > 0)
                {
                    glory -= 1;
                }
            }

            // POP UP DIALOGUE
            Debug.Log($"Player lost {-amount} glory. Total: {glory}");
        }
        UpdateStatusUI();

    }

    //Maybe add Steal Glory?

    public bool SpendGold(int amount)
    {
        if (amount <= gold)
        {
            AddGold(-amount);

            // POP UP DIALOGUE
            Debug.Log($"Player spent {amount} gold. Remaining: {gold}");
            UpdateStatusUI();
            return true;

        } else {

            // POP UP DIALOGUE
            Debug.Log($"Player doesn't have {amount} gold. Remaining: {gold}");
            UpdateStatusUI();
            return false;
        }
    }

    public void gainHealth(int amount)
    {
        health += amount;
        Debug.Log($"Player lost {amount} HP. Remaining: {health}/{maxHealth} HP");
        if (health < 0)
        {
            //Teleport to start
            //Lose 50% gold
            int deduct = gold / 2;
            gold = gold - deduct;
            //clear misc effects
            poisonDuration = 0;
            Debug.Log($"Player lost all HP. Loses {deduct} gold. Remaining: {gold}");
            BoardWalk player = GetComponent<BoardWalk>();
            if (player != null)
            {
                player.TeleportToTile(2); // send to start tile
            }
            else
            {
                Debug.LogWarning("No BoardWalk component found on player!");
            }
            health = maxHealth;
            //UpdateStatusUI();
        }

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        UpdateStatusUI();
    }

    public void ApplyEffect(string effectName, int duration)
    {
        if (effectName == "Poison")
        {
            poisonDuration += duration;
        }
        else
        {
            Debug.Log("Null effect");
        }
            Debug.Log($"{playerName} is affected by {effectName} for {duration} turns.");
        UpdateStatusUI();
    }


    //Item

    public IEnumerator UseItem(string itemName)
    {
        bool dummy = false;
        if (itemName == "Pixie Dust")
        {
            moveBonus += 3;
            Debug.Log($"{playerName} used Pixie Dust! +3 to next dice roll.");
            dummy = true;
        }
        else if (itemName == "Double Dice")
        {
            diceCount = 2;
            Debug.Log($"{playerName} used Double Dice! +1 Dice this turn.");
            dummy = true;
        }
        else if (itemName == "Triple Dice")
        {
            diceCount = 3;
            Debug.Log($"{playerName} used Triple Dice! +2 Dice this turn.");
            dummy = true;
        }
        else if (itemName == "Potion")
        {
            yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d6. Heal based on result!");

            bool finished = false;
            int result = 0;

            // Roll a visual d20
            yield return DiceRoller.Instance.StartCoroutine(
                DiceRoller.Instance.RollDiceVisual(6, 1, (total) =>
                {
                    result = total;
                    finished = true;
                })
            );
            yield return new WaitUntil(() => finished);
            health += result;
            yield return DialogManager.Instance.ShowMessageAndWait($"You healed {result} health!");
            dummy = true;
        }
        else if (itemName == "Greater Potion")
        {
            yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Heal based on result!");

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
            health += result;
            yield return DialogManager.Instance.ShowMessageAndWait($"You healed {result} health!");
            dummy = true;
        }
        else if (itemName == "Lucky Dice")
        {
            //    yield return DialogManager.Instance.ShowAmountChoiceAndWait(
            //    "Gambler: Would you like to gamble your hard-earned gold? You can maybe double it!",
            //    "-1!",
            //    "+1!",
            //    1,
            //    10,
            //    (int value) =>
            //    {
            //        amount_to_gamble += value;
            //        player.GetComponent<PlayerData>().AddGold(-amount_to_gamble);
            //    }
            //);
            diceCount = -1;
            Debug.Log($"{playerName} used Custom Dice! They can choose their dice roll this turn.");
            dummy = true;
        }

            yield return new WaitUntil(() => dummy);
        //Remove 1 item from inventory with the same name.
        RemoveItemSpecific(itemName);
        usedItem = true;
        UpdateStatusUI();
    }

    public bool AddItem(string itemName)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (string.IsNullOrEmpty(items[i]))
            {
                items[i] = itemName;
                Debug.Log($"Picked up {itemName} in slot {i}");
                return true;
            }
        }
        UpdateStatusUI();
        Debug.Log("Inventory full!");
        return false;
    }

    public void RemoveItem(int slot)
    {
        if (slot >= 0 && slot < items.Length)
        {
            Debug.Log($"Removed {items[slot]} from slot {slot}");
            items[slot] = null;
            UpdateStatusUI();
        }
        UpdateStatusUI();
    }

    public void RemoveItemSpecific(string itemName)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == itemName)
            {
                Debug.Log($"Found {itemName} in slot {i}");
                RemoveItem(i);
                UpdateStatusUI();
                return; // stop after removing one
            }
        }

        Debug.Log($"{itemName} not found in inventory!");
        UpdateStatusUI();
    }

    public int ItemCount()
    {
        //count how many items are in list?
        int count = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                count += 1;
            }
        }
        return count;
    }

    public int RollDice(int sides, int rolls)
    {
        int total = 0;
        for (int i = 0; i < rolls; i++)
        {
            int roll = Random.Range(1, sides + 1);
            total += roll;
            Debug.Log($"{playerName} rolled a {roll} on a d{sides}");
        }
        return total;
    }

    //take items list and display appropriate sprites?

    public void UpdateStatusUI()
    {
        if (statsText == null)
        {
            Debug.LogWarning("statsText is null for " + playerName);
            return;
        }

        if (statsText != null)
        {

            string itemDisplay = " ";
            for (int i = 0; i < items.Length; i++)
            {
                switch (items[i])
                {
                    case "Pixie Dust":
                        itemDisplay += "<sprite name=\"pixie_dust\">";
                        break;
                    case "Double Dice":
                        itemDisplay += "<sprite name=\"double_dice\">";
                        break;
                    case "Triple Dice":
                        itemDisplay += "<sprite name=\"triple_dice\">";
                        break;
                    case "Potion":
                        itemDisplay += "<sprite name=\"potion\">";
                        break;
                    case "Greater Potion":
                        itemDisplay += "<sprite name=\"greater_potion\">";
                        break;
                    case "Lucky Dice":
                        itemDisplay += "<sprite name=\"lucky_dice\">";
                        break;
                    default:
                        itemDisplay += "";
                        break;
                }
                itemDisplay += "";

            }
            //itemDisplay = string.Join(" ", items);

            statsText.text = $"{playerName}\nHealth: {health}/{maxHealth}\nGold: {gold}\nGlory: {glory}\nItems: {itemDisplay}";
            statsText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            statsText.enabled = false;
            statsText.enabled = true;
            //Debug.Log($"Updating UI for {playerName}: Gold={gold}, Text={statsText.text}");
        } else
        {
            Debug.LogWarning("statsText is null for " + playerName);
        }
    }




}
