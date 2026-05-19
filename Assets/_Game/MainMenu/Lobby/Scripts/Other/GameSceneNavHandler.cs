using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneNavHandler : MonoBehaviour
{
    
    public static GameSceneNavHandler Instance { set; get; }
    
    [Header("Scene")]
    public Canvas mainMenuCanvas;
    public GameObject mainMenuScene;
    public GameObject menuManager;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
    }

  
    void Update()
    {
        
    }
}
