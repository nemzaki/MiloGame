using System;
using UnityEngine;


public class CloudSave : MonoBehaviour
{
    
    public static CloudSave Instance { set; get; }

    private void Awake()
    {
        Instance = this;
    }
}






