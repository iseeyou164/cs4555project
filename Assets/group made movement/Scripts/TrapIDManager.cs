using System.Collections;
using UnityEngine;

public class TrapIDManager : MonoBehaviour
{ 
    public GameObject boulderPrefab;
    public GameObject poisonIvyEffectPrefab;

    public static TrapIDManager Instance;
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

    public IEnumerator TriggerTrap(int trapID, BoardWalk player)
    {
        EventManager.IsEventRunning = true;
        /* gonna use a switch to determine which trap to activate */
        switch (trapID)
        {
            case 0:
                Debug.Log("Trap 0: Return to start!");
                yield return TeleportTrap(player, 0);
                break;

            case 1:
                Debug.Log("Trap 1: Move back 3 tiles");
                yield return MoveBackwards(player, 3);
                break;

            case 2:
                Debug.Log("Trap 2: Dodge the boulder!");
                yield return BoulderTrap(player);
                break;

            case 3:
                Debug.Log("Trap 3: Avoid being pricked by poison ivy!");
                yield return PoisonIvyTrap(player);
                break;

            case 4:
                Debug.Log("Trap 4: Dodge the arrow from the goblin watchtower!");
                yield return GoblinTower(player);
                break;

            //case 4: mushroom launch pad: launches them down the cliff

            default:
                Debug.Log("Trap ?");
                player.EndTileEffect();
                break;
        }
    }

