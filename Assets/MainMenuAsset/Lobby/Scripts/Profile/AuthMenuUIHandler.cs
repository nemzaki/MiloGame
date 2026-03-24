using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;

public class AuthMenuUIHandler : MonoBehaviour
{

    public static AuthMenuUIHandler Instance { set; get; }

    public GameObject mainAuthScreen;
    
    [Header("Panels")] 
    public GameObject loginMainPanel;

    private void Awake()
    {
        Instance = this;
    }

    public void OnCloseAuthScreen()
    {
        mainAuthScreen.SetActive(false);
    }

    public void OnOpenAuthScreen()
    {
        mainAuthScreen.SetActive(true);
        OnMainPanelClicked();
    }
    
    private void HideAllPanel()
    {
        loginMainPanel.SetActive(false);
    }

    public void OnMainPanelClicked()
    {
        HideAllPanel();
        loginMainPanel.SetActive(true);
        MainMenuUIHandler.Instance.HideAllPanels();
    }
    
    public void OnSignUpPanelClicked()
    {
        HideAllPanel();
    }

    public void OnSignInPanelClicked()
    {
        HideAllPanel();
    }
}
