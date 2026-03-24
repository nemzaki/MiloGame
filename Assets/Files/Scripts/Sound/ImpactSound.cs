using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ImpactSound : MonoBehaviour
{
    public AudioClip[] impactClips;
    private AudioSource  _audioSource;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        PlaySound();
    }

    private void PlaySound()
    {
        var clip = impactClips[Random.Range(0, impactClips.Length)];
        _audioSource.resource = clip;
        _audioSource.PlayOneShot(clip);
    }
}
