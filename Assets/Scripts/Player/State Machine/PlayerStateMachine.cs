using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem;
using KinematicCharacterController;
//using UnityEditorInternal;

public enum CharacterState
{
    Default, 
    LedgeGrabbing, 
    Swimming, 
    Interacting, 
    Attacking,
    Climbing
}

public enum ClimbingState
{
    Anchoring,
    Climbing,
    DeAnchoring
}

public enum OrientationMethod
{
    TowardsCamera,
    TowardsMovement,
}

public struct PlayerCharacterInputs
{
    public float MoveAxisForward;
    public float MoveAxisRight;
    public bool IsMovementPressed;

    public Quaternion CameraRotation;

    public bool JumpDown;
    public bool JumpUp;

    public bool CrouchDown;
    public bool CrouchUp;

    public bool Attack1Down;
    public bool Attack1Held;
    public bool Attack1Up;

    public bool InteractDown;
    public bool InteractUp;
}

public struct AICharacterInputs
{
    public Vector3 MoveVector;
    public Vector3 LookVector;
}

public enum BonusOrientationMethod
{
    None,
    TowardsGravity,
    TowardsGroundSlopeAndGravity,
}

  //Struct used to set up the damage, knockback direction and amount of
    //the basic combo attacks.
// public struct GroundAttackData
// {
//     public string[] animName;
//     // public float[] knockback;
//     // public Vector3[] knockbackDir;
//     // public float[] damage;
// };

public class PlayerStateMachine : MonoBehaviour, ICharacterController
{
    [Header("References")]
    [SerializeField] public Animator _animator;
    public KinematicCharacterMotor Motor;

    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f;
    public float StableMovementSharpness = 15f;
    public float OrientationSharpness = 10f;
    public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

    [Header("Air Movement")]
    public float MaxAirMoveSpeed = 15f;
    public float AirAccelerationSpeed = 15f;
    public float Drag = 0.1f;

    [Header("Jumping")]
    public bool AllowJumpingWhenSliding = false;
    public float JumpUpSpeed = 10f;
    public float JumpScalableForwardSpeed = 10f;
    public float JumpPreGroundingGraceTime = 0f;
    public float JumpPostGroundingGraceTime = 0f;

    [Header("Misc")]
    public List<Collider> IgnoredColliders = new List<Collider>();
    public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
    public float BonusOrientationSharpness = 10f;
    public Vector3 Gravity = new Vector3(0, -30f, 0);
    public Transform MeshRoot;
    public Transform CameraFollowPoint;
    public float CrouchedCapsuleHeight = 1f;

    [Header("Ladder Climbing")]
    public float ClimbingSpeed = 4f;
    public float AnchoringDuration = 0.25f;
    public LayerMask InteractionLayer;

    //MERDA VELLA, non borrar que fai falta para refactorizar o ledgegrab

    [Header("Ledge Grab")]
    [SerializeField] public Transform HeadRayCast;
    [SerializeField] public Transform TorsoRaycast;
    [SerializeField] public Vector2 LedgeGrabSize = new Vector2(0.3f, 0f);
    [SerializeField] public float LedgeGrabDistance = 0.6f;
    [SerializeField] public float LedgeGrabOffset = 0.1f;

    [SerializeField] public bool _canLedgeGrab = true;
    [SerializeField] public Vector3 _ledgeGrabPosition;
    [SerializeField] public Vector3 _ledgeGrabDirection;

    //END MERDA VELLA 

    //MERDA MIÑA

    [Header("Interacting")]
    public Vector3 InteractablePosition;
    public Quaternion InteractableRotation;
    

    [Header("Attacking")]
    public bool canAttack = true;
    public int currentCombo = 0;
    //Create a string list array to store the combo list
    public string[] comboList;


    // PRIVATE VARIABLES --------------------------------------------------
    public CharacterState CurrentCharacterState { get; private set; }

    private Collider[] _probedColliders = new Collider[8];
    private RaycastHit[] _probedHits = new RaycastHit[8];
    public Vector3 _moveInputVector;
    private Vector3 _lookInputVector;
    private bool IsMovementPressed = false;
    private bool _jumpRequested = false;
    private bool _jumpConsumed = false;
    private bool _jumpedThisFrame = false;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump = 0f;
    private Vector3 _internalVelocityAdd = Vector3.zero;
    private bool _shouldBeCrouching = false;
    private bool _isCrouching = false;

    private Vector3 lastInnerNormal = Vector3.zero;
    private Vector3 lastOuterNormal = Vector3.zero;

