using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SignUp : MonoBehaviour
{
    public static SignUp Instance { set; get; }
    
    [Header("Anonymous")] 
    public Button anonymousSignInButton;
    public GameObject createAccountButton;
    
    [Header("Profile")] 
    public Button deleteProfileButton;
    
    [Header("Convert Account Panels")] 
    public GameObject convertWithAppleButton;
    public GameObject convertWithGoogleButton;
    public GameObject mainSettingPanel;
    public GameObject convertEmailPanel;
    public GameObject convertAccountPanel;

    [Header("Anonymous Login Panel")] 
    public GameObject anonymousWarningPanel;
    
    private void Start()
    {
        deleteProfileButton.onClick.AddListener(DeleteProfileData);
        anonymousSignInButton.onClick.AddListener(AnonymousSignIn);
        
        Instance = this;
    }
    
    #region DeleteProfile
    private async void DeleteProfileData()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            MainMenuUIHandler.Instance.Error("ERROR", "NOT SIGNED IN");
            return;
        }
        MainMenuUIHandler.Instance.OpenActionStatus("DELETING PROFILE AND DATA");
        await SaveDataLocal.Instance.DeleteCloudData();
        await DeleteAccount();
        
        //Clear session token
        if(AuthenticationService.Instance.SessionTokenExists)
            AuthenticationService.Instance.ClearSessionToken();
        
        MainMenuUIHandler.Instance.CloseActionStatus("DATA DELETED SUCCESSFULLY", 2);
    }
    
    //Todo delete all player data too
    async Task DeleteAccount()
    {
        try
        {
            await AuthenticationService.Instance.DeleteAccountAsync();
            SaveDataLocal.Instance.guessLogin = "guess";
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
    #endregion

    #region AnonymousSignIn

    public void OpenAnonymousWarningPanel()
    {
        if (SaveDataLocal.Instance.previousAccount == "real")
        {
            anonymousWarningPanel.SetActive(true);
        }
        else
        {
            AnonymousSignIn();
        }
    }
    
    private async void AnonymousSignIn()
    {
        try
        {
            await SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
    
    async Task SignInAnonymouslyAsync()
    {
        try
        {
            MainMenuUIHandler.Instance.OpenActionStatus("SIGNING IN");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            MainMenuUIHandler.Instance.CloseActionStatus("SIGNED IN", 2);
            SaveDataLocal.Instance.logInStatus = "in";
            SaveDataLocal.Instance.guessLogin = "guess";
            
            //Only resets if there was a previous account
            SaveDataLocal.Instance.AnonymousLoginReset();
            anonymousWarningPanel.SetActive(false);
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
    
    #endregion
    
    
    #region SignOut
    public void OnSignOutClicked()
    {
        try
        {
            MainMenuUIHandler.Instance.OpenActionStatus("SIGNING OUT");
            AuthenticationService.Instance.SignOut();
            MainMenuUIHandler.Instance.CloseActionStatus("SIGNED OUT", 2);
            
            //Clear session token
            if (SaveDataLocal.Instance.guessLogin != "guess")
            {
                if (AuthenticationService.Instance.SessionTokenExists)
                    AuthenticationService.Instance.ClearSessionToken();
            }

            SaveDataLocal.Instance.guessLogin = "guess";
            SaveDataLocal.Instance.SaveGame();
        }
        catch (AuthenticationException ex)
        {
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(),ex.Message);
        }
        catch (RequestFailedException ex)
        {
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(),ex.Message);
        }
    }
    #endregion
    
    private void GuessAccount()
    {
        createAccountButton.SetActive(SaveDataLocal.Instance.guessLogin == "guess");    
    }

    public async Task UpdateCloudData()
    { 
        //LOAD PLAYER CLOUD DATA IF FOUND
        //MainMenuUIHandler.Instance.gettingCloudDataScreen.SetActive(true);
        await SaveDataLocal.Instance.LoadCloudData();
        //MainMenuUIHandler.Instance.gettingCloudDataScreen.SetActive(false);
        
        SaveDataLocal.Instance.guessLogin = "real";
        SaveDataLocal.Instance.previousAccount = "real";
        Debug.Log("Got cloud data");
        Debug.Log("Login in Status "+ SaveDataLocal.Instance.logInStatus);
        SaveDataLocal.Instance.SaveGame();
    }
    
    private void Update()
    {
        //GuessAccount();
    }
}
