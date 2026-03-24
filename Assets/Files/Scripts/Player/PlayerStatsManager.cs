using Quantum;
using Quantum.Prototypes;
using UnityEngine;

public unsafe class PlayerStatsManager : QuantumCallbacks
{
    [SerializeField] private QuantumEntityView _entityView;

    private PlayerMovementHandler _playerMovementHandler;
    
    private float _hardHitTimer;
    
    private void Awake()
    {
        _entityView = GetComponent<QuantumEntityView>();
        _playerMovementHandler = GetComponent<PlayerMovementHandler>();
    }

    public void ShowHardHitIndicator()
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;

        if (!frame.Exists(_entityView.EntityRef))
            return;
        
        var playerStat = frame.Unsafe.GetPointer<PlayerStat>(_entityView.EntityRef);
        var playerHealth = InGameUIHandler.Instance.playerHealth[playerStat->PlayerNumber];
        
        _hardHitTimer = 0;
        playerHealth.hardHitIndicator.SetActive(true);
    }
    
    private void HardHitIndicator(PlayerHealth playerHealth)
    {
        if (playerHealth.hardHitIndicator.activeSelf)
        {
            _hardHitTimer += Time.deltaTime;

            if (_hardHitTimer >= 0.5f)
            {
                playerHealth.hardHitIndicator.SetActive(false);
                _hardHitTimer = 0;
            }
        }
    }
    
    //CONNECTION
    private void WarningConnection()
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;

        if (!frame.Exists(_entityView.EntityRef))
            return;
        
        var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(_entityView.EntityRef);
        var playerInput = playerMovement->InputDesires;

        InGameUIHandler.Instance.warningBadConnectionPanel.SetActive(playerInput.ConnectionWarning);
    }
    
    public override void OnUpdateView(QuantumGame game)
    {
        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        //var predictedFrame = QuantumRunner.Default.Game.Frames.Predicted;

        if (!frame.Exists(_entityView.EntityRef))
            return;
        
        var playerAttack = frame.Get<PlayerAttack>(_entityView.EntityRef);
        var config = QuantumUnityDB.GetGlobalAsset<PlayerConfig>(playerAttack.playerConfig.Id);
        var playerStat = frame.Unsafe.GetPointer<PlayerStat>(_entityView.EntityRef);

        var playerHealth = InGameUIHandler.Instance.playerHealth[playerStat->PlayerNumber];
        playerHealth.playerName.text = playerStat->PlayerName.ToString();
        
        switch (playerStat->RoundsWon)
        {
            case 1:
                playerHealth.round1Won.color = InGameUIHandler.Instance.roundWonActiveColor;
                break;
            case 2:
                playerHealth.round2Won.color = InGameUIHandler.Instance.roundWonActiveColor;
                break;
        }

        //PLAYER HEALTH
        var healthNormalized = Mathf.InverseLerp(0, 100, playerStat->PlayerHealth);
        playerHealth.playerHealthSlider.Value = healthNormalized;
        
        //PLAYER STAMINA
        var staminaNormalized = Mathf.InverseLerp(0, 100, playerStat->PlayerStamina.AsFloat);
        playerHealth.playerStaminaSlider.Value = staminaNormalized;
        playerHealth.noStaminaIndicator.SetActive(playerAttack.NoStamina);
        
        //PLAYER PING
        playerHealth.badPingIndicator.SetActive(playerStat->BadPing);
        
        //HIT COUNTER
        if (playerStat->HitCounter > 0)
        {
            playerHealth.hitCounter.SetActive(true);
            playerHealth.hitCounterText.text = playerStat->HitCounter.ToString();
        }
        else
        {
            playerHealth.hitCounter.SetActive(false);
        }
        
        if (_playerMovementHandler.isLocalPlayer)
        {
            InGameUIHandler.Instance.checkPlayerWon = playerStat->Won;
        }
        
        //HARD HIT INDICATOR
        HardHitIndicator(playerHealth);
        
        //CONNECTION 
        WarningConnection();
    }

}
