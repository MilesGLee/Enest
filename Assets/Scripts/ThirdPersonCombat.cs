using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(ThirdPersonMovement))]
public class ThirdPersonCombat : MonoBehaviour
{
    [Header("Combat Movement")]
    [SerializeField] private MeshRenderer _visualMeshRenderer;
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _invincibleMaterial;
    [SerializeField] private Transform _cameraTransform;
    private ThirdPersonMovement _movement;
    private CharacterController _controller;
    private float _dodgeCooldown = 1.0f;
    private float _dodgeDuration = 0.5f;
    private bool _canDodge;
    private bool _dodgeActive;
    private Vector3 _dodgeDirection;
    [Header("Combat Attacks")]
    [SerializeField] private Animator _animator;
    [Header("Targetting")]
    [SerializeField] private Transform _target;
    [SerializeField] private CinemachineBrain _cameraBrain;
    private bool _targetting;

    void Start()
    {
        _movement = GetComponent<ThirdPersonMovement>();
        _canDodge = true;
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        #region Dodging
        if (_canDodge && Input.GetKeyDown(KeyCode.Space))
        {
            _animator.SetTrigger("dodge");
            _canDodge = false;
            _dodgeActive = true;
            _movement.CanMove = false;
            RoutineBehaviour.Instance.StartNewTimedAction(args => { _canDodge = true; }, TimedActionCountType.SCALEDTIME, _dodgeCooldown);
            RoutineBehaviour.Instance.StartNewTimedAction(args => { _dodgeActive = false; _movement.CanMove = true; _visualMeshRenderer.material = _defaultMaterial; }, TimedActionCountType.SCALEDTIME, _dodgeDuration);
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            Vector3 inputDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
            if (inputDirection.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
                _dodgeDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
            else
            {
                _dodgeDirection = transform.forward;
            }
            
        }
        if (_dodgeActive && _dodgeDirection != Vector3.zero) 
        {
            _visualMeshRenderer.material = _invincibleMaterial;
            _controller.Move(_dodgeDirection * 10f * Time.deltaTime);
        }
        #endregion
        #region Attacking
        if (Input.GetKeyDown(KeyCode.Mouse0) && !_dodgeActive) 
        {
            _animator.SetTrigger("attack");
        }
        #endregion
        #region Targetting
        if (Input.GetKeyDown(KeyCode.F)) 
        {
            _targetting = !_targetting;
        }
        if (_targetting) 
        {
            _cameraBrain.enabled = false;
            _cameraBrain.transform.LookAt(_target);
        }
        if (!_targetting) 
        {
            _cameraBrain.enabled = true;
        }
        #endregion
    }

}
