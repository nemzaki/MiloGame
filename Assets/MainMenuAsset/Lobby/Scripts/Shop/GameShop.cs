using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

public class GameShop : MonoBehaviour
{
    public static GameShop Instance{get; private set;}
    
    private AllPlayerData _playerData;
    private AllFightShopData _fightData;
    
    public TextMeshProUGUI cashText;
    
    [Header("Camera")] 
    public GameObject normalCamera;
    public GameObject shopCamera;
    
    [Header("Screens")] 
    public GameObject mainScreen;
    public GameObject playerSelectScreen;
    public GameObject playerCustomizationScreen;
    
    [Header("Panels")] 
    public Color normalColor;
    public Color selectedColor;
    public GameObject[] panels;
    public TextMeshProUGUI[] activePanelText;
    
    [Header("STATE")] 
    public bool selectingPlayer;
    public bool selectingHat;
    public bool selectingHardPunch;
    public bool selectingHardKick;
    public bool selectingCelebrate;
    
    [Header("CHARACTERS")] 
    public int currentCharacterIndex;
    public int currentHatIndex;

    [Header("SHOP")] 
    public GameObject selectionButtons;
    public GameObject buyButton;
    public GameObject selectButton;
    public TextMeshProUGUI itemCostText;

    [Header("Animations")] 
    public string currentAnimationClip;
    
    [Header("Animation Clips")] 
    public Animator anim;

    public string[] introClipName;
    public string[] hardPunchClipName;
    public string[] hardKickClipName;
    public string[] celebrateClipName;
    
    [Header("Selection Buttons")] 
    public GameObject openPortalButton;
    public GameObject openShopButton;
    
    public Image[] fightStyleButtons;
    public Image[] introButtons;
    public Image[] hardPunchButtons;
    public Image[] hardKickButtons;
    public Image[] celebrateButtons;
    
    public int _currentHardPunchIndex;
    public int _currentHardKickIndex;
    public int _currentCelebrateIndex;

    [Header("Animations")]
    public float duration = 0.2f;
    public Ease easeType = Ease.OutBack;

    [Header("Scene")] 
    public GameObject controls;
    public CinemachineOrbitalFollow cameraOrbit;
    public MenuPlayerMove player;
    public GameObject house;
    public Transform normalPos;
    public Transform shopPos;

