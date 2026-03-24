using System;
using System.Collections;
using UnityEngine;

public class UpdateCharacterVisuals : MonoBehaviour
{

    public static UpdateCharacterVisuals Instance;
    
    private ResourceManager _resourceManager;
    [Header("State")] 
    public bool inMenu;
    public bool inGame;
    
    [Header("Player")]
    public Transform character;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _resourceManager = ResourceManager.Instance;

        if (inMenu)
        {
            UpdateVisuals();
        }
    }
    
    public void UpdateVisuals()
    {
        ChangeCharacter(SaveDataLocal.Instance.currentPlayerIndex);
    }
    
    public void ChangeCharacter(int index)
    {
        if (character == null)
        {
            Debug.LogError("Character transform is null!");
            return;
        }

        // CLEAR CHILD
        foreach (Transform child in character)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        // SPAWN NEW CHARACTER
        if (_resourceManager.playerData.player[index].playerObj != null)
        {
            var playerBody = Instantiate(_resourceManager.playerData.player[index].playerObj, character);
            playerBody.transform.localPosition = Vector3.zero;
            playerBody.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("Player object at index " + index + " is null!");
        }
    }
    
}
