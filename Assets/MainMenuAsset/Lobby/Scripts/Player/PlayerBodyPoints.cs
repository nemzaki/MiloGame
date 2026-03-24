using System;
using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerBodyPoints : MonoBehaviour
{
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int IdleType = Animator.StringToHash("IdleType");

    //private FullBodyBipedIK _fullBodyBipedIK;
    private UpdateCharacterVisuals _updateCharacterVisuals;
    private UpdatePlayerMovementAnimator _updateAnimatorMove;
    private PlayerMovementHandler _playerMovementHandler;
    private PlayerEffectsHandler _playerEffectsHandler;
    private MenuPlayerMove _menuPlayerMove;
    
    //private AimIK _aimIK;
    //private LookAtIK _lookAtIK;
    
    public Transform projectileSpawnPoint;
    public Animator anim;
    public Transform rootBone;
    
    private void Awake()
    {
        anim = GetComponent<Animator>();
        _updateCharacterVisuals = GetComponentInParent<UpdateCharacterVisuals>();
        
        //_aimIK = GetComponent<AimIK>();
        //_lookAtIK = GetComponent<LookAtIK>();
        //_fullBodyBipedIK = GetComponent<FullBodyBipedIK>();
    }

    private void Start()
    {
        StartCoroutine(SetStuff());
    }

    IEnumerator SetStuff()
    {
        yield return new WaitForEndOfFrame();

        if (_updateCharacterVisuals.inMenu)
        {
            _menuPlayerMove = GetComponentInParent<MenuPlayerMove>();
            _menuPlayerMove.anim = anim;
            GameShop.Instance.anim = GetComponent<Animator>();
            SetIdles();
        }
        
        //_aimIK.enabled = false;
        //_lookAtIK.enabled = false;
        
        if(_updateCharacterVisuals.inMenu)
            yield break;
        
        _updateAnimatorMove = GetComponentInParent<UpdatePlayerMovementAnimator>();
        _playerMovementHandler = GetComponentInParent<PlayerMovementHandler>();
        _playerEffectsHandler = GetComponentInParent<PlayerEffectsHandler>();

        _playerEffectsHandler.spawnPosition = projectileSpawnPoint;
        _updateAnimatorMove._anim = anim;
        
        //_aimIK.enabled = true;
        //_lookAtIK.enabled = true;
        
        _updateAnimatorMove.rootBone = rootBone;
    }

    private void SetIdles()
    {
        anim.SetBool(Idle, true);
        anim.SetInteger(IdleType, Random.Range(1,5));
    }
}
