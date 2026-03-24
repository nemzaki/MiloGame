using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class PlayerDebug : QuantumCallbacks
{
    private QuantumEntityView _entityView;
    
    private PlayerMovementHandler _playerMovementHandler;

    [Header("Hit Visualizer")]
    public MeshRenderer hitVisualizer;
    public Color hitColor;
    public Color normalColor;
    public Color attackColor;
    
    // 
    void Start()
    {
        _entityView = GetComponent<QuantumEntityView>();
        _playerMovementHandler = GetComponent<PlayerMovementHandler>();
    }

    public override void OnUpdateView(QuantumGame game)
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        if (!frame.Exists(_entityView.EntityRef))
            return;

        var playerAttack = frame.Get<PlayerAttack>(_entityView.EntityRef);

        if (playerAttack.GotHit)
        {
            hitVisualizer.material.color = hitColor;
        }
        else if(!playerAttack.GotHit)
        {
            hitVisualizer.material.color = normalColor;
        }
        else if(playerAttack.isAttacking)
        {
            hitVisualizer.material.color = attackColor;
        }
    }
    
}