    private IEnumerator TeleportTrap(BoardWalk player, int targetIndex)
    {
        yield return DialogManager.Instance.ShowMessageAndWait($"You see yourself being pulled in into a portal!");
        yield return new WaitForSeconds(0.25f);
        yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Roll 10+ to escape!");

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

        if (result >= 20)
        {
            SoundManager.Instance.Play("generic_dodge");
            SoundManager.Instance.Play("generic_glory");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Success!] You rolled {result}! You dodged successfully and gained a Glory Crystal!");
            player.GetComponent<PlayerData>().AddItem("Glory Warp");
            player.GetComponent<PlayerData>().experience += 10;
        }
        else if (result >= 17)
        {
            SoundManager.Instance.Play("generic_dodge");
            SoundManager.Instance.Play("generic_glory");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Greate Success!] You rolled {result}! You dodged successfully and gained a Warp Crystal!");
            player.GetComponent<PlayerData>().AddItem("Warp Crystal");
        }
        else if (result >= 10)
        {
            SoundManager.Instance.Play("generic_dodge");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You dodged successfully!");
        }
        else
        {
            SoundManager.Instance.Play("generic_error");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You failed to dodge and return to start!");
            yield return player.StartCoroutine(player.TeleportToTile(targetIndex));
            //player.TeleportToTile(targetIndex);

            //yield return new WaitForSeconds(0.25f);
            //yield return player.StartCoroutine(player.Land(player.tiles[targetIndex]));

        }
        Debug.Log("Trap Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

    private IEnumerator MoveBackwards(BoardWalk player, int steps)
    {
        yield return DialogManager.Instance.ShowMessageAndWait($"A trap triggers! You stumble backward {steps} spaces!");
        // move back tiles
        player.MoveBackwards(steps);
        yield return null; // optional small delay
        Debug.Log("Trap Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

    private IEnumerator BoulderTrap(BoardWalk player)
    {
        SoundManager.Instance.Play("generic_rockfall");
        yield return DialogManager.Instance.ShowMessageAndWait("You see a boulder falling on top of you!");
        yield return new WaitForSeconds(0.25f);
        yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Roll 10+ to dodge!");

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
        if(result >= 20)
        {
            SoundManager.Instance.Play("generic_heavyblow");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Success!] You rolled {result}! You destroyed the falling boulder and gained a Lucky Dice!");
            player.GetComponent<PlayerData>().AddItem("Lucky Dice");
            player.GetComponent<PlayerData>().experience += 10;
        }
        else if (result <= 1)
        {
            SoundManager.Instance.Play("choice_error");
            ParticleManager.Instance.Play("rock", transform.position);
            yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Failure!] You rolled {result}! The boulder fell onto you and dealt 10 damage!");
            int damage = 10;
            //Spawn the boulder visually above the player
            if (boulderPrefab != null)
            {
                GameObject boulder = Instantiate(boulderPrefab, player.transform.position + Vector3.up * 10f, Quaternion.identity);

                yield return new WaitForSeconds(1.5f);
                yield return DialogManager.Instance.ShowMessageAndWait($"The boulder crushed you! You take {damage} damage!");
                ParticleManager.Instance.Play("dust", player.transform.position);
                player.GetComponent<PlayerData>().gainHealth(-damage);

                Destroy(boulder, 0.5f); // cleanup
            }
            else
            {
                Debug.LogWarning("Boulder prefab not assigned!");
                player.GetComponent<PlayerData>().gainHealth(-damage);
            }
        }
        else if (result >= 10)
        {
            SoundManager.Instance.Play("generic_dodge");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You dodged successfully!");
        }
        else
        {
            int damage = 5;
            yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! A boulder falls on you!");

            //Spawn the boulder visually above the player
            if (boulderPrefab != null)
            {
                GameObject boulder = Instantiate(boulderPrefab, player.transform.position + Vector3.up * 10f, Quaternion.identity);

                yield return new WaitForSeconds(1.5f);
                yield return DialogManager.Instance.ShowMessageAndWait($"The boulder crushed you! You take {damage} damage!");
                ParticleManager.Instance.Play("dust", player.transform.position);
                player.GetComponent<PlayerData>().gainHealth(-damage);

                Destroy(boulder, 0.5f); // cleanup
            }
            else
            {
                Debug.LogWarning("Boulder prefab not assigned!");
                player.GetComponent<PlayerData>().gainHealth(-damage);
            }

        }
        Debug.Log("Trap Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

    private IEnumerator PoisonIvyTrap(BoardWalk player)
    {
        yield return DialogManager.Instance.ShowMessageAndWait("You attempt to walk through thick grass.");
        yield return new WaitForSeconds(0.25f);
        yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Roll 10+ to remain unscathed.");

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

        if (result >= 20)
        {
            SoundManager.Instance.Play("generic_gold");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Success!] You rolled {result}! You found {result} gold on the ground!");
            player.GetComponent<PlayerData>().AddGold(result);
            player.GetComponent<PlayerData>().experience += 10;
        }
        else if (result <= 1)
        {
            SoundManager.Instance.Play("choice_error");
            int damage = 3;
            yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Failure!] You rolled {result}! You take {damage} damage and are poisoned for {damage} turn(s)");

            if (poisonIvyEffectPrefab != null)
            {
                GameObject effect = Instantiate(poisonIvyEffectPrefab, player.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                yield return new WaitForSeconds(0.5f); // wait for particles to play
                Destroy(effect);

                player.GetComponent<PlayerData>().gainHealth(-damage);
                player.GetComponent<PlayerData>().ApplyEffect("Poison", damage);
            }
            else
            {
                Debug.LogWarning("Poison Ivy Effect prefab not assigned!");
                player.GetComponent<PlayerData>().gainHealth(-damage);
            }
        }
        else if (result >= 10)
        {
            yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You remain unscathed by the grass!");
        }
        else
        {
            SoundManager.Instance.Play("generic_poison");
            int damage = 3;
            yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You are poisoned for {damage} turn(s)!");

            if (poisonIvyEffectPrefab != null)
            {
                GameObject effect = Instantiate(poisonIvyEffectPrefab, player.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                yield return new WaitForSeconds(0.5f); // wait for particles to play
                Destroy(effect);
                player.GetComponent<PlayerData>().ApplyEffect("Poison", damage);
            }
            else
            {
                Debug.LogWarning("Poison Ivy Effect prefab not assigned!");
            }

        }
        Debug.Log("Trap Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

    private IEnumerator GoblinTower(BoardWalk player)
    {
        yield return new WaitForSeconds(0.25f);
        SoundManager.Instance.Play("generic_goblin");
        yield return DialogManager.Instance.ShowMessageAndWait("You see a goblin readying its bow against you!");
        yield return new WaitForSeconds(0.25f);
        yield return DialogManager.Instance.ShowMessageAndWait("Press SPACE to roll a d20. Roll 10+ to dodge!");

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

        if (result >= 20)
        {
            SoundManager.Instance.Play("generic_dodge");
            SoundManager.Instance.Play("generic_die");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Success!] You rolled {result}! You parried the arrow and it flew into the goblin, slaying it! Gain a Lucky Dice");
            player.GetComponent<PlayerData>().AddItem("Lucky Dice");
            player.GetComponent<PlayerData>().experience += 10;
        }
        else if (result <= 1)
        {
            SoundManager.Instance.Play("choice_error");
            ParticleManager.Instance.Play("wood", player.transform.position);
            yield return DialogManager.Instance.ShowMessageAndWait($"[Critical Failure!] You rolled {result}! You were fatally shot in a vital area and are slain!");
            player.GetComponent<PlayerData>().gainHealth(-20);
        }
        else if (1 < result && result <= 9)
        {
            SoundManager.Instance.Play("choice_error");
            ParticleManager.Instance.Play("wood", player.transform.position);
            yield return DialogManager.Instance.ShowMessageAndWait($"[Fail!] You rolled {result}! You barely escaped, but lost {11-result} health!");
            player.GetComponent<PlayerData>().gainHealth(-(11 - result));
        }
        else if (10 <= result && result <= 19)
        {
            SoundManager.Instance.Play("generic_dodge");
            yield return DialogManager.Instance.ShowMessageAndWait($"[Success!] You rolled {result}! You successfully dodged the arrow and fled!");
        }
        Debug.Log("Trap Finished");
        EventManager.IsEventRunning = false;
        player.EndTileEffect();
    }

}
