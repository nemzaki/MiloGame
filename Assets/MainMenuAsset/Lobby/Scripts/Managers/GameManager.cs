using System;
using System.Collections;
using System.Collections.Generic;
using Quantum;
using RootMotion.FinalIK;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using Input = UnityEngine.Input;
using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    
    public QuantumRunnerLocalDebug localRunnerDebug;

    public EntityRef localPlayerEntity;
    public GameObject localPlayer;
    public bool isLocalPlayer;

    public int targetFPS = 60;
    
    
    [Header("Game State")]
    public bool gameInProgress;
    public bool gameFinished;
    
    [Header("Camera")]
    public CinemachineTargetGroup cameraTargetGroup;
    public AimCamera aimCamera;
    
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = targetFPS;
        
        localRunnerDebug = FindAnyObjectByType<QuantumRunnerLocalDebug>();
        localRunnerDebug.RuntimeConfig.Seed = Random.Range(0, 1000);
        
        //RenderSettings.fog = true; // Turn fog on
    }

    private void Start()
    {
        GetDataLocalRunner();
        
        RegisterMatchStat();
    }

    private static void RegisterMatchStat()
    {
        if (QuantumRunner.Default == null || !QuantumRunner.Default.IsRunning)
            return;

        var game = QuantumRunner.Default.Game;
        if (game == null)
            return;

        var frame = game.Frames.Verified;
        if (frame == null)
            return;
        
        if(frame.RuntimeConfig.training)
            return;

        SaveDataLocal.Instance.totalMatches += 1;
        SaveDataLocal.Instance.SaveGame();
    }
    
    private unsafe void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (EditorApplication.isPaused)
            {
                // Resume
                EditorApplication.isPaused = false;
            }
            else if (EditorApplication.isPlaying)
            {
                // Pause
                EditorApplication.isPaused = true;
            }
        }
        #endif
        
        if (QuantumRunner.Default == null || !QuantumRunner.Default.IsRunning)
            return;

        var game = QuantumRunner.Default.Game;
        if (game == null)
            return;

        var frame = game.Frames.Verified;
        if (frame == null)
            return;

        var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
        if (gameplay == null)
            return;

        gameInProgress = gameplay->State == EGameplayState.InProgress;
        gameFinished = gameplay->State == EGameplayState.Finished;
    }

    private void GetDataLocalRunner()
    {
        localRunnerDebug.LocalPlayers[0].PlayerNickname = SaveDataLocal.Instance.playerName;
        localRunnerDebug.LocalPlayers[0].currentPlayerIndex = SaveDataLocal.Instance.currentPlayerIndex;
        localRunnerDebug.LocalPlayers[0].idleType = SaveDataLocal.Instance.currentIdleType;
        localRunnerDebug.LocalPlayers[0].hardPunchType = SaveDataLocal.Instance.currentHardPunchType;
        localRunnerDebug.LocalPlayers[0].hardKickType = SaveDataLocal.Instance.currentHardKickType;
        localRunnerDebug.LocalPlayers[0].celebrateType = SaveDataLocal.Instance.currentCelebrationType;
        localRunnerDebug.LocalPlayers[0].moveType = SaveDataLocal.Instance.currentMovementType;
    }
}
