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

    // The most recently instantiated character GameObject — used by ShopUIController
    // to grab the Animator without risking getting a stale/pending-destroy instance.
    [HideInInspector] public GameObject lastSpawnedCharacter;

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
        ChangeCharacter(SaveDataLocal.Instance.currentPlayerIndex, SaveDataLocal.Instance.currentSkinIndex);
    }

    public void ChangeCharacter(int index, int skinIndex = 0)
    {
        if (character == null)
        {
            Debug.LogError("Character transform is null!");
            return;
        }

        if (_resourceManager == null)
        {
            Debug.LogWarning("[UpdateCharacterVisuals] ResourceManager not ready — skipping ChangeCharacter.");
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

        // SPAWN NEW CHARACTER — use skin prefab when available, fall back to base playerObj
        var playerItem = _resourceManager.playerData.player[index];
        var prefabToUse = playerItem.playerObj;

        if (playerItem.skins != null &&
            skinIndex > 0 &&
            skinIndex < playerItem.skins.Length &&
            playerItem.skins[skinIndex].skinPrefab != null)
        {
            prefabToUse = playerItem.skins[skinIndex].skinPrefab;
        }

        if (prefabToUse != null)
        {
            var playerBody = Instantiate(prefabToUse, character);
            playerBody.transform.localPosition = Vector3.zero;
            playerBody.transform.localRotation = Quaternion.identity;
            lastSpawnedCharacter = playerBody;
        }
        else
        {
            Debug.LogError("Player object at index " + index + " is null!");
        }
    }
    
}
