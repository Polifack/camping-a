using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public PlayerInput _playerInput;
    [SerializeField] public CharacterController _characterController;
    [SerializeField] public Animator _animator;

    [Header("Movement")]
    [SerializeField] public bool _canMove = true;
    [SerializeField] public Vector2 _currentMovementInput;
    [SerializeField] public Vector3 _currentMovement;
    [SerializeField] public Vector3 _appliedMovement;
    [SerializeField] public Vector3 _cameraRelativeMovement;
    [SerializeField] public bool _isMovementPressed;
    [SerializeField] public float _speedMultiplier = 15f;
    [SerializeField] public float _rotationFactorPerFrame = 15f;

    [Header("Ledge Grab")]
    [SerializeField] public Transform _headRaycast;
    [SerializeField] public Transform _torsoRaycast;
    [SerializeField] public bool _canLedgeGrab = true;
    [SerializeField] public Vector2 _ledgeGrabSize = new Vector2(0.3f, 0f);
    [SerializeField] public float _ledgeGrabDistance = 0.6f;
    [SerializeField] public float _ledgeGrabOffset = 0.1f;
    [SerializeField] public Vector3 _ledgeGrabPosition;
    [SerializeField] public Vector3 _ledgeGrabDirection;


    [Header("Gravity")]
    [SerializeField] public float _gravity = -9.8f;
    [SerializeField] public bool _isFalling = false;
    [SerializeField] public float _fallMultiplier = 2.0f;
    [SerializeField] public bool _isGrounded;


    [Header("Jumping")]
    [SerializeField] public float _maxJumpHeight = 1f;
    [SerializeField] public float _maxJumpTime = 0.75f;
    public float _initialJumpVelocity;
    [SerializeField] public bool _isJumpPressed = false;
    [SerializeField] public bool _isJumping = false;
    [SerializeField] public bool _requiredNewJumpPress = false;


    [Header("Attack")]
    [SerializeField] public bool _isAttackPressed = false;


    [Header("States")]
    [SerializeField] public PlayerBaseState _currentState;
    [SerializeField] public PlayerStateFactory _states;

    void Awake()
    {   
        //Reference variables
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        //States
        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        //Input callbacks
        //Movement
        _playerInput.CharacterControls.Move.started += onMovementInput;
        _playerInput.CharacterControls.Move.canceled += onMovementInput;
        _playerInput.CharacterControls.Move.performed += onMovementInput;
        
        //Jump
        _playerInput.CharacterControls.Jump.started += onJump;
        _playerInput.CharacterControls.Jump.canceled += onJump;

        //Attack
        _playerInput.CharacterControls.Attack.started += onAttack;
        _playerInput.CharacterControls.Attack.canceled += onAttack;

        setupJumpVariables();
    }

    void Start()
    {
        _characterController.Move(_appliedMovement * Time.deltaTime);
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
        _requiredNewJumpPress = false;
    }

    void onAttack(InputAction.CallbackContext context)
    {
        _isAttackPressed = context.ReadValueAsButton();
    }


    void handleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = _cameraRelativeMovement.x;
        positionToLookAt.y = 0f;
        positionToLookAt.z = _cameraRelativeMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }

    }

    void Update()
    {
        // _isGrounded = _characterController.isGrounded;
        // raycast towards the ground checking if distance is less than 0.1f and hitting object of layer 3 (ground)
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f, 1 << 3);
        _currentState.UpdateStates();

        if (_canMove)
        {
            //Camera relative movement
            _cameraRelativeMovement = ConvertToCameraSpace(_appliedMovement);
            _characterController.Move(_cameraRelativeMovement * _speedMultiplier * Time.deltaTime);
            
            //Rotation after calculating relative movement
            handleRotation();
        }

        //Debug
        DebugVisualizeRaycasts();
    }

    // void FixedUpdate()
    // {
    //     Vector3 boxSize = new Vector3(_ledgeGrabSize, 0.1f, _ledgeGrabSize);
    //     RaycastHit headHit;
    //     RaycastHit torsoHit;

    //     // Player layer is 3, thus ignore it when casting
    //     int layerMask = 1 << 3;
    //     layerMask = ~layerMask;

    //     bool head = Physics.BoxCast(_headRaycast.position, boxSize, transform.forward, out headHit, Quaternion.identity, _ledgeGrabSize *2f, Physics.AllLayers);
    //     bool torso = Physics.BoxCast(_torsoRaycast.position, boxSize, transform.forward, out torsoHit, Quaternion.identity, _ledgeGrabSize *2f, Physics.AllLayers);

    //     Debug.Log("Head: " + head + " Torso: " + torso);
    // }

    Vector3 ConvertToCameraSpace(Vector3 vectorToRotate)
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraForwardZProduct = vectorToRotate.z * cameraForward;
        Vector3 cameraRightXProduct = vectorToRotate.x * cameraRight;

        Vector3 vectorRotateToCameraSpace = cameraForwardZProduct + cameraRightXProduct;
        vectorRotateToCameraSpace.y = vectorToRotate.y;

        return vectorRotateToCameraSpace;
    }

    void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }

    void DebugVisualizeRaycasts()
    {
        //Debug.DrawRay(_headRaycast.position, _headRaycast.forward * _ledgeGrabSize, Color.red);
        //Debug.DrawRay(_torsoRaycast.position, _torsoRaycast.forward * _ledgeGrabSize, Color.red);

        // Visualize the box cast drawing spheres at the vertices of the box
        // Debug.DrawRay(_headRaycast.position + new Vector3(0, (0.1f / 2), 0), _headRaycast.forward * _ledgeGrabDistance, Color.red);
        // Debug.DrawRay(_headRaycast.position - new Vector3(0, (0.1f / 2), 0), _headRaycast.forward * _ledgeGrabDistance, Color.red);
        // Debug.DrawRay(_headRaycast.position + new Vector3((_ledgeGrabSize / 2), 0, 0), _headRaycast.forward * _ledgeGrabDistance, Color.red);
        // Debug.DrawRay(_headRaycast.position - new Vector3((_ledgeGrabSize / 2), 0, 0), _headRaycast.forward * _ledgeGrabDistance, Color.red);
        // Debug.DrawRay(_headRaycast.position + new Vector3(0, 0, (_ledgeGrabSize / 2)), _headRaycast.forward * _ledgeGrabDistance, Color.red);
        // Debug.DrawRay(_headRaycast.position - new Vector3(0, 0, (_ledgeGrabSize / 2)), _headRaycast.forward * _ledgeGrabDistance, Color.red);
        Vector3 boxSize = new Vector3(_ledgeGrabSize.x, 0.05f, _ledgeGrabSize.y);

        DrawBoxCastBox(_headRaycast.position, boxSize, transform.rotation, transform.forward, _ledgeGrabDistance, Color.red);
        DrawBoxCastBox(_torsoRaycast.position, boxSize, transform.rotation, transform.forward, _ledgeGrabDistance, Color.blue);

        // Debug.DrawRay(_torsoRaycast.position + new Vector3(0, (0.1f / 2), 0), _torsoRaycast.forward * _ledgeGrabDistance, Color.blue);
        // Debug.DrawRay(_torsoRaycast.position - new Vector3(0, (0.1f / 2), 0), _torsoRaycast.forward * _ledgeGrabDistance, Color.blue);
        // Debug.DrawRay(_torsoRaycast.position + new Vector3((_ledgeGrabSize / 2), 0, 0), _torsoRaycast.forward * _ledgeGrabDistance, Color.blue);
        // Debug.DrawRay(_torsoRaycast.position - new Vector3((_ledgeGrabSize / 2), 0, 0), _torsoRaycast.forward * _ledgeGrabDistance, Color.blue);
        // Debug.DrawRay(_torsoRaycast.position + new Vector3(0, 0, (_ledgeGrabSize / 2)), _torsoRaycast.forward * _ledgeGrabDistance, Color.blue);
        // Debug.DrawRay(_torsoRaycast.position - new Vector3(0, 0, (_ledgeGrabSize / 2)), _torsoRaycast.forward * _ledgeGrabDistance, Color.blue);


        


    }

