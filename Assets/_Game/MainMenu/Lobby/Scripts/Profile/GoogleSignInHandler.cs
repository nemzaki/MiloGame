using System;
using System.Threading.Tasks;
using UnityEngine;
//using GooglePlayGames;
//using GooglePlayGames.BasicApi;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.UI;

public class GoogleSignInHandler : MonoBehaviour
{
    [Header("UI")] 
    public GameObject unlinkGoogle;
    public GameObject unlinkGooglePanel;
    public Button unlinkButton;

    /*
    #if UNITY_ANDROID   
    private void Awake()
    {
        InitializePlayGamesLogin();
    }

    private void Update()
    {
        unlinkGoogle.SetActive(!string.IsNullOrEmpty(SaveDataLocal.Instance.googleIDToken));
    }

    public void OpenUnlinkGooglePanel()
    {
        unlinkGooglePanel.SetActive(true);
    }

    public void CloseUnlinkGooglePanel()
    {
        unlinkGooglePanel.SetActive(false);
    }

    public async void UnlinkAccount()
    {
        await UnlinkGoogleAsync();
    }
    
    void InitializePlayGamesLogin()
    {
        var config = new PlayGamesClientConfiguration.Builder()
            // Requests an ID token be generated.  
            // This OAuth token can be used to
            // identify the player to other services such as Firebase.
            .RequestIdToken()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }
    
    public void LoginGoogle()
    {
        Social.localUser.Authenticate(OnGoogleLogin);
    }

    async void OnGoogleLogin(bool success)
    {
        if (success)
        {
            Debug.Log("Login with Google done. IdToken: " + ((PlayGamesLocalUser)Social.localUser).GetIdToken());
            //SaveDataLocal.Instance.googleIDToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
            //await SignInWithGoogleAsync(((PlayGamesLocalUser)Social.localUser).GetIdToken());
        }
        else
        {
            MainMenuUIHandler.Instance.Error("ERROR", "GOOGLE LOGIN FAILED");
        }
    }
    
    async Task SignInWithGoogleAsync(string idToken)
    {
        try
        {
            MainMenuUIHandler.Instance.OpenActionStatus("SIGNING IN");
            await AuthenticationService.Instance.SignInWithGoogleAsync(idToken);
            MainMenuUIHandler.Instance.CloseActionStatus("SIGNED IN");
        }
        catch (AuthenticationException ex)
        {
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(), ex.ToString());
        }
        catch (RequestFailedException ex)
        {
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(), ex.ToString());
        }
    }
    
    async Task UnlinkGoogleAsync()
    {
        try
        {
            MainMenuUIHandler.Instance.OpenActionStatus("UNLINKING GOOGLE ACCOUNT");
            await AuthenticationService.Instance.UnlinkGoogleAsync();
            MainMenuUIHandler.Instance.CloseActionStatus("GOOGLE ACCOUNT UNLINKED");
        }
        catch (AuthenticationException ex)
        {
            MainMenuUIHandler.Instance.Error("ERROR", ex.ToString());
        }
        catch (RequestFailedException ex)
        {
            MainMenuUIHandler.Instance.Error("ERROR", ex.ToString());
        }
    }
#endif*/
}
