using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AuthManager : MonoBehaviour
{
    
    public static AuthManager Instance { set; get; }

    public MainMenuUIHandler mainMenuHandler;
    
    public bool isInitialized;
    
    public bool debugMode;
    
    [SerializeField] 
    private VersionChecker checkGameVersion;
    
    void Awake()
    {
        Instance = this;
        
        StartClientService();
    }
    
    public void RetryConnection()
    {
        if(isInitialized)
            return;
          
        StartClientService();
    }
    
    private async void StartClientService()
    {
        try
        {
            await UnityServices.InitializeAsync();
            SetupEvents();

            Debug.Log("Login in status "+SaveDataLocal.Instance.logInStatus);
            if (SaveDataLocal.Instance.logInStatus == "in")
            {
                await SignInCachedUserAsync();
            }

            if (Application.platform != RuntimePlatform.OSXEditor)
            {
                await checkGameVersion.CheckGameVersion();
            }
            
            CheckStates();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            //_mainMenuHandler.connectionStatus.text = "Service initialization failed. Please try again.";
            mainMenuHandler.Error("ERROR", e.Message);
        }
    }
    
    
    private void CheckStates()
    {
        if (!AuthenticationService.Instance.IsSignedIn && !debugMode)
        {
            AuthMenuUIHandler.Instance.OnOpenAuthScreen();
        }
    }
    
    async Task SignInCachedUserAsync()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Player is already signed in.");
            return;
        }

        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            Debug.Log("No cached session found.");
            mainMenuHandler.connectionScreen.SetActive(false);
            return;
        }

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException ex)
        {
            mainMenuHandler.Error($"Auth Error: {ex.ErrorCode}", ex.Message);
        }
        catch (RequestFailedException ex)
        {
            mainMenuHandler.Error($"Request Error: {ex.ErrorCode}", ex.Message);
        }
    }


    //EVENT HANDLERS
    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => 
        {
            SignInConfirmAsync();
            
            //CLOSE AUTH SCREEN
            AuthMenuUIHandler.Instance.OnCloseAuthScreen();
            
            //OPEN MAIN SCREEN
            mainMenuHandler.OnMainPanelClicked();
            
            mainMenuHandler.connectionScreen.SetActive(false);
            
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
            isInitialized = true;
        };

        AuthenticationService.Instance.SignInFailed += (err) =>
        {
            //Debug.LogError(err);
            mainMenuHandler.Error(err.ErrorCode.ToString(), err.Message);
        };

        AuthenticationService.Instance.SignedOut += () => 
        {
            SaveDataLocal.Instance.logInStatus = "out";
            AuthMenuUIHandler.Instance.OnOpenAuthScreen();
            SaveDataLocal.Instance.SaveGame();
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }

    public async void SignInConfirmAsync()
    {
        try
        {
            await AuthenticationService.Instance.GetPlayerNameAsync();
            
            SaveDataLocal.Instance.playerName = AuthenticationService.Instance.PlayerName;
            mainMenuHandler.GetPlayerName(AuthenticationService.Instance.PlayerName);
            SaveDataLocal.Instance.SaveGame();
        }
        catch
        {
            // ignored
        }
    }
}
