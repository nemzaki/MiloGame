using UnityEngine;

[CreateAssetMenu(fileName = "SoundFXData", menuName = "SoundFXData")]
public class SoundFXDataSO : ScriptableObject
{
    [Header("Swing Clips")]
    public AudioClip[] swingClips;
    
    [Header("Hit Clips")]
    public AudioClip[] hitClips;
    
    [Header("Block Clips")]
    public AudioClip[] blockClips;
}
