using UnityEngine;
using TMPro;
using System.Collections;

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


    void Start()
    {
        diceCount = 1;
        maxHealth = 20;
        health = 20;
        gold = 10;
        glory = 0;
        usedItem = false;
        items = new string[3];

        if (statsText == null)
        {
            statsText = GameObject.Find($"{playerName}_StatsText").GetComponent<TextMeshProUGUI>();
        }
        PlayerManager.Instance.RegisterPlayer(this);
        UpdateStatusUI();

        //PlayerManager.Instance.GetPlayer(0).AddGold(50); <- to give player 1 50 gold
    }

    // Call this instead of AddGold() for animated increment
    public void AddGold(int amount)
    {
        StartCoroutine(AddGoldCoroutine(amount));
    }

    private IEnumerator AddGoldCoroutine(int amount)
    {
        int increment = 1;
        if (amount < 0)
        {
            increment = -1;
        }

        for (int i = 0; i < amount; i++)
        {
            gold += increment;
            if (gold < 0) gold = 0;  // prevent negative gold

            UpdateStatusUI();

            yield return new WaitForSeconds(0.25f);  // wait 0.1 seconds between each increment
        }
        Debug.Log($"{playerName} gains {amount} gold. gold: {gold}");
        //UpdateStatusUI();
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

    }

    //Maybe add Steal Glory?

    public bool SpendGold(int amount)
    {
        if (amount <= gold)
        {
            AddGold(-amount);

            // POP UP DIALOGUE
            Debug.Log($"Player spent {amount} gold. Remaining: {gold}");

            return true;

        } else {

            // POP UP DIALOGUE
            Debug.Log($"Player doesn't have {amount} gold. Remaining: {gold}");

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
    }


    //Item

    public void UseItem(string itemName)
    {
        if (itemName == "Pixie Dust")
        {
            moveBonus += 3;
            Debug.Log($"{playerName} used Pixie Dust! +3 to next dice roll.");
        }
        else if (itemName == "Double Dice")
        {
            diceCount = 2;
            Debug.Log($"{playerName} used Double Dice! +1 Dice this turn.");
        }
        else if (itemName == "Triple Dice")
        {
            diceCount = 3;
            Debug.Log($"{playerName} used Triple Dice! +2 Dice this turn.");
        }


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

        Debug.Log("Inventory full!");
        return false;
    }

    public void RemoveItem(int slot)
    {
        if (slot >= 0 && slot < items.Length)
        {
            Debug.Log($"Removed {items[slot]} from slot {slot}");
            items[slot] = null;
        }
    }

    public void RemoveItemSpecific(string itemName)
    {
        int i = 0;

        while (i < items.Length)
        {
            if (items[i] == itemName)
            {
                Debug.Log($"Found {itemName} from slot {i}");
                RemoveItem(i);
                i = items.Length;
            }
        }
    }

    public int RollDice(int sides, int rolls)
    {
        int total = 0;
        for (int i = 0; i < rolls; i++) ;
        {
            int roll = Random.Range(1, sides + 1);
            total += roll;
            Debug.Log($"{playerName} rolled a {roll} on a d{sides}");
        }
        return total;
    }

    public void UpdateStatusUI()
    {
        if (statsText != null)
        {
            string itemDisplay = string.Join(" ", items);
            statsText.text = $"{playerName}\nHealth: {health}/{maxHealth}\nGold: {gold}\nGlory: {glory}\nItems: {string.Join(" ", items)}";
            statsText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            statsText.enabled = false;
            statsText.enabled = true;
            Debug.Log($"Updating UI for {playerName}: Gold={gold}, Text={statsText.text}");
        } else
        {
            Debug.LogWarning("statsText is null for " + playerName);
        }
    }


    //Work on Gear Next time!




}
