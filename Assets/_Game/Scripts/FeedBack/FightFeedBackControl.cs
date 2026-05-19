using Lofelt.NiceVibrations;
using MoreMountains.Feedbacks;
using UnityEngine;

public class FightFeedBackControl : MonoBehaviour
{
    public static FightFeedBackControl Instance{get; private set;}
    
    public MMF_Player feedbackPlayer;
    public MMF_Player feedbackHardHit;
    
    private void Awake()
    {
        Instance = this;
    }

    public void PlayLightHitFeedBack()
    {
        feedbackPlayer.PlayFeedbacks();

        if (SaveDataLocal.Instance.haptics == "on")
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
        }
    }
    
    public void PlayHardHitFeedBack()
    {
        feedbackHardHit.PlayFeedbacks();
        
        if (SaveDataLocal.Instance.haptics == "on")
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
        }
    }
}