    // Ladder vars
    private float _ladderUpDownInput;
    private MyLadder _activeLadder { get; set; }
    private ClimbingState _internalClimbingState;
    private ClimbingState _climbingState
    {
        get
        {
            return _internalClimbingState;
        }
        set
        {
            _internalClimbingState = value;
            _anchoringTimer = 0f;
            _anchoringStartPosition = Motor.TransientPosition;
            _anchoringStartRotation = Motor.TransientRotation;
        }
    }
    private Vector3 _ladderTargetPosition;
    private Quaternion _ladderTargetRotation;
    private float _onLadderSegmentState = 0;
    private float _anchoringTimer = 0f;
    private Vector3 _anchoringStartPosition = Vector3.zero;
    private Quaternion _anchoringStartRotation = Quaternion.identity;
    private Quaternion _rotationBeforeClimbing = Quaternion.identity;


    private void Start()
    {

        // Assign the characterController to the motor
        Motor.CharacterController = this;

        // Handle initial state
        TransitionToState(CharacterState.Default);

        // Get animator
        _animator = GetComponentInChildren<Animator>();

    }

    /// <summary>
    /// Handles movement state transitions and enter/exit callbacks
    /// </summary>
    public void TransitionToState(CharacterState newState)
    {
        CharacterState tmpInitialState = CurrentCharacterState;
        OnStateExit(tmpInitialState, newState);
        CurrentCharacterState = newState;
        OnStateEnter(newState, tmpInitialState);
    }