    [Header("TV Screen")] 
    public GameObject[] screens;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _playerData = ResourceManager.Instance.playerData;
        _fightData = AllFightShopData.Instance;
    }

    private void Update()
    {
        cashText.text = SaveDataLocal.Instance.cash.ToString();
        CheckAnimationState();
        
        ChangeScreen(currentCharacterIndex);
    }

    private void ChangeScreen(int index)
    {
        for(var i=0;i<screens.Length;i++)
        {
            screens[i].SetActive(index == i);
        }    
    }
    
    private void UpdateActiveButtons()
    {
        var normalScale = new Vector3(1, 1, 1);
        var selectedScale = new Vector3(1.2f, 1.2f, 1.2f);
   

        // Fight Style
        for (var i = 0; i < fightStyleButtons.Length; i++)
        {
            var targetScale = i == SaveDataLocal.Instance.currentIdleType ? selectedScale : normalScale;
            fightStyleButtons[i].transform.DOScale(targetScale, duration).SetEase(easeType);
        }
        
        // Hard Punch
        for (var i = 0; i < hardPunchButtons.Length; i++)
        {
            var targetScale = i == SaveDataLocal.Instance.currentHardPunchType ? selectedScale : normalScale;
            hardPunchButtons[i].transform.DOScale(targetScale, duration).SetEase(easeType);
        }

        // Hard Kick
        for (var i = 0; i < hardKickButtons.Length; i++)
        {
            var targetScale = i == SaveDataLocal.Instance.currentHardKickType ? selectedScale : normalScale;
            hardKickButtons[i].transform.DOScale(targetScale, duration).SetEase(easeType);
        }
        
        //Celebrate
        for (var i = 0; i < celebrateButtons.Length; i++)
        {
            var targetScale = i == SaveDataLocal.Instance.currentCelebrationType ? selectedScale : normalScale;
            celebrateButtons[i].transform.DOScale(targetScale, duration).SetEase(easeType);
        }
    }
    
    private void HideAllScreens()
    {
        mainScreen.SetActive(false);
        playerSelectScreen.SetActive(false);
        playerCustomizationScreen.SetActive(false);
    }

    private void HideAllSelections()
    {
        selectingPlayer = false;
        selectingHat = false;
        selectingHardPunch = false;
        selectingHardKick = false;
        selectingCelebrate = false;
    }
    
    public void ChangePanel(int index)
    {
        for (var i = 0; i < panels.Length; i++)
        {
            if (index == i)
            {
                panels[i].SetActive(true);
                activePanelText[i].color = selectedColor;
            }
            else
            {
                panels[i].SetActive(false);
                activePanelText[i].color = normalColor;
            }
        }
    }
    
    public void OpenMainScreen()
    {
        HideAllScreens();
        mainScreen.SetActive(true);
        
        selectionButtons.SetActive(false);
        
        cameraOrbit.HorizontalAxis.Value = 180;
    }
    
    public void OnOpenPlayerSelectScreen()
    {
        HideAllScreens();
        playerSelectScreen.SetActive(true);
        
        HideAllSelections();
        selectingPlayer = true;
        
        selectionButtons.SetActive(true);
    }

    public void OnOpenFightStyleScreen()
    {
        HideAllScreens();
        playerCustomizationScreen.SetActive(true);
        ChangePanel(0);
        
        UpdateActiveButtons();
        
        _currentHardPunchIndex = SaveDataLocal.Instance.currentHardPunchType;
        _currentHardKickIndex = SaveDataLocal.Instance.currentHardKickType;
        _currentCelebrateIndex = SaveDataLocal.Instance.currentCelebrationType;
    }
    
    public void OpenShop()
    {
        currentCharacterIndex = SaveDataLocal.Instance.currentPlayerIndex;
        currentHatIndex = SaveDataLocal.Instance.currentHatIndex;
        
        normalCamera.SetActive(false);
        shopCamera.SetActive(true);
        
        house.SetActive(false);

        //Move the player
        controls.SetActive(false);
        cameraOrbit.HorizontalAxis.Value = 180;
        var rb = player.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.MovePosition(normalPos.position);
        rb.MoveRotation(normalPos.rotation);
    }
    
    public void CloseShop()
    {
        SaveDataLocal.Instance.LoadGame();
        SavePlayerDataLocal.Instance.UpdateLoadData();
        _fightData.LoadData();
        
        UpdateCharacterVisuals.Instance.UpdateVisuals();
        
        normalCamera.SetActive(true);
        shopCamera.SetActive(false);
        
        house.SetActive(true);
        
        //Move the player
        controls.SetActive(true);
        var rb = player.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.MovePosition(shopPos.position);
        rb.MoveRotation(shopPos.rotation);
    }

    public void ResetData()
    {
        SaveDataLocal.Instance.LoadGame();  
        _fightData.LoadData();
    }
    
    //
    public void ChangeFightStyle(int index)
    {
        SaveDataLocal.Instance.currentIdleType = index;
        SaveDataLocal.Instance.SaveGame();
        
        UpdateActiveButtons();
        selectionButtons.SetActive(false);
    }
    

    public void ChangeCelebrateType(int index)
    {
        SaveDataLocal.Instance.currentCelebrationType = index;
        
        anim.Play(celebrateClipName[index]);
        UpdateActiveButtons();
        
        _currentCelebrateIndex = index;
        
        HideAllSelections();
        selectingCelebrate = true;
        CheckEnableSelect();
        selectionButtons.SetActive(true);
    }
    
    public void ChangeHardPunchType(int index)
    {
        SaveDataLocal.Instance.currentHardPunchType = index;
        
        anim.SetBool("IsAttacking", true);
        anim.Play(hardPunchClipName[index]);
        currentAnimationClip = hardPunchClipName[index];
        
        UpdateActiveButtons();
        
        _currentHardPunchIndex = index;
        
        HideAllSelections();
        selectingHardPunch = true;
        CheckEnableSelect();
        
        selectionButtons.SetActive(true);
    }

    public void ChangeHardKickType(int index)
    {
        SaveDataLocal.Instance.currentHardKickType = index;
        
        anim.SetBool("IsAttacking", true);
        anim.Play(hardKickClipName[index]);
        currentAnimationClip = hardKickClipName[index];
        
        UpdateActiveButtons();
        
        _currentHardKickIndex = index;
        
        HideAllSelections();
        selectingHardKick = true;
        CheckEnableSelect();
        
        selectionButtons.SetActive(true);
    }

    private void CheckAnimationState()
    {
        if(!anim)
            return;
        
        //Check attack animations
        var state = anim.GetCurrentAnimatorStateInfo(2);
        if (state.IsName(currentAnimationClip) && state.normalizedTime >= 1)
        {
            anim.SetBool("IsAttacking", false);
        }
    }
    #region ChangePlayer
    public void OnNextCharacter()
    {
        if (currentCharacterIndex < ResourceManager.Instance.playerData.player.Length - 1)
        {
            currentCharacterIndex += 1;
            SaveDataLocal.Instance.currentPlayerIndex = currentCharacterIndex;
            UpdateCharacterVisuals.Instance.UpdateVisuals();
            CheckEnableSelect();
        }
    }

    public void OnPreviousCharacter()
    {
        if (currentCharacterIndex >= 1)
        {
            currentCharacterIndex -= 1;
            SaveDataLocal.Instance.currentPlayerIndex = currentCharacterIndex;
            UpdateCharacterVisuals.Instance.UpdateVisuals();
            CheckEnableSelect();
        }
    }
    #endregion

    #region ChangeHat
    public void OnNextHat()
    {
        if (currentHatIndex < ResourceManager.Instance.playerData.player[currentCharacterIndex].hats.Length - 1)
        {
            currentHatIndex += 1;
            SaveDataLocal.Instance.currentHatIndex = currentHatIndex;
            UpdateCharacterVisuals.Instance.UpdateVisuals();
            CheckEnableSelect();
        }
    }

    public void OnPreviousHat()
    {
        if (currentHatIndex >= 1)
        {
            currentHatIndex -= 1;
            SaveDataLocal.Instance.currentHatIndex = currentHatIndex;
            UpdateCharacterVisuals.Instance.UpdateVisuals();
            CheckEnableSelect();
        }
    }
    
    #endregion
    
    private void CheckEnableSelect()
    {
        if (selectingPlayer)
        {
            selectButton.SetActive(_playerData.player[currentCharacterIndex].status == "owned");
            buyButton.SetActive(_playerData.player[currentCharacterIndex].status == "buy");
            itemCostText.text = ResourceManager.Instance.playerData.player[currentCharacterIndex].cost.ToString();
        }
        else if(selectingHat)
        {
            selectButton.SetActive(_playerData.player[currentCharacterIndex].hats[currentHatIndex].status == "owned");
            buyButton.SetActive(_playerData.player[currentCharacterIndex].hats[currentHatIndex].status == "buy");
            itemCostText.text = ResourceManager.Instance.playerData.player[currentCharacterIndex].hats[currentHatIndex].cost.ToString();
        }
        else if(selectingHardPunch)
        {
            selectButton.SetActive(_fightData.hardPunchItems[_currentHardPunchIndex].currentStatus == "owned");
            buyButton.SetActive(_fightData.hardPunchItems[_currentHardPunchIndex].currentStatus == "buy");
            itemCostText.text = _fightData.hardPunchItems[_currentHardPunchIndex].itemCost.ToString();
        }
        else if(selectingHardKick)
        {
            selectButton.SetActive(_fightData.hardKickItems[_currentHardKickIndex].currentStatus == "owned");
            buyButton.SetActive(_fightData.hardKickItems[_currentHardKickIndex].currentStatus == "buy");
            itemCostText.text = _fightData.hardKickItems[_currentHardKickIndex].itemCost.ToString();
        }
        else if(selectingCelebrate)
        {
             selectButton.SetActive(_fightData.celebrateItems[_currentCelebrateIndex].currentStatus == "owned");
             buyButton.SetActive(_fightData.celebrateItems[_currentCelebrateIndex].currentStatus == "buy");
             itemCostText.text = _fightData.celebrateItems[_currentCelebrateIndex].itemCost.ToString();
        }
    }

    public void OnSelectItem()
    {
        if (selectingPlayer)
        {
            SaveDataLocal.Instance.currentPlayerIndex = currentCharacterIndex;
            
            SaveDataLocal.Instance.currentMovementType =
                ResourceManager.Instance.playerData.player[currentCharacterIndex].playerMovementType;
        }
        else if (selectingHat)
        {
            SaveDataLocal.Instance.currentHatIndex = currentHatIndex;
        }
        else if (selectingHardPunch)
        {
            SaveDataLocal.Instance.currentHardPunchType = _currentHardPunchIndex;
        }
        else if(selectingHardKick)
        {
            SaveDataLocal.Instance.currentHardKickType = _currentHardKickIndex;
        }
        else if(selectingCelebrate)
        {
            SaveDataLocal.Instance.currentCelebrationType = _currentCelebrateIndex;
        }
        
        SaveDataLocal.Instance.SaveGame();
        _fightData.SaveData();
    }
        
    public void OnBuyItem()
    {
        if (selectingPlayer)
        {
            if (_playerData.player[currentCharacterIndex].cost <= SaveDataLocal.Instance.cash)
            { 
                _playerData.player[currentCharacterIndex].status = "owned";
               SaveDataLocal.Instance.cash -= _playerData.player[currentCharacterIndex].cost;
               SaveDataLocal.Instance.SaveGame();
               
               buyButton.SetActive(false);
               selectButton.SetActive(true);
            }
            else
            {
                //IAP Panel
            }
        }
        else if(selectingHat)
        {
            if (_playerData.player[currentCharacterIndex].hats[currentHatIndex].cost <= SaveDataLocal.Instance.cash)
            {
                _playerData.player[currentCharacterIndex].hats[currentHatIndex].status = "owned";
                SaveDataLocal.Instance.cash -= _playerData.player[currentCharacterIndex].hats[currentHatIndex].cost;
                SaveDataLocal.Instance.SaveGame();
                
                buyButton.SetActive(false);
                selectButton.SetActive(true);
            }
            else
            {
                //IAP Panel
            }
        }
        else if(selectingHardPunch)
        {
            if (_fightData.hardPunchItems[_currentHardPunchIndex].itemCost <= SaveDataLocal.Instance.cash)
            {
                _fightData.hardPunchItems[_currentHardPunchIndex].currentStatus = "owned";
                SaveDataLocal.Instance.cash -= _fightData.hardPunchItems[_currentHardPunchIndex].itemCost;
                SaveDataLocal.Instance.SaveGame();
                
                _fightData.SaveData();
                
                buyButton.SetActive(false);
                selectButton.SetActive(true);
            }
            else
            {
                //IAP Panel
            }
        }
        else if(selectingHardKick)
        {
            if (_fightData.hardKickItems[_currentHardKickIndex].itemCost <= SaveDataLocal.Instance.cash)
            {
                _fightData.hardKickItems[_currentHardKickIndex].currentStatus = "owned";
                SaveDataLocal.Instance.cash -= _fightData.hardKickItems[_currentHardKickIndex].itemCost;
                SaveDataLocal.Instance.SaveGame();
                
                _fightData.SaveData();
                
                buyButton.SetActive(false);
                selectButton.SetActive(true);
            }
            else
            {
                //IAP Panel
            }
        }
        else if (selectingCelebrate)
        {
            if (_fightData.celebrateItems[_currentCelebrateIndex].itemCost <= SaveDataLocal.Instance.cash)
            {
                _fightData.celebrateItems[_currentCelebrateIndex].currentStatus = "owned";
                SaveDataLocal.Instance.cash -= _fightData.celebrateItems[_currentCelebrateIndex].itemCost;
                SaveDataLocal.Instance.SaveGame();
                
                _fightData.SaveData();
                
                buyButton.SetActive(false);
                selectButton.SetActive(true);
            }
            else
            {
                //IAP Panel
            }
        }
        
    }
}
















