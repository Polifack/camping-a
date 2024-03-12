using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] CharacterController _characterController;
    [SerializeField] Animator _animator;


    [Header("Movement")]
    [SerializeField] Vector2 _currentMovementInput;
    [SerializeField] Vector3 _currentMovement;
    [SerializeField] Vector3 _appliedMovement;
    [SerializeField] bool _isMovementPressed;
    [SerializeField] float _speedMultiplier;
    [SerializeField] float _rotationFactorPerFrame;


    [Header("Gravity")]
    [SerializeField] float _gravity = -9.8f;
    [SerializeField] bool _isFalling = false;
    [SerializeField] float _fallMultiplier = 2.0f;


    [Header("Jumping")]
    [SerializeField] float _maxJumpHeight = 1f;
    [SerializeField] float _maxJumpTime = 0.75f;
    float _initialJumpVelocity;
    [SerializeField] bool _isJumpPressed = false;
    [SerializeField] bool _isJumping = false;


    void Awake()
    {
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _playerInput.CharacterControls.Move.started += onMovementInput;
        _playerInput.CharacterControls.Move.canceled += onMovementInput;
        _playerInput.CharacterControls.Move.performed += onMovementInput;

        _playerInput.CharacterControls.Jump.started += onJump;
        _playerInput.CharacterControls.Jump.canceled += onJump;

        // ??????? 
        // _playerInput.CharacterControls.Interact.started += ctx => handleInteract();

        setupJumpVariables();

    }

    void setupJumpVariables()
    {
        float timeToApex = _maxJumpTime/2;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex,2);
        _initialJumpVelocity = (2* _maxJumpHeight)/timeToApex;
    }

    void onMovementInput (InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x;
        _currentMovement.z = _currentMovementInput.y;
        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;

    }

    void onJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
    }

    void handleJump()
    {
        if (!_isJumping && _characterController.isGrounded && _isJumpPressed){
            _isJumping = true;
            _currentMovement.y = _initialJumpVelocity;
            _appliedMovement.y = _initialJumpVelocity;
            _animator.SetBool("_isJumping", true);
        }
        else if (_isJumping && _characterController.isGrounded && !_isJumpPressed)
        {
            _isJumping = false;
        }
    }

    void handleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = _currentMovement.x;
        positionToLookAt.y = 0f;
        positionToLookAt.z = _currentMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }

    }

    void handleAnimation()
    {
        if (_isMovementPressed && (!_isFalling || !_isJumping))
            _animator.SetBool("isRunning", true);
        else
            _animator.SetBool("isRunning", false);
    }

    void handleGravity()
    {
        _isFalling = (_currentMovement.y <= 0.0f || !_isJumpPressed);

        if (_characterController.isGrounded) 
        {
            _currentMovement.y = _gravity;
            _appliedMovement.y = _gravity;
            _animator.SetBool("_isFalling", false);
            _animator.SetBool("_isJumping", false);
        } 
        else if (_isFalling)
        {
            _animator.SetBool("_isFalling", true);
            float previousYVelocity = _currentMovement.y;
            _currentMovement.y = _currentMovement.y + (_gravity * _fallMultiplier * Time.deltaTime);
            _appliedMovement.y = Mathf.Max((previousYVelocity + _currentMovement.y) * .5f, -20f);
        }
        else 
        {
            float previousYVelocity = _currentMovement.y;
            _currentMovement.y = _currentMovement.y + (_gravity * Time.deltaTime);
            _appliedMovement.y = (previousYVelocity + _currentMovement.y) * .5f;
        }
    }

    void Update()
    {
        handleRotation();
        handleAnimation();

        _appliedMovement.x = _currentMovement.x;
        _appliedMovement.z = _currentMovement.z;
        _characterController.Move(_appliedMovement * _speedMultiplier * Time.deltaTime);
        
        handleGravity();
        handleJump();
    }

    // Update is called once per frame
    void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }
}