    /// <summary>
    /// Event when entering a state
    /// </summary>
    public void OnStateEnter(CharacterState state, CharacterState fromState)
    {
        // Debug the current state
        //Debug.Log("Entering state: " + state.ToString());
        switch (state)
        {
            case CharacterState.Default:
                {
                    Motor.SetMovementCollisionsSolvingActivation(true);
                    Motor.SetGroundSolvingActivation(true);

                    break;
                }

            case CharacterState.Interacting:
                {
                    Motor.SetMovementCollisionsSolvingActivation(false);
                    Motor.SetGroundSolvingActivation(false);

                    _animator.SetBool("isInteracting", true);
                    break;
                }
            case CharacterState.Attacking:
                {
                    Motor.SetMovementCollisionsSolvingActivation(false);
                    Motor.SetGroundSolvingActivation(false);

                    _animator.SetBool("holdingAttack1", true);

                    //_animator.SetBool("isPunching", true);
                    _animator.CrossFade(comboList[currentCombo], 0.1f);
                    break;
                }
            case CharacterState.Climbing:
                    {
                        _rotationBeforeClimbing = Motor.TransientRotation;

                        Motor.SetMovementCollisionsSolvingActivation(false);
                        Motor.SetGroundSolvingActivation(false);
                        _climbingState = ClimbingState.Anchoring;

                        // Store the target position and rotation to snap to
                        _ladderTargetPosition = _activeLadder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);
                        _ladderTargetRotation = _activeLadder.transform.rotation;
                        break;
                    }
        }
    }

    /// <summary>
    /// Event when exiting a state
    /// </summary>
    public void OnStateExit(CharacterState state, CharacterState toState)
    {
        switch (state)
        {
            case CharacterState.Default:
                {
                    break;
                }
            case CharacterState.Interacting:
                {

                    _animator.SetBool("isInteracting", false);

                    break;
                }
        }
    }

    /// <summary>
    /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
    /// </summary>
    public void SetInputs(ref PlayerCharacterInputs inputs)
    {
        // Check for interaction input (works for ladders and other interactables)
        if (inputs.InteractDown)
        {
            if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders, InteractionLayer, QueryTriggerInteraction.Collide) > 0)
            {
                if (_probedColliders[0] != null)
                {
                    // Handle ladders
                    MyLadder ladder = _probedColliders[0].gameObject.GetComponent<MyLadder>();
                    if (ladder)
                    {
                        // Transition to ladder climbing state
                        if (CurrentCharacterState == CharacterState.Default)
                        {
                            _activeLadder = ladder;
                            TransitionToState(CharacterState.Climbing);
                        }
                        // Transition back to default movement state
                        else if (CurrentCharacterState == CharacterState.Climbing)
                        {
                            _climbingState = ClimbingState.DeAnchoring;
                            _ladderTargetPosition = Motor.TransientPosition;
                            _ladderTargetRotation = _rotationBeforeClimbing;
                        }
                    }

                    // Handle other interactables
                    else if (_probedColliders[0].gameObject.GetComponent<Interactable>())
                    {
                        //TransitionToState(CharacterState.Interacting);
                    }
                }
            }
        }
        _ladderUpDownInput = inputs.MoveAxisForward;

        // Check for pressing movement keys. EWsto úsase solo para o animador, para transicionar a animación de idle a run
        // TODO: esto hai que eliminalo e cambiar a transición de idle -> andar -> correr mediante a velocidade que leve o menda
        IsMovementPressed = inputs.IsMovementPressed;

        // Clamp input
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

        // Calculate camera direction and rotation on the character plane
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

        // Check para comprobar si se mant'en o boton pulsado. A logica con esta variable esta no componente animator
        // Os checks de manter botons pulsados poden ir fora dos estados, por seguridade
        // Se ves unha forma mellor, cambiao sen fallo
        if (!inputs.Attack1Held)
        {
            _animator.SetBool("holdingAttack1", false);
        }

        switch (CurrentCharacterState)
        {
            // Grounded + Airborne
            case CharacterState.Default:
                {
                    // Move and look inputs
                    _moveInputVector = cameraPlanarRotation * moveInputVector;

                    switch (OrientationMethod)
                    {
                        case OrientationMethod.TowardsCamera:
                            _lookInputVector = cameraPlanarDirection;
                            break;
                        case OrientationMethod.TowardsMovement:
                            _lookInputVector = _moveInputVector.normalized;
                            break;
                    }

                    // Jumping input
                    if (inputs.JumpDown)
                    {
                        _timeSinceJumpRequested = 0f;
                        _jumpRequested = true;
                    }

                    // Crouching input
                    if (inputs.CrouchDown)
                    {
                        _shouldBeCrouching = true;

                        if (!_isCrouching)
                        {
                            _isCrouching = true;
                            Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                            MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
                        }
                    }
                    else if (inputs.CrouchUp)
                    {
                        _shouldBeCrouching = false;
                    }

                    // Interacting input
                    if (inputs.InteractDown)
                    {
                        //TransitionToState(CharacterState.Interacting);
                    }

                    if (inputs.Attack1Down)
                    {
                        TransitionToState(CharacterState.Attacking);
                    }

                    break;
                }

            case CharacterState.Interacting:
                {
                    break;
                }
            
            case CharacterState.Attacking:
                {
                    if (canAttack && inputs.Attack1Down)
                    {   
                        // ao pulsar o boton empeza a contar como held, hasta que se solte
                        _animator.SetBool("holdingAttack1", true);
                        // iniciar a animacion
                        _animator.CrossFade(comboList[currentCombo], 0.1f);
                    }
                    break;
                }
        }
    }

    

    /// <summary>
    /// This is called every frame by the AI script in order to tell the character what its inputs are
    /// </summary>
    // public void SetInputs(ref AICharacterInputs inputs)
    // {
    //     _moveInputVector = inputs.MoveVector;
    //     _lookInputVector = inputs.LookVector;
    // }

    //private Quaternion _tmpTransientRot;

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>
    public void BeforeCharacterUpdate(float deltaTime)
    {
        // Handle animation calls
        switch (CurrentCharacterState)
        {
            // Grounded + Airborne
            case CharacterState.Default:
                {
                    // Grounded
                    if (Motor.GroundingStatus.IsStableOnGround)
                    {
                        _animator.SetBool("isFalling", false);

                        if (IsMovementPressed)
                        {
                            _animator.SetBool("isRunning", true);
                        }
                        else
                        {
                            _animator.SetBool("isRunning", false);
                        }

                        if (_jumpRequested)
                        {
                            _animator.SetBool("isJumping", true);
                        }
                        else
                        {
                            _animator.SetBool("isJumping", false);
                        }
                    }
                    // Airborne
                    else
                    {
                        _animator.SetBool("isFalling", true);
                    }
                    break;
                }
            case CharacterState.Interacting:
                {
                    //_animator.SetBool("isInteracting", true);
                    break;
                }
        }

        handleInteract();
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its rotation should be right now. 
    /// This is the ONLY place where you should set the character's rotation
    /// </summary>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        switch (CurrentCharacterState)
        {
            // Grounded + Airborne
            case CharacterState.Default:
                {
                    if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
                    {
                        // Smoothly interpolate from current to target look direction
                        Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                        // Set the current rotation (which will be used by the KinematicCharacterMotor)
                        currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                    }

                    Vector3 currentUp = (currentRotation * Vector3.up);
                    if (BonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                    {
                        // Rotate from current up to invert gravity
                        Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    }
                    else if (BonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);

                            Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

                            // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                            Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.Capsule.radius));
                        }
                        else
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                        }
                    }
                    else
                    {
                        Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    }
                    break;
                }

            case CharacterState.Interacting:
                {
                    Motor.SetRotation(InteractableRotation);
                    break;
                }

            case CharacterState.Attacking:
                {
                    //currentRotation = _tmpTransientRot;
                    break;
                }
            case CharacterState.Climbing:
                    {
                        switch (_climbingState)
                        {
                            case ClimbingState.Climbing:
                                currentRotation = _activeLadder.transform.rotation;
                                break;
                            case ClimbingState.Anchoring:
                            case ClimbingState.DeAnchoring:
                                currentRotation = Quaternion.Slerp(_anchoringStartRotation, _ladderTargetRotation, (_anchoringTimer / AnchoringDuration));
                                break;
                        }
                        break;
                    } 
        }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now. 
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (CurrentCharacterState)
        {
            // Grounded + Airborne
            case CharacterState.Default:
                {
                    // Ground movement
                    if (Motor.GroundingStatus.IsStableOnGround)
                    {
                        float currentVelocityMagnitude = currentVelocity.magnitude;

                        Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

                        // Reorient velocity on slope
                        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                        // Calculate target velocity
                        Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                        Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                        Vector3 targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                        // Smooth movement Velocity
                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
                    }
                    // Air movement
                    else
                    {
                        // Add move input
                        if (_moveInputVector.sqrMagnitude > 0f)
                        {
                            Vector3 addedVelocity = _moveInputVector * AirAccelerationSpeed * deltaTime;

                            Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                            // Limit air velocity from inputs
                            if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
                            {
                                // clamp addedVel to make total vel not exceed max vel on inputs plane
                                Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, MaxAirMoveSpeed);
                                addedVelocity = newTotal - currentVelocityOnInputsPlane;
                            }
                            else
                            {
                                // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                                if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                                {
                                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                                }
                            }

                            // Prevent air-climbing sloped walls
                            if (Motor.GroundingStatus.FoundAnyGround)
                            {
                                if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                                {
                                    Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                                }
                            }

                            // Apply added velocity
                            currentVelocity += addedVelocity;
                        }

                        // Gravity
                        currentVelocity += Gravity * deltaTime;

                        // Drag
                        currentVelocity *= (1f / (1f + (Drag * deltaTime)));
                    }

                    // Handle jumping
                    _jumpedThisFrame = false;
                    _timeSinceJumpRequested += deltaTime;
                    if (_jumpRequested)
                    {
                        // See if we actually are allowed to jump
                        if (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
                        {
                            // Calculate jump direction before ungrounding
                            Vector3 jumpDirection = Motor.CharacterUp;
                            if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                            {
                                jumpDirection = Motor.GroundingStatus.GroundNormal;
                            }

                            // Makes the character skip ground probing/snapping on its next update. 
                            // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                            Motor.ForceUnground();

                            // Add to the return velocity and reset jump state
                            currentVelocity += (jumpDirection * JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                            currentVelocity += (_moveInputVector * JumpScalableForwardSpeed);
                            _jumpRequested = false;
                            _jumpConsumed = true;
                            _jumpedThisFrame = true;
                        }
                    }

                    // Take into account additive velocity
                    if (_internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity += _internalVelocityAdd;
                        _internalVelocityAdd = Vector3.zero;
                    }
                    break;
                }

            case CharacterState.Interacting:
                {
                    currentVelocity = Vector3.zero;
                    // TODO: cambiar o setposition por un lerp, debo mirar como esta implemenmtado no caso das escaleiras
                    Motor.SetPosition(InteractablePosition);
                    break;
                }

            case CharacterState.Attacking:
                {   
                    // Aqui o interesante sería ter unha lista de ataques con velocidades distintas para cada un, de forma que
                    // cada ataqeu te mova de forma distinta segun o que sea, por jemplo, un stinger movete a dios para adiante
                    // o seu sería implementar unha lista de ataques e que cada elemento desa lista teña algo rollo
                    // moveSpeed, knockback, knockbackDir, damage, etc
                    currentVelocity = Vector3.zero;
                    break;
                }
            case CharacterState.Climbing:
                    {
                        currentVelocity = Vector3.zero;

                        switch (_climbingState)
                        {
                            case ClimbingState.Climbing:
                                currentVelocity = (_ladderUpDownInput * _activeLadder.transform.up).normalized * ClimbingSpeed;
                                break;
                            case ClimbingState.Anchoring:
                            case ClimbingState.DeAnchoring:
                                Vector3 tmpPosition = Vector3.Lerp(_anchoringStartPosition, _ladderTargetPosition, (_anchoringTimer / AnchoringDuration));
                                currentVelocity = Motor.GetVelocityForMovePosition(Motor.TransientPosition, tmpPosition, deltaTime);
                                break;
                        }
                        break;
                    }
        }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime)
    {
        switch (CurrentCharacterState)
        {
            case CharacterState.Default:
                {
                    // Handle jump-related values
                    {
                        // Handle jumping pre-ground grace period
                        if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
                        {
                            _jumpRequested = false;
                        }

                        if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
                        {
                            // If we're on a ground surface, reset jumping values
                            if (!_jumpedThisFrame)
                            {
                                _jumpConsumed = false;
                            }
                            _timeSinceLastAbleToJump = 0f;
                        }
                        else
                        {
                            // Keep track of time since we were last able to jump (for grace period)
                            _timeSinceLastAbleToJump += deltaTime;
                        }
                    }

                    // Handle uncrouching
                    if (_isCrouching && !_shouldBeCrouching)
                    {
                        // Do an overlap test with the character's standing height to see if there are any obstructions
                        Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                        if (Motor.CharacterOverlap(
                            Motor.TransientPosition,
                            Motor.TransientRotation,
                            _probedColliders,
                            Motor.CollidableLayers,
                            QueryTriggerInteraction.Ignore) > 0)
                        {
                            // If obstructions, just stick to crouching dimensions
                            Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                        }
                        else
                        {
                            // If no obstructions, uncrouch
                            MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                            _isCrouching = false;
                        }
                    }
                    break;
                }
            case CharacterState.Interacting:
                {
                    break;
                }
            case CharacterState.Attacking:
                {
                    break;
                }
            case CharacterState.Climbing:
                    {
                        switch (_climbingState)
                        {
                            case ClimbingState.Climbing:
                                // Detect getting off ladder during climbing
                                _activeLadder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);
                                if (Mathf.Abs(_onLadderSegmentState) > 0.05f)
                                {
                                    _climbingState = ClimbingState.DeAnchoring;

                                    // If we're higher than the ladder top point
                                    if (_onLadderSegmentState > 0)
                                    {
                                        _ladderTargetPosition = _activeLadder.TopReleasePoint.position;
                                        _ladderTargetRotation = _activeLadder.TopReleasePoint.rotation;
                                    }
                                    // If we're lower than the ladder bottom point
                                    else if (_onLadderSegmentState < 0)
                                    {
                                        _ladderTargetPosition = _activeLadder.BottomReleasePoint.position;
                                        _ladderTargetRotation = _activeLadder.BottomReleasePoint.rotation;
                                    }
                                }
                                break;
                            case ClimbingState.Anchoring:
                            case ClimbingState.DeAnchoring:
                                // Detect transitioning out from anchoring states
                                if (_anchoringTimer >= AnchoringDuration)
                                {
                                    if (_climbingState == ClimbingState.Anchoring)
                                    {
                                        _climbingState = ClimbingState.Climbing;
                                    }
                                    else if (_climbingState == ClimbingState.DeAnchoring)
                                    {
                                        TransitionToState(CharacterState.Default);
                                    }
                                }

                                // Keep track of time since we started anchoring
                                _anchoringTimer += deltaTime;
                                break;
                        }
                        break;
                    }
        }
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        // Handle landing and leaving ground
        if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
        {
            OnLanded();
        }
        else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
        {
            OnLeaveStableGround();
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        if (IgnoredColliders.Count == 0)
        {
            return true;
        }

        if (IgnoredColliders.Contains(coll))
        {
            return false;
        }

        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void AddVelocity(Vector3 velocity)
    {
        switch (CurrentCharacterState)
        {
            case CharacterState.Default:
                {
                    _internalVelocityAdd += velocity;
                    break;
                }
        }
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    protected void OnLanded()
    {
    }

    protected void OnLeaveStableGround()
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    //TODO esta merda ten que comprobarse durante o falling state, e usando a variable "_canLedgeGrab"

    // private void CheckLedgeGrabBoxCast()
    // {
    //     Vector3 boxSize = new Vector3(_ctx._ledgeGrabSize.x, 0.05f, _ctx._ledgeGrabSize.y);
    //     RaycastHit headHit;
    //     RaycastHit torsoHit;

    //     // Player layer is 3, thus ignore it when casting
    //     // int layerMask = 1 << 3;
    //     // layerMask = ~layerMask;
        
    //     bool head = Physics.BoxCast(_ctx._headRaycast.position, boxSize, _ctx.transform.forward, out headHit, _ctx.transform.rotation, _ctx._ledgeGrabDistance, Physics.AllLayers);
    //     bool torso = Physics.BoxCast(_ctx._torsoRaycast.position, boxSize, _ctx.transform.forward, out torsoHit, _ctx.transform.rotation, _ctx._ledgeGrabDistance, Physics.AllLayers);

    //     if (!head)
    //     {
    //         if (torso)
    //         {
    //             // Get the ledge grab position and direction from the torso hit
    //             // and apply offset to the ledge grab position
    //             _ctx._ledgeGrabPosition = torsoHit.point;
    //             _ctx._ledgeGrabDirection = torsoHit.normal;
    //             _ctx._ledgeGrabPosition += _ctx._ledgeGrabDirection * _ctx._ledgeGrabOffset;

    //             // Switch to the ledge grab state
    //             SwitchState(_factory.LedgeGrab());
    //         }
    //     }
        
    // }

    //TODO toda esta merda vai no estado "LedgGrab", hai que converrtilo todo á nova lógica de movemento

    // public void HandleGravity()
    // {
    //     float previousYVelocity = _ctx._currentMovement.y;
    //     _ctx._currentMovement.y = 0f;
    //     _ctx._appliedMovement.y = 0f;
    // }

    // public override void EnterState()
    // {
    //     InitializeSubState();
    //     _ctx._animator.SetBool("isLedgeGrabbing", true);
    //     _ctx._canMove = false;

    //     //Rotate the player to face the ledge
    //     _ctx.transform.rotation = Quaternion.LookRotation(-_ctx._ledgeGrabDirection, Vector3.up);
    //     //Move the player to the ledge grab position, which is the ledge grab raycast hit point plus an offset to prevent the player from clipping into the ledge
    //     _ctx.transform.position = _ctx.transform.position + (_ctx._ledgeGrabDirection * _ctx._ledgeGrabOffset);

    // }

    // public override void ExitState()
    // {
    //     _ctx._animator.SetBool("isLedgeGrabbing", false);
    //     _ctx._canMove = true;
    //     //Disable the raycast for a certain amount of time in order to prevent the player from grabbing the same ledge again
    //     _ctx.StartCoroutine(WaitToReEnableLedgeGrab());
    // }

    //     public IEnumerator WaitToReEnableLedgeGrab()
    // {
    //     _ctx._canLedgeGrab = false;
    //     yield return new WaitForSeconds(0.5f);
    //     _ctx._canLedgeGrab = true;
    // }


    void handleInteract()
    {
        // 1. raycast forward
        Debug.DrawRay(transform.position + new Vector3(0, 1, 0), transform.forward * 3, Color.red);
        RaycastHit hit;
        Ray r = new Ray(transform.position + new Vector3(0, 1, 0), transform.forward * 3);
        float distance = 3f;

        if (Physics.Raycast(r, out hit, distance))
        {
            // 2. if we hit an interactable object, display the interactable UI
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.EnableInteract();
            }
        }
    }


    /// mover a una clase aparte en la que se guarden todas las acciones
    public IEnumerator PerformAction(string actionName, Vector3 moveTo, Vector3 newRotation, AudioClip playAudio)
    {
        // plays an animation and restores to normal state afterwards
        Vector3 oldPosition = transform.position;

        TransitionToState(CharacterState.Interacting);

        //_gravity = 0;
        //_canMove = false;

        //Motor.SetPosition(moveTo);
        //Motor.SetRotation(Quaternion.Euler(newRotation));
        InteractablePosition = moveTo;
        InteractableRotation = Quaternion.Euler(newRotation);

        _animator.Play(actionName);
        Debug.Log(playAudio);
        GameManager.instance.audioSource.PlayOneShot(playAudio);
        
        yield return new WaitForSeconds(5);

        //TransitionToState(CharacterState.Default);

        Debug.Log("resetting");
        //_gravity = -9.8f;
        //_canMove = true;
        //Motor.SetPosition(oldPosition);

        //_animator.Play("Idle");

        // if audio is playing, stop it
        if (GameManager.instance.audioSource.isPlaying)
        {
            GameManager.instance.audioSource.Stop();
        }
    }

}