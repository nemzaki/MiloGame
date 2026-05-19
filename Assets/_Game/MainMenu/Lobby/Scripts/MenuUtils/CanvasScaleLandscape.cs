using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CanvasScaleLandscape : MonoBehaviour
{

    private CanvasScaler _canvasScaler;
    
    public ScaleLandscape mobileScale;
    public ScaleLandscape pcScale;

    private void OnEnable()
    {
        _canvasScaler = GetComponent<CanvasScaler>();
        
        
        if(CheckDeviceTypeManager.Instance == null)
            return;

        _canvasScaler.referenceResolution = CheckDeviceTypeManager.Instance.handheld ? 
            new Vector2(mobileScale.width, mobileScale.length) : new Vector2(pcScale.width, pcScale.length);
    }
}

[Serializable]
public class ScaleLandscape
{
    public float length;
    public float width;
}
