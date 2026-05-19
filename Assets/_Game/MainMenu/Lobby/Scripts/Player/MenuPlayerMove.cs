using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuPlayerMove : MonoBehaviour
{
    
    
    public float moveSpeed;
    public float turnSpeed;
    
    private float _horizontal;
    private float _vertical;

    public Animator anim;
    private Rigidbody _rb;
    
    public Transform camTarget;
    public Vector3 targetOffset;
    private Vector3 moveDirection;

    public Transform closet;
    public Transform portal;
    public float distanceToEngage;
    public float portalDistance;
    public float closetDistance;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate; // smooth movement
    }

    private void FixedUpdate()
    {
        // Read input once per physics frame
        _horizontal = ControlFreak2.CF2Input.GetAxis("Horizontal");
        _vertical = ControlFreak2.CF2Input.GetAxis("Vertical");
    
        moveDirection = new Vector3(_horizontal, 0, _vertical);
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1); // optional, prevent diagonal speed > 1
    
        // Move rigidbody using velocity
        _rb.linearVelocity = moveDirection * moveSpeed;

        // Rotate with physics
        if (moveDirection.magnitude > 0)
        {
            var targetRotation = Quaternion.LookRotation(moveDirection);
            _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
    }

    private void LateUpdate()
    {
        CheckDistance();
        camTarget.transform.position = transform.position + targetOffset;
        
        if (!anim) return;
    
        anim.SetBool("IsMoving", moveDirection.magnitude != 0);
        anim.SetBool("InMenu", true);
        anim.SetBool("Grounded", true);
        anim.SetFloat("FightStanceRange", 4);
    }

    private void CheckDistance()
    {
        portalDistance = Vector3.Distance(transform.position, portal.position);

        GameShop.Instance.openPortalButton.SetActive(portalDistance < distanceToEngage);
        
        closetDistance = Vector3.Distance(transform.position, closet.position);
        
        GameShop.Instance.openShopButton.SetActive(closetDistance < distanceToEngage);
    }
}
