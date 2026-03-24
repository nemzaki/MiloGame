using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject appleSignInButton;
    public GameObject googleSignInButton;

    private void Awake()
    {
        CheckPlatformSignIn();
        Application.targetFrameRate = 120;
    }

    public void CheckPlatformSignIn()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                //appleSignInButton.SetActive(true);
                //googleSignInButton.SetActive(false);
                break;
            
            case RuntimePlatform.Android:
                //appleSignInButton.SetActive(false);
                //googleSignInButton.SetActive(true);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
  
}
