using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeFormatter 
{
    public static string FormatTime(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);

        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);
        
        return formattedTime;
    }
    
    public static string FormatTimeAccurate(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);
        int milliseconds = Mathf.FloorToInt((totalSeconds * 1000) % 1000);
        
        string formattedTime = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        
        return formattedTime;
    }
}
