using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "Scriptable Object/All Data")]
public class AllDataSO : ScriptableObject
{
    [Header("CHARACTERS")]
    public GameObject[] characters;
    
    
}
