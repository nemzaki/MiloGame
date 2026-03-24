using Quantum;
using Quantum.Prototypes;
using Quantum.Scripts.Weapon;
using UnityEngine;

public class PlayerSoundEffectsManager : QuantumCallbacks
{
    private QuantumEntityView _entityView;
    
    private AudioSource _audioSource;
    
    [SerializeField] private SoundFXDataSO soundFXData;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _entityView = GetComponent<QuantumEntityView>();

        QuantumEvent.Subscribe<EventPlayerSwingClip>(this, PlayerSwingEvent);
        QuantumEvent.Subscribe<EventPlayerHitClip>(this, PlayerHitClip);
        QuantumEvent.Subscribe<EventPlayerBlockClip>(this, PlayerBlockClip);
    }

    private void OnDestroy()
    {
        QuantumEvent.UnsubscribeListener<EventPlayerSwingClip>(this);
        QuantumEvent.UnsubscribeListener<EventPlayerHitClip>(this);
        QuantumEvent.UnsubscribeListener<EventPlayerBlockClip>(this);
    }
    
    public override void OnUpdateView(QuantumGame game)
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        //var predictedFrame = QuantumRunner.Default.Game.Frames.Predicted;

        if (!frame.Exists(_entityView.EntityRef))
            return;
        
        var playerAttack = frame.Get<PlayerAttack>(_entityView.EntityRef);
    }
    
    //QUANTUM EVENTS
    private void PlayerSwingEvent(EventPlayerSwingClip playerSwingClip)
    {
        if(playerSwingClip.PlayerEntity != _entityView.EntityRef)
            return;
        
        var x = Random.Range(0, soundFXData.swingClips.Length);
        _audioSource.clip = soundFXData.swingClips[x];
        _audioSource.PlayOneShot(soundFXData.swingClips[x]);
    }

    private void PlayerHitClip(EventPlayerHitClip playerHitClip)
    {
        if(playerHitClip.PlayerEntity != _entityView.EntityRef)
            return;
        
        var x = Random.Range(0, soundFXData.hitClips.Length);
        _audioSource.clip = soundFXData.hitClips[x];
        _audioSource.PlayOneShot(soundFXData.hitClips[x]);
    }

    private void PlayerBlockClip(EventPlayerBlockClip playerBlockClip)
    {
        if(playerBlockClip.PlayerEntity != _entityView.EntityRef)
            return;
        
        var x = Random.Range(0, soundFXData.blockClips.Length);
        _audioSource.clip = soundFXData.blockClips[x];
        _audioSource.PlayOneShot(soundFXData.blockClips[x]);
    }
}
