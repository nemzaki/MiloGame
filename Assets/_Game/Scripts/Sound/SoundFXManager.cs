using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [Header("Impacts")]
    public AudioClip[] concreteImpacts;
    public AudioClip[] dirtImpacts;
    public AudioClip[] metalImpacts;
    public AudioClip[] rockImpacts;
    public AudioClip[] woodImpacts;


    private void PlayRandomImpact(AudioClip[] clips, AudioSource source)
    {
        
    }
}
