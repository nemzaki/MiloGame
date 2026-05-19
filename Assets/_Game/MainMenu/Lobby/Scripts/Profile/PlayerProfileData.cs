using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerProfileData : MonoBehaviour
{
    [Header("Player details")]
    [SerializeField] TMP_InputField playerNameInputField;
    [SerializeField] private TextMeshProUGUI playerNameHolder;
    public GameObject updateNamePanel;
    
    [Header("Info")] 
    public Button updateInfoButton;
    public Button getCloudDataButton;
    
    [Header("UI")] 
    public GameObject deleteProfileButton;

    [Header("State")]
    public bool canUpdateName;
    
    private void OnEnable()
    {
        canUpdateName = true;
    }

    private void Awake()
    {
        updateInfoButton.onClick.AddListener(UpdateInfo);
        getCloudDataButton.onClick.AddListener(LoadCloudData);
        
        playerNameInputField.onValueChanged.AddListener(OnTextChanged);
    }

    void Update()
    {
        CheckCanDelete();

        if (updateNamePanel.activeSelf)
        {
            playerNameHolder.text = SaveDataLocal.Instance.playerName;
        }
    }
    
    private void OnTextChanged(string text)
    {
        playerNameInputField.text = text.ToLower();
    }
    
    #region UpdatePlayerProfile
    public void OpenUpdatePlayerNamePanel()
    {
        updateNamePanel.SetActive(true);
    }

    public void CloseUpdatePlayerNamePanel()
    {
        updateNamePanel.SetActive(false);    
    }
    
    #endregion
    
    void CheckCanDelete()
    {
        if(AuthManager.Instance.debugMode)
            return;
        
        deleteProfileButton.SetActive(SaveDataLocal.Instance.guessLogin != "guess");
    }

    private async void UpdateInfo()
    {
        await UpdateUserName();
    }
    
    public async Task UpdateUserName()
    {
        if (!canUpdateName)
        {
            MainMenuUIHandler.Instance.Error("NOTICE", "TOO MUCH REQUEST TO UPDATE NAME, TRY AGAIN LATER");
            return;
        }
        
        if(!NameVerification(playerNameInputField.text))
            return;

        try
        {
            MainMenuUIHandler.Instance.OpenActionStatus("UPDATING INFO");

            if (!AuthManager.Instance.debugMode)
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(playerNameInputField.text);
            }

            await Task.Delay(2000);
            //Panels
            SignUp.Instance.mainSettingPanel.SetActive(true);
            CloseUpdatePlayerNamePanel();
            
            MainMenuUIHandler.Instance.CloseActionStatus("DATA UPDATED", 2);
            
            //UPDATE NAME
            if (!AuthManager.Instance.debugMode)
            {
                SaveDataLocal.Instance.playerName = AuthenticationService.Instance.PlayerName;
            }
            else
            {
                SaveDataLocal.Instance.playerName = playerNameInputField.text;
                SaveDataLocal.Instance.SaveGame();
            }

            MainMenuUIHandler.Instance.GetPlayerName(SaveDataLocal.Instance.playerName);

            canUpdateName = false;
        }
        catch (AuthenticationException ex)
        {
            MainMenuUIHandler.Instance.CloseActionStatus("ERROR", 2);
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(),ex.Message);
        }
        catch (RequestFailedException ex)
        {
            MainMenuUIHandler.Instance.CloseActionStatus("ERROR", 2);
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(),ex.Message);
        }
    }
    
    private bool NameVerification(string playerName)
    {
        switch (playerName.Length)
        {
            case < 3:
                MainMenuUIHandler.Instance.Error("Error", "NAME TOO SHORT MINIMUM LENGTH IS 3");
                return false;
            case > 12:
                MainMenuUIHandler.Instance.Error("Error", "NAME TOO LONG MAXIMUM LENGTH IS 12");
                return false;
        }
        if(BadWordFilter.ContainsBadWord(playerName))
        {
            MainMenuUIHandler.Instance.Error("Error", "OFF ALL THE NAMES YOU CHOOSE THIS PICK AGAIN");
            return false;
        }

        return true;
    }

    private async void LoadCloudData()
    {
        await GetCloudData();
    }
    public async Task GetCloudData()
    {
        MainMenuUIHandler.Instance.OpenActionStatus("GETTING CLOUD DATA");
        await SaveDataLocal.Instance.LoadCloudData();
        MainMenuUIHandler.Instance.CloseActionStatus("CLOUD DATA LOADED", 2);
    }
}












