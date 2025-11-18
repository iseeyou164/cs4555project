using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);   // <-- THIS IS ALL YOU NEED
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
