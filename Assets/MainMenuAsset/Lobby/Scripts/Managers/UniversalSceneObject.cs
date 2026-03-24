using UnityEngine;

public class UniversalSceneObject : MonoBehaviour
{
    public static UniversalSceneObject Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep the object across scenes
    }
}