//Draws the full box from start of cast to its end distance. Can also pass in hitInfoDistance instead of full distance
	public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color)
	{
		direction.Normalize();
		Box bottomBox = new Box(origin, halfExtents, orientation);
		Box topBox = new Box(origin + (direction * distance), halfExtents, orientation);
			
		Debug.DrawLine(bottomBox.backBottomLeft, topBox.backBottomLeft,	color);
		Debug.DrawLine(bottomBox.backBottomRight, topBox.backBottomRight, color);
		Debug.DrawLine(bottomBox.backTopLeft, topBox.backTopLeft, color);
		Debug.DrawLine(bottomBox.backTopRight, topBox.backTopRight,	color);
		Debug.DrawLine(bottomBox.frontTopLeft, topBox.frontTopLeft,	color);
		Debug.DrawLine(bottomBox.frontTopRight, topBox.frontTopRight, color);
		Debug.DrawLine(bottomBox.frontBottomLeft, topBox.frontBottomLeft, color);
		Debug.DrawLine(bottomBox.frontBottomRight, topBox.frontBottomRight,	color);
	
		DrawBox(bottomBox, color);
		DrawBox(topBox, color);
	}
	
	public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color)
	{
		DrawBox(new Box(origin, halfExtents, orientation), color);
	}
	public static void DrawBox(Box box, Color color)
	{
		Debug.DrawLine(box.frontTopLeft,	 box.frontTopRight,	color);
		Debug.DrawLine(box.frontTopRight,	 box.frontBottomRight, color);
		Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color);
		Debug.DrawLine(box.frontBottomLeft,	 box.frontTopLeft, color);
												 
		Debug.DrawLine(box.backTopLeft,		 box.backTopRight, color);
		Debug.DrawLine(box.backTopRight,	 box.backBottomRight, color);
		Debug.DrawLine(box.backBottomRight,	 box.backBottomLeft, color);
		Debug.DrawLine(box.backBottomLeft,	 box.backTopLeft, color);
												 
		Debug.DrawLine(box.frontTopLeft,	 box.backTopLeft, color);
		Debug.DrawLine(box.frontTopRight,	 box.backTopRight, color);
		Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color);
		Debug.DrawLine(box.frontBottomLeft,	 box.backBottomLeft, color);
	}
	
	public struct Box
	{
		public Vector3 localFrontTopLeft     {get; private set;}
		public Vector3 localFrontTopRight    {get; private set;}
		public Vector3 localFrontBottomLeft  {get; private set;}
		public Vector3 localFrontBottomRight {get; private set;}
		public Vector3 localBackTopLeft      {get {return -localFrontBottomRight;}}
		public Vector3 localBackTopRight     {get {return -localFrontBottomLeft;}}
		public Vector3 localBackBottomLeft   {get {return -localFrontTopRight;}}
		public Vector3 localBackBottomRight  {get {return -localFrontTopLeft;}}

		public Vector3 frontTopLeft     {get {return localFrontTopLeft + origin;}}
		public Vector3 frontTopRight    {get {return localFrontTopRight + origin;}}
		public Vector3 frontBottomLeft  {get {return localFrontBottomLeft + origin;}}
		public Vector3 frontBottomRight {get {return localFrontBottomRight + origin;}}
		public Vector3 backTopLeft      {get {return localBackTopLeft + origin;}}
		public Vector3 backTopRight     {get {return localBackTopRight + origin;}}
		public Vector3 backBottomLeft   {get {return localBackBottomLeft + origin;}}
		public Vector3 backBottomRight  {get {return localBackBottomRight + origin;}}

		public Vector3 origin {get; private set;}

		public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
		{
			Rotate(orientation);
		}
		public Box(Vector3 origin, Vector3 halfExtents)
		{
			this.localFrontTopLeft     = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
			this.localFrontTopRight    = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
			this.localFrontBottomLeft  = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
			this.localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);

			this.origin = origin;
		}


		public void Rotate(Quaternion orientation)
		{
			localFrontTopLeft     = RotatePointAroundPivot(localFrontTopLeft    , Vector3.zero, orientation);
			localFrontTopRight    = RotatePointAroundPivot(localFrontTopRight   , Vector3.zero, orientation);
			localFrontBottomLeft  = RotatePointAroundPivot(localFrontBottomLeft , Vector3.zero, orientation);
			localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
		}
	}

	 //This should work for all cast types
	static Vector3 CastCenterOnCollision(Vector3 origin, Vector3 direction, float hitInfoDistance)
	{
		return origin + (direction.normalized * hitInfoDistance);
	}
	
	static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
	{
		Vector3 direction = point - pivot;
		return pivot + rotation * direction;
	}

}
