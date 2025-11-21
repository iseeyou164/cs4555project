using System.Collections;
using TMPro;
//using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.ProBuilder.MeshOperations;
//using static TurnMenu;
//using static UnityEditor.Rendering.CameraUI;
//using static UnityEngine.GraphicsBuffer;

public class PlayerData : MonoBehaviour
{
    [Header("Stats")]
    public int gold = 0;
    public int glory = 0;
    public int maxHealth = 20;
    public int health = 20;
    public int level = 1;
    public int experience = 0;
    public int experience_cap = 10;

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
    private int score;
    private int placement;

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
        level = 1;
        diceCount = 1;
        maxHealth = 20;
        health = 20;
        gold = 10;
        glory = 0;
        score = 0;
        experience = 0;
        experience_cap = 15;
        usedItem = false;
        items = new string[3];

        //PlayerManager.Instance.RegisterPlayer(this);

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
        levelUp();
        UpdateStatusUI();
    }

    // Call this instead of AddGold() for animated increment
    public void AddGold(int amount)
    {
        StartCoroutine(AddGoldCoroutine(amount));
    }

    public void levelUp()
    {
        int leftover = 0;
        if (experience >= experience_cap)
        {
            leftover = experience - experience_cap;
            level += 1;
            maxHealth += 5;
            gainHealth(5);
            SoundManager.Instance.Play("generic_glory2");
            experience = leftover;
            experience_cap += 15;
        }
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
            if (amount > 0)
            {
                SoundManager.Instance.Play("generic_money");
            }
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
                SoundManager.Instance.Play("generic_glory2");
                glory += 1;
                experience += 15;
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

        //if (glory >= GameSettings.Instance.glory_to_win)
        //{
        //    //run gameend function in turn manager
        //    //TurnManager.Instance.EndGame(this);
        //    StartCoroutine(TurnManager.Instance.EndGame(this));
        //}

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
        if (amount < 0) ParticleManager.Instance.Play("blood", transform.position);

        if (amount <= -20)
        {
            SoundManager.Instance.Play("generic_die");
        }
        else if (amount <= -15)
        {
            SoundManager.Instance.Play("generic_slash");
        }
        else if (amount <= -10)
        {
            SoundManager.Instance.Play("generic_heavyblow");
        }
        else if (amount <= -5)
        {
            SoundManager.Instance.Play("generic_claw");
        }
        else if (amount <= -1)
        {
            SoundManager.Instance.Play("generic_bite");
        }
        else
        {
            SoundManager.Instance.Play("generic_heal");
        }

            health += amount;
        Debug.Log($"Player lost {amount} HP. Remaining: {health}/{maxHealth} HP");
        if (health <= 0)
        {
            //health = maxHealth;
            SoundManager.Instance.Play("generic_die");
            //UpdateStatusUI();
            StartCoroutine(Die());
        }

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        UpdateStatusUI();
    }

    public IEnumerator Die()
    {
        //Teleport to start
        //Lose 50% gold
        int deduct = gold / 2;
        gold = gold - deduct;
        if (gold < 0)
        {
            gold = 0;
        }
        //clear misc effects
        poisonDuration = 0;
        yield return DialogManager.Instance.ShowMessageAndWait($"Player lost all HP. Loses {deduct} gold. Remaining: {gold}");
        BoardWalk player = GetComponent<BoardWalk>();
        if (player != null)
        {
            yield return player.TeleportToTile(0); // send to start tile
        }
        else
        {
            Debug.LogWarning("No BoardWalk component found on player!");
        }
        health = maxHealth;
    }

    public int calculateScore()
    {
        score = (glory * 100000) + (gold * 100) + (health);
        return score;
    }

    public void ApplyEffect(string effectName, int duration)
    {
        if (effectName == "Poison")
        {
            SoundManager.Instance.Play("generic_poison");
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
        SoundManager.Instance.Play("generic_useitem");
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
            gainHealth(result);
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
            gainHealth(result);
            yield return DialogManager.Instance.ShowMessageAndWait($"You healed {result} health!");
            dummy = true;
        }
        else if (itemName == "Lucky Dice")
        {
            diceCount = -1;
            Debug.Log($"{playerName} used Custom Dice! They can choose their dice roll this turn.");
            dummy = true;
        }
        else if (itemName == "Warp Crystal")
        {
            //get this player's current tile index in board walk
            //then set this player's current tile index to the random target player's current tile index index
            //then set the random target player's current tile index to random_dummy 
            //then run teleport to file function for both players
            PlayerData random_target = this;
            while (random_target == this)
            {
                random_target = PlayerManager.Instance.GetPlayer(Random.Range(0, PlayerManager.Instance.players.Count));
            }

            BoardWalk myBW = GetComponent<BoardWalk>();
            BoardWalk targetBW = random_target.GetComponent<BoardWalk>();

            int myTile = myBW.currentTileIndex;
            int theirTile = targetBW.currentTileIndex;

            myBW.currentTileIndex = theirTile;
            targetBW.currentTileIndex = myTile;

            yield return StartCoroutine(myBW.TeleportToTile(theirTile));
            yield return StartCoroutine(targetBW.TeleportToTile(myTile));

            yield return DialogManager.Instance.ShowMessageAndWait($"{playerName} swapped with {random_target.playerName}!");

            Debug.Log($"{playerName} used Warp Crystal! They can swap places with a random player.");
            dummy = true;
        }
        else if (itemName == "Glory Warp")
        {
            BoardWalk myBW = GetComponent<BoardWalk>();
            int gloryTile = GloryManager.Instance.currentGloryTileIndex;
            myBW.currentTileIndex = gloryTile;
            yield return StartCoroutine(myBW.TeleportToTile(gloryTile-1));
            //set player's current tile index to the current tile index of current active glory tile
            // then run teleport to file function for this player
            yield return DialogManager.Instance.ShowMessageAndWait($"{playerName} teleported to the Glory Tile!");
            Debug.Log($"{playerName} used Glory Warp! They can teleport to the Glory tile.");
            dummy = true;
        }
        else if (itemName == "Landmine")
        {
            //do this if you want. not needed
            Debug.Log($"{playerName} used Landmine! They set up a trap.");
            dummy = true;
        }

        yield return new WaitUntil(() => dummy);
        //Remove 1 item from inventory with the same name.
        RemoveItemSpecific(itemName);
        usedItem = true;
        UpdateStatusUI();
    }

    public string getDescription(string itemName)
    {
        string desc = "";


        if (itemName == "Pixie Dust")
        {
            desc = "+3 to this turn's movement.";
        }
        else if (itemName == "Double Dice")
        {
            desc = "Use 2 dice for this turn's movement.";
        }
        else if (itemName == "Triple Dice")
        {
            desc = "Use 3 dice for this turn's movement.";
        }
        else if (itemName == "Potion")
        {
            desc = "Heal equal to d6 result.";
        }
        else if (itemName == "Greater Potion")
        {
            desc = "Heal equal to d20 result.";
        }
        else if (itemName == "Lucky Dice")
        {
            desc = "Manually choose 1-10 for this turn's movement.";
        }
        else if (itemName == "Warp Crystal")
        {
            desc = "Swaps position with a random player.";
        }
        else if (itemName == "Glory Warp")
        {
            desc = "Teleports to the Glory tile.";
        }
        else if (itemName == "Landmine")
        {
            desc = "Sets up a trap. Explodes player that passes/lands on it.";
        }
        return desc;
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
        //SoundManager.Instance.Play("generic_useitem");
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
        //SoundManager.Instance.Play("generic_useitem");
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
            if (items[i] != null || items[i] == "")
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

    public void ResetForNewGame()
    {
        Debug.LogWarning("Reset");
        // Basic stats
        gold = 10;
        glory = 0;
        health = 20;

        // Reset poison, shields, etc if you have them
        poisonDuration = 0;
        usedItem = false;

        // Clear inventory
        items[0] = null;
        items[1] = null;
        items[2] = null;

        // Move player to starting tile
        // BoardWalk holds the movement logic, so reset position there
        BoardWalk bw = GetComponent<BoardWalk>();
        StartCoroutine(bw.TeleportToTile(0));
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
                    case "Warp Crystal":
                        itemDisplay += "<sprite name=\"warp_crystal\">";
                        break;
                    case "Glory Warp":
                        itemDisplay += "<sprite name=\"glory_warp\">";
                        break;
                    case "Landmine":
                        itemDisplay += "<sprite name=\"landmine\">";
                        break;
                    default:
                        itemDisplay += "";
                        break;
                }
                itemDisplay += "";

            }
            //itemDisplay = string.Join(" ", items);

            statsText.text = $"{playerName} Lv{level} [{experience}/{experience_cap}]\nHealth: {health}/{maxHealth}\nGold: {gold}\nGlory: {glory}\nItems: {itemDisplay}";
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
