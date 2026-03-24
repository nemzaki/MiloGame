using System;
using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SettingsUIHandler : MonoBehaviour
{
    
    [Header("Graphics")]
    public int currentGraphicsIndex;
    public TextMeshProUGUI graphicsValueText;
    public string[] graphicsValues;
    
    [Header("Panels")] 
    public GameObject[] panels;

    [Header("Sliders")] 
    public Slider soundSlider;
    public Slider musicSlider;
    
    [Header("Toggle")] 
    public GameObject hapticsToggleOn;
    public GameObject hapticsToggleOff;
    
    

    private void Start()
    {
        //
        musicSlider.value = SaveDataLocal.Instance.musicFXVolume;
        soundSlider.value = SaveDataLocal.Instance.soundFXVolume;
        
        //
        hapticsToggleOn.SetActive(SaveDataLocal.Instance.haptics == "on");
        hapticsToggleOff.SetActive(SaveDataLocal.Instance.haptics == "off");
        
        currentGraphicsIndex = SaveDataLocal.Instance.graphics;
        graphicsValueText.text = graphicsValues[currentGraphicsIndex];

        QualitySettings.SetQualityLevel(SaveDataLocal.Instance.graphics);
    }
    
    //Controls
    public void ChangePanel(int index)
    {
        for (var i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(index == i);
        }
    }
    
    //
    public void HapticsChange()
    {
        if (SaveDataLocal.Instance.haptics == "on"
            || string.IsNullOrEmpty(SaveDataLocal.Instance.haptics))
        {
            SaveDataLocal.Instance.haptics = "off";
        }
        else
        {
            SaveDataLocal.Instance.haptics = "on";
        }
        
        hapticsToggleOn.SetActive(SaveDataLocal.Instance.haptics == "on");
        hapticsToggleOff.SetActive(SaveDataLocal.Instance.haptics == "off");
    }
    
    public void OnMusicVolumeChange()
    {
        SaveDataLocal.Instance.musicFXVolume = musicSlider.value;
    }

    public void OnSoundVolumeChange()
    {
        SaveDataLocal.Instance.soundFXVolume = soundSlider.value;
    }
    
    //GRAPHICS
    public void GraphicsNext()
    {
        if(currentGraphicsIndex == graphicsValues.Length - 1)
            return;
        
        currentGraphicsIndex += 1;
        graphicsValueText.text = graphicsValues[currentGraphicsIndex];
        SaveDataLocal.Instance.graphics = currentGraphicsIndex;
        SaveDataLocal.Instance.SaveGame();
        
        QualitySettings.SetQualityLevel(SaveDataLocal.Instance.graphics);
    }

    public void GraphicsPrevious()
    {
        if(currentGraphicsIndex == 0)
            return;
        
        currentGraphicsIndex -= 1;
        graphicsValueText.text = graphicsValues[currentGraphicsIndex];
        SaveDataLocal.Instance.graphics = currentGraphicsIndex;
        SaveDataLocal.Instance.SaveGame();
        
        QualitySettings.SetQualityLevel(SaveDataLocal.Instance.graphics);
    }
}