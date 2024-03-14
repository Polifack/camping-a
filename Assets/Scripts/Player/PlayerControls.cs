using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    public PlayerStateMachine Character;
    public ExampleCharacterCamera CharacterCamera;
    public PlayerCharacterInputs characterInputs;


    // ESTA MERDA NON VAI
    //public PlayerInput _playerInput;

    // [Header("Movement")]
    // //[SerializeField] public bool _canMove = true;
    // [SerializeField] public Vector2 _currentMovementInput;


    private const string MouseXInput = "Mouse X";
    private const string MouseYInput = "Mouse Y";
    private const string MouseScrollInput = "Mouse ScrollWheel";
    private const string HorizontalInput = "Horizontal";
    private const string VerticalInput = "Vertical";

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // Tell camera to follow transform
        CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);

        // Ignore the character's collider(s) for camera obstruction checks
        CharacterCamera.IgnoredColliders.Clear();
        CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());

        // Struct to pass on to the character controller
        characterInputs = new PlayerCharacterInputs();

        // ESTA MERDA NON VAI

        //Input callbacks, trust me bro they are different
        // _playerInput = new PlayerInput();

        // //Movement
        // _playerInput.CharacterControls.Move.started += onMovementInput;
        // _playerInput.CharacterControls.Move.canceled += onMovementInput;
        // _playerInput.CharacterControls.Move.performed += onMovementInput;
        
        // //Jump
        // _playerInput.CharacterControls.Jump.started += onJump;
        // _playerInput.CharacterControls.Jump.canceled += onJump;

        // //Attack
        // _playerInput.CharacterControls.Attack.started += onAttack;
        // _playerInput.CharacterControls.Attack.canceled += onAttack;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        HandleCharacterInput();
    }

    private void LateUpdate()
    {
        // Handle rotating the camera along with physics movers
        if (CharacterCamera.RotateWithPhysicsMover && Character.Motor.AttachedRigidbody != null)
        {
            CharacterCamera.PlanarDirection = Character.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * CharacterCamera.PlanarDirection;
            CharacterCamera.PlanarDirection = Vector3.ProjectOnPlane(CharacterCamera.PlanarDirection, Character.Motor.CharacterUp).normalized;
        }

        HandleCameraInput();
    }

    private void HandleCameraInput()
    {
        // Create the look input vector for the camera
        float mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
        float mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
        Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

        // Prevent moving the camera while the cursor isn't locked
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            lookInputVector = Vector3.zero;
        }

        // Input for zooming the camera (disabled in WebGL because it can cause problems)
        float scrollInput = -Input.GetAxis(MouseScrollInput);
#if UNITY_WEBGL
    scrollInput = 0f;
#endif

        // Apply inputs to the camera
        CharacterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);

        // Handle toggling zoom level
        if (Input.GetMouseButtonDown(1))
        {
            CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;
        }
    }

    private void HandleCharacterInput()
    {
        // PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

        // Build the CharacterInputs struct
        characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
        characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);
        characterInputs.IsMovementPressed = characterInputs.MoveAxisForward != 0 || characterInputs.MoveAxisRight != 0;

        characterInputs.CameraRotation = CharacterCamera.Transform.rotation;

        characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
        characterInputs.JumpUp = Input.GetKeyUp(KeyCode.Space);

        characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.C);
        characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.C);

        characterInputs.Attack1Down = Input.GetKeyDown(KeyCode.Mouse0);
        characterInputs.Attack1Up = Input.GetKeyUp(KeyCode.Mouse0);

        characterInputs.InteractDown = Input.GetKeyDown(KeyCode.E);
        characterInputs.InteractUp = Input.GetKeyUp(KeyCode.E);

        // Apply inputs to character
        Character.SetInputs(ref characterInputs);
    }

    //ESTA PUTA MERDA NON VAI

    // void onMovementInput (InputAction.CallbackContext context)
    // {
    //     _currentMovementInput = context.ReadValue<Vector2>();
    //     Debug.Log("Movement Input: " + _currentMovementInput);
    //     characterInputs.MoveAxisForward = _currentMovementInput.x;
    //     characterInputs.MoveAxisRight = _currentMovementInput.y;
    //     characterInputs.IsMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    // }

    // void onJump(InputAction.CallbackContext context)
    // {
    //     characterInputs.JumpDown = context.ReadValueAsButton();
    //     Debug.Log("Movement Input: " + characterInputs.JumpDown);
    //     //_requiredNewJumpPress = false;
    // }

    // void onAttack(InputAction.CallbackContext context)
    // {
    //     characterInputs.Attack1Down = context.ReadValueAsButton();
    //     //_isAttackPressed = false;
    // }
}