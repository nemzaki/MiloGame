
using System;
using System.Text;
using System.Threading.Tasks;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class AppleSignInHandler : MonoBehaviour
{
    IAppleAuthManager m_AppleAuthManager;
    public string Token { get; private set; }
    public string Error { get; private set; }

    [Header("UI")] 
    public GameObject unlinkApple;
    public GameObject unlinkAccountPanel;
    public Button unlinkButton;

    public bool convertingAccount;
    public GameObject convertingAccountPanel;
    
    private void Start()
    {
        unlinkButton.onClick.AddListener(UnlinkAccount);
    }

    public void Initialize()
    {
        var deserializer = new PayloadDeserializer();
        m_AppleAuthManager = new AppleAuthManager(deserializer);
    }

    public void Update()
    {
        if (m_AppleAuthManager != null) 
        {
            m_AppleAuthManager.Update();
        }

        unlinkApple.SetActive(!string.IsNullOrEmpty(SaveDataLocal.Instance.appleIDToken));
    }

    public void OpenUnlinkApplePanel()
    {
        unlinkAccountPanel.SetActive(true);
    }

    public void CloseUnlinkApplePanel()
    {
        unlinkAccountPanel.SetActive(false);
    }

    private async void UnlinkAccount()
    {
        try
        {
            await UnlinkAppleAsync();
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }

    public void ConvertAppleAccount()
    {
        convertingAccount = true;
    }

    public void NotConvertAppleAccount()
    {
        convertingAccount = false;
    }
    
    public void LoginToApple()
    {
        // Initialize the Apple Auth Manager
        if (m_AppleAuthManager == null)
        {
            Initialize();
        }
        
        // Set the login arguments
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        // Perform the login
        m_AppleAuthManager?.LoginWithAppleId(
            loginArgs, async credential =>
            {
                var appleIDCredential = credential as IAppleIDCredential;
                if (appleIDCredential != null)
                {
                    var idToken = Encoding.UTF8.GetString(
                        appleIDCredential.IdentityToken,
                        0,
                        appleIDCredential.IdentityToken.Length);
                   
                    Token = idToken;

                    if (!convertingAccount)
                    {
                        await SignInWithAppleAsync(Token);
                    }
                    else
                    {
                        await LinkWithAppleAsync(Token);
                    }
                }
                else
                {
                    Error = "Retrieving Apple Id Token failed.";
                    MainMenuUIHandler.Instance.Error("ERROR", "Sign-in with Apple error. Message: " +
                                                              "appleIDCredential is null");
                }
            },
            error =>
            {
                MainMenuUIHandler.Instance.Error("ERROR", "Sign-in " +
                                                          "with Apple error. Message: " + error);
                Error = "Retrieving Apple Id Token failed.";
            }
        );
    }
    
    async Task SignInWithAppleAsync(string idToken)
    {
        try
        {
            MainMenuUIHandler.Instance.OpenActionStatus("SIGNING IN");
            await AuthenticationService.Instance.SignInWithAppleAsync(idToken);
            MainMenuUIHandler.Instance.CloseActionStatus("SIGNED IN", 2);
            
            //CLEAR LOCAL DATA
            await SignUp.Instance.UpdateCloudData();
            
            SaveDataLocal.Instance.appleIDToken = Token;
        }
        catch (AuthenticationException ex)
        {
            //Debug.LogException(ex);
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(), ex.ToString());
        }
        catch (RequestFailedException ex)
        {
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(), ex.ToString());
        }
    }
    
    async Task LinkWithAppleAsync(string idToken)
    {
        try
        {
            MainMenuUIHandler.Instance.OpenActionStatus("LINKING WITH APPLE");
            await AuthenticationService.Instance.LinkWithAppleAsync(idToken);
            MainMenuUIHandler.Instance.CloseActionStatus("LINK SUCCESSFULLY", 2);
            
            //CLEAR LOCAL DATA
            convertingAccountPanel.SetActive(false);
            await SignUp.Instance.UpdateCloudData();
            SaveDataLocal.Instance.appleIDToken = Token;
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(), ex.ToString());
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(), ex.ToString());
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            MainMenuUIHandler.Instance.Error(ex.ErrorCode.ToString(), ex.ToString());
        }
    }
    
    async Task UnlinkAppleAsync()
    {
        try
        {
            MainMenuUIHandler.Instance.OpenActionStatus("UNLINKING APPLE ACCOUNT");
            await AuthenticationService.Instance.UnlinkAppleAsync();
            MainMenuUIHandler.Instance.CloseActionStatus("APPLE ACCOUNT UNLINKED",2);

            SaveDataLocal.Instance.previousAccount = "guess";
            SaveDataLocal.Instance.guessLogin = "guess";
            SaveDataLocal.Instance.appleIDToken = "";
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
}
