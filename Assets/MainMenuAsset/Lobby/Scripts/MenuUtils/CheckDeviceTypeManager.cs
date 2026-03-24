using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckDeviceTypeManager : MonoBehaviour
{
    public static CheckDeviceTypeManager Instance { set; get; }

    public bool handheld;
    public bool pc;

    public string playerDeviceType;
    
    [Header("Platforms")] 
    public bool android;
    public bool ios;
    public bool web;
    public bool windows;
    public bool mac;
    
    public void Awake()
    {
        Instance = this;

        var deviceType = SystemInfo.deviceType;

        handheld = deviceType == DeviceType.Handheld;
        pc = deviceType == DeviceType.Desktop;
        
        CheckDevicePlatform();
    }

    private void CheckDevicePlatform()
    {
        android = Application.platform == RuntimePlatform.Android;
        ios = Application.platform == RuntimePlatform.IPhonePlayer;
        web = Application.platform == RuntimePlatform.WebGLPlayer;
        windows = Application.platform == RuntimePlatform.WindowsPlayer;
        mac = Application.platform == RuntimePlatform.OSXPlayer;

        if (android)
        {
            playerDeviceType = "Android";
        }
        else if (ios)
        {
            playerDeviceType = "IOS";
        }
        else if (web)
        {
            playerDeviceType = "Web";
        }
        else if (windows)
        {
            playerDeviceType = "Windows";
        }
        else if (mac)
        {
            playerDeviceType = "Mac";
        }
        else
        {
            playerDeviceType = "Unknown";
        }
    }
    
}
