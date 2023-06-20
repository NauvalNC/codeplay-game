using UnityEngine;

public class LiveSingleton : MonoBehaviour
{
    private static LiveSingleton Instance;

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}