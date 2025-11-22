using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Rendering.Analytics;
using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public static DiceRoller Instance;

    [Header("Dice Prefabs")]
    [SerializeField] private List<GameObject> dicePrefabs;

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float rollForce = 3f;
    public float torqueForce = 3f;
    //public float rollDuration = 2f; // how long dice roll before reading result
    public float stopThreshold = 0.01f; // how slow dice must get to be considered stopped
    public float checkInterval = 0.5f;  // how often to check if dice stopped
    public float timeout = 5f; // 5 seconds timeout
    private FocusCamera focusCam;

    private void Awake()
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

    public IEnumerator RollDiceVisual(int sides, int rolls, System.Action<int> onResult)
    {
        focusCam = FindFirstObjectByType<FocusCamera>();
        SoundManager.Instance.Play("generic_clank");
        //get prefab for dice
        GameObject diceData = dicePrefabs.Find(d => d.name.ToLower().Contains($"d{sides}"));
        if (diceData == null)
        {
            Debug.LogError($"No dice prefab found for d{sides}! Please assign it in the Inspector.");
            yield break;
        }

        List<DiceFaceReader> spawnedDice = new List<DiceFaceReader>();
        int total = 0;

        //spawn and roll dice (visually)
        for (int i = 0; i < rolls; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 0.3f; // to prevent overlapping
            GameObject die = Instantiate(diceData, spawnPoint.position + offset, Random.rotation);

            Rigidbody rb = die.GetComponent<Rigidbody>();

            focusCam.SetTarget(rb.transform);
            var a = GameObject.Find("PointOfInterest").GetComponent<PointOfInterest>();
            a.SetTarget(rb.transform);

            rb.AddForce(Random.onUnitSphere * rollForce, ForceMode.Impulse);
            rb.AddTorque(Random.onUnitSphere * torqueForce, ForceMode.Impulse);
            //issue to fix: gets result before dice is finished rolling.
            spawnedDice.Add(die.GetComponent<DiceFaceReader>());
        }

        // Wait until all dice have stopped (expires in X seconds)
        float timer = 0f;
        bool allStopped = false;
        //while (!allStopped)
        //{
        //    allStopped = true;
        //    foreach (var die in spawnedDice)
        //    {
        //        if (!die.HasStopped)
        //        {
        //            allStopped = false;
        //            break;
        //        }
        //    }
        //    yield return null; // check again next frame
        //}
        while (!allStopped && timer < timeout)
        {
            allStopped = true;
            foreach (var die in spawnedDice)
            {
                if (!die.HasStopped)
                {
                    allStopped = false;
                    break;
                }
            }
            timer += Time.deltaTime;
            yield return null; // check again next frame
        }

        if (!allStopped)
        {
            Debug.LogWarning("Dice timeout reached! Forcing result...");
            // Force a random result for dice that didn’t stop
            foreach (var die in spawnedDice)
            {
                if (die.HasStopped)
                {
                    total += die.FinalValue;
                }
                else
                {
                    total += Random.Range(1, sides + 1);
                    Destroy(die.gameObject);
                }
                allStopped = true;
            }
        }
        else
        {
            foreach (var die in spawnedDice)
            {
                total += die.FinalValue;
            }
        }

        if (TurnManager.Instance.GetCurrentPlayer().moveBonus!=0)
        {
            DialogManager.Instance.ShowTop($"Round {TurnManager.Instance.current_round} / {GameSettings.Instance.round_limit}: {TurnManager.Instance.GetCurrentPlayer().playerName}'s turn.\n" +
            $"Roll: {total} + {TurnManager.Instance.GetCurrentPlayer().moveBonus}\n");
            total += TurnManager.Instance.GetCurrentPlayer().moveBonus;
        }
        else
        {
            DialogManager.Instance.ShowTop($"Round {TurnManager.Instance.current_round} / {GameSettings.Instance.round_limit}: {TurnManager.Instance.GetCurrentPlayer().playerName}'s turn.\n" +
            $"Roll: {total}\n");
        }
            Debug.Log($"Total roll result with (d{sides} x{rolls}): {total}");
        onResult?.Invoke(total);

        // clean up after a delay
        yield return new WaitForSeconds(1f);
        foreach (var die in spawnedDice)
        {
            if (die != null)
                Destroy(die.gameObject);
        }
        DiceResult.Instance.ShowResult(total);
        focusCam.SetTarget(PlayerManager.Instance.GetPlayer(TurnManager.Instance.currentPlayerIndex).transform);
        var poi = GameObject.Find("PointOfInterest").GetComponent<PointOfInterest>();
        poi.SetTarget(PlayerManager.Instance.GetPlayer(TurnManager.Instance.currentPlayerIndex).transform);

    }

    // Roll player vs gambler
    public IEnumerator RollPlayerVsGambler(System.Action<int, int> onComplete)
    {
        int playerResult = -1;
        int gamblerResult = -1;

        //player dice
        yield return StartCoroutine(RollDiceVisual(6, 1, (value) =>
        {
            playerResult = value;
            Debug.Log("Player rolled: " + value);
        }));

        yield return new WaitForSeconds(1f);

        //gambler dice
        yield return StartCoroutine(RollDiceVisual(7, 1, (value) =>
        {
            gamblerResult = value;
            Debug.Log("Gambler rolled: " + value);
        }));

        // Return results
        onComplete?.Invoke(playerResult, gamblerResult);
    }

    public IEnumerator RollDamage(int damageStat, System.Action<int> onComplete)
    {
        int guaranteed_bonus = 0;
        int number_of_6dice = 1;
        int number_of_20dice = 0;
        int total_damage = 0;

        //calculates dice damage (25)
        while (damageStat > 0)
        {
            if(damageStat >= 10)
            {
                number_of_20dice++;
                damageStat -= 10;
            }
            else if (damageStat >= 3)
            {
                number_of_6dice++;
                damageStat -= 3;
            }
            else
            {
                guaranteed_bonus += 1;
                damageStat -= 1;
            }
        }

        if (number_of_20dice > 0)
        {
            yield return StartCoroutine(RollDiceVisual(20, number_of_20dice, (value) =>
            {
                total_damage += value;
                Debug.Log("Player rolled: " + value);
            }));
        }
        if(number_of_6dice > 0)
        {
            yield return StartCoroutine(RollDiceVisual(6, number_of_6dice, (value) =>
            {
                total_damage += value;
                Debug.Log("Player rolled: " + value);
            }));
        }
        total_damage += guaranteed_bonus;

        //for(int i = 0;  i < number_of_20dice; i++)
        //{
        //    yield return StartCoroutine(RollDiceVisual(20, 1, (value) =>
        //    {
        //        total_damage += value;
        //        Debug.Log("Player rolled: " + value);
        //    }));
        //}

        //for(int i = 0; i < number_of_6dice; i++)
        //{
        //    yield return StartCoroutine(RollDiceVisual(6, 1, (value) =>
        //    {
        //        total_damage += value;
        //        Debug.Log("Player rolled: " + value);
        //    }));
        //}

        //25 Attack Stat = 2d20 + 1d6 + 2 total damage
        //25 - 10(2) - 3(1) - 2 = 0
        onComplete?.Invoke(total_damage);
    }




}
