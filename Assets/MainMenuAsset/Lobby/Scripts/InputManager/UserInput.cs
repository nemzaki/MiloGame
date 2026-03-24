using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class UserInput : MonoBehaviour
{
    public static UserInput Instance { set; get; }
    
    //MAIN MENU
    public bool EscapeInput { get; private set; }

    private InputAction _escapeAction;
    
    //IN GAME
    public bool PowerUpInput { get; private set; }
    public Vector2 PlayerMoveInput { get; private set; }
    
    public bool JumpInput { get; private set; }
    
    public bool AttackInput {get; private set;}
    
    public bool BlockInput { get; private set; }
    
    public bool SprintInput { get; private set; }
    
    private InputAction _powerUpAction;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private InputAction _blockAction;
    private InputAction _sprintAction;
    
    private PlayerInput _playerInput;
    
    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        _playerInput = GetComponent<PlayerInput>();
        
        SetupInputAction();
        EnhancedTouchSupport.Enable();
    }

    private void Update()
    {
        UpdateInputs();
    }

    private void SetupInputAction()
    {
        //MENU
        _escapeAction = _playerInput.actions["Escape"];
        
        //IN GAME
        _powerUpAction = _playerInput.actions["PowerUp"];
        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _attackAction = _playerInput.actions["Attack"];
        _blockAction = _playerInput.actions["Block"];
        _sprintAction = _playerInput.actions["Sprint"];
        
        _powerUpAction.Enable();
        _moveAction.Enable();
        _jumpAction.Enable();
        _attackAction.Enable();
        _blockAction.Enable();
        _sprintAction.Enable();
    }

    private void UpdateInputs()
    {
        //MENU
        EscapeInput = _escapeAction.IsPressed();
        
        //GAME PLAY
        PowerUpInput = _powerUpAction.IsPressed();
        PlayerMoveInput = _moveAction.ReadValue<Vector2>();
        AttackInput = _attackAction.IsPressed();
        BlockInput = _blockAction.IsPressed();
        
        JumpInput = _jumpAction.IsPressed();
        SprintInput = _sprintAction.IsPressed();
    }
}














