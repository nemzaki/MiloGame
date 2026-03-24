using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public AllPlayerData playerData;
    
    private void Awake()
    {
        Instance = this;
    }
    
}
