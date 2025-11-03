using System.Collections;
using System.Collections.Generic;
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
            rb.AddForce(Random.onUnitSphere * rollForce, ForceMode.Impulse);
            rb.AddTorque(Random.onUnitSphere * torqueForce, ForceMode.Impulse);
            //issue to fix: gets result before dice is finished rolling.
            spawnedDice.Add(die.GetComponent<DiceFaceReader>());
        }

        // Wait until all dice have stopped
        bool allStopped = false;
        while (!allStopped)
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
            yield return null; // check again next frame
        }


        foreach (var die in spawnedDice)
        {
            total += die.FinalValue;
        }


        //for (int i = 0; i < rolls; i++)
        //{
        //    int value = Random.Range(1, sides + 1);
        //    total += value;
        //}
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
    }
}
