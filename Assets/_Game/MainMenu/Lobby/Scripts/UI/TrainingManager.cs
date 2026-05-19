using System;
using System.Collections;
using Quantum;
using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    public AssetRef<AIConfig> aiConfig;

    public bool aiActive;
    public GameObject activeIcon;
    public GameObject inactiveIcon;
    
    public GameObject aiIControlScreen;
    public GameObject controlScreen;

    private void Start()
    {
        var config = QuantumUnityDB.GetGlobalAsset<AIConfig>(aiConfig.Id);
        
        var frame = QuantumRunner.Default?.Game?.Frames.Verified;
        if(frame == null)
            return;

        StartCoroutine(EnableAI(frame, config));
    }

    IEnumerator EnableAI(Frame frame, AIConfig config)
    {
        yield return new WaitForSeconds(2);
        
        //Set ai active base on mode
        config.setActive = !frame.RuntimeConfig.training;
    }
    
    public void OpenAIControlScreen()
    {
        aiIControlScreen.SetActive(true);
    }

    public void CloseAIControlScreen()
    {
        aiIControlScreen.SetActive(false);
    }

    public void OpenControlScreen()
    {
        controlScreen.SetActive(true);
        aiIControlScreen.SetActive(false);
    }

    public void CloseControlScreen()
    {
        controlScreen.SetActive(false);
        aiIControlScreen.SetActive(true);
    }
    
    public void SetAIActive()
    {
        aiActive = !aiActive;
        
        activeIcon.SetActive(aiActive);
        inactiveIcon.SetActive(!aiActive);
        
        var config = QuantumUnityDB.GetGlobalAsset<AIConfig>(aiConfig.Id);
        config.setActive = aiActive;
    }
}
