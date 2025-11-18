using UnityEngine;

public class Battle : MonoBehaviour
{
    public static Battle Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // keep BattleSystem alive across scene changes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
