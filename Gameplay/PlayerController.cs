using UnityEngine;
using UnityEngine.InputSystem;

public enum PlatformType {
    PC,
    Mobile
}

[System.Serializable]
public struct MovementSettings {
    public float speedWalk;
    public float speedRun;
    public float speedCrouch;
    public float speedCrawl;
    public float speedSlide;
    public float jumpPower;
    public float jumpCountMax;
    public float jumpCount;
    public float gravity;

    public float glideFall;
    public float speedGlideMove;
    public float glideControl;

    public float mouseSens;
    public float slideDuration;
    public float slideCooldown;
    public float slideSpeedDecay;
}

public class PlayerController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CapsuleCollider _coll;
    [SerializeField] private Transform _cameraTransform;

    [Header("Settings")]
    [SerializeField]
    private MovementSettings _movementSettings = new MovementSettings {
        speedWalk = 3f,
        speedRun = 6f,
        speedCrouch = 1.5f,
        speedCrawl = 1f,
        speedSlide = 8f,
        mouseSens = 0.5f,
        jumpPower = 2f,
        jumpCount = 2f,
        jumpCountMax = 2f,
        gravity = -9.81f,
        glideFall = -2f,
        speedGlideMove = 4f,
        glideControl = 2f,
        slideDuration = 1f,
        slideCooldown = 1.5f,
        slideSpeedDecay = 0.8f
    };

    private float heightStand = 2f;
    private float heightCrouch = 1f;
    private float heightCrawl = 0.5f;
    private float heightSpeed = 5f;


    private readonly Vector3 _vectorUp = Vector3.up;
    private readonly Vector3 _vectorRight = Vector3.right;
    private readonly Vector3 _vectorForward = Vector3.forward;
    private readonly Vector2 _vector2Zero = Vector2.zero;
    private readonly Vector3 _vector3Zero = Vector3.zero;
    private const float GROUNDED_VELOCITY_Y = -2f;
    private const float CAMERA_PITCH_MIN = -90f;
    private const float CAMERA_PITCH_MAX = 90f;
    private const float INPUT_DEADZONE = 0.1f;
    private const float STAMINA_MAX = 100f;
    
    private const float CAMERA_HEIGHT_MULTIPLIER = 0.6f;
    private const float CAMERA_HEIGHT_MIN_MULTIPLIER = -0.6f;
    private const float CAMERA_HEIGHT_MAX_MULTIPLIER = 0.3f;
    private const float STANCE_HEIGHT_THRESHOLD = 0.01f;
    private const float STUN_SPEED_MULTIPLIER = 0.3f;
    private const float STUN_DAMAGE_THRESHOLD = 20f;
    private const float STUN_DURATION = 0.5f;
    private const float RUN_SPEED_THRESHOLD = 0.1f;
    private const float MOUSE_SENSITIVITY_MULTIPLIER = 100f;
    private const float CAMERA_TILT_DIVISOR = 45f;
    private const float CAMERA_TILT_THRESHOLD = 35f;
    private const float GLIDE_SPEED_BOOST_MULTIPLIER = 0.8f;
    private const float GLIDE_SPEED_REDUCE_MULTIPLIER = 0.3f;
    private const float GLIDE_FALL_MULTIPLIER_DIVE = 8f;
    private const float GLIDE_FALL_MULTIPLIER_NORMAL = 4f;
    private const float GLIDE_GRAVITY_MULTIPLIER_DIVE = 0.75f;
    private const float GLIDE_GRAVITY_MULTIPLIER_NORMAL = 0.65f;
    private const float JETPACK_VELOCITY_TARGET = 5f;
    private const float JETPACK_VELOCITY_SPEED = 5f;
    private const float GRAVITY_MULTIPLIER_GLIDE = 0.5f;

    public bool IsFisgunRotActive;
    private Vector2 _moveInput;
    private Vector2 _mouseInput;
    
    [Header("Platform Settings")]
    [SerializeField] private PlatformType _platformType = PlatformType.PC;
    public PlatformType CurrentPlatform => _platformType;
    
    [Header("Mobile Input Settings")]
    [SerializeField] private RectTransform _cameraControlPanel;
    [SerializeField] private Camera _uiCamera;
    [SerializeField] private JoystickBase _joystick;
    [SerializeField] private bool _useJoystick = true;
    private int _rightTouchId = -1;
    private Vector2 _lookInput = Vector2.zero;
    public bool IsMobileUIEnabled = true;

    [SerializeField] private Vector3 _velocity;
    private Vector3 _moveDirection;
    private Vector3 _tempHorizontalVelocity;
    private Vector3 _tempCameraForward;
    private Vector3 _tempCameraRight;
    private float _cameraPitch;
    [SerializeField] private float _currentSpeed;

    private bool _wasGrounded;

    private bool _isGliding;
    private bool _isJetpackUsed;


    [Header("Camera Settings")]
    [SerializeField] private float _cameraSmoothness = 5f;
    [SerializeField] private bool _invertY = false;
    [SerializeField] private bool _invertX = false;

    private float _targetCameraPitch;

    public enum PlayerStance { Standing, Crouching, Crawl, Gliding, Simming, Noclip, }
    private PlayerStance _currentStance = PlayerStance.Standing;
    private float _targetHeight;

    private bool _isSliding = false;
    private float _slideTimer = 0f;
    private float _slideCooldownTimer = 0f;
    private Vector3 _slideDirection;
    private float _currentSlideSpeed;

    private InputAction _jumpAction;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _runAction;
    private InputAction _crouchAction;
    private InputAction _crawlAction;
    private InputAction _slideAction;
    private InputAction _glideAction;
    private InputAction _jetpackAction;
    private InputAction _attackAction;
    private InputAction _reloadAction;


    public GameObject ObjGlider;
    public GameObject ObjJetpack;

    public bool IsSliding => _isSliding;
    public bool IsGliding => _isGliding;
    public PlayerStance CurrentStance => _currentStance;
    public float CurrentSpeed => _currentSpeed;
    public bool IsGrounded => _characterController.isGrounded;
    private HealthSystem _health;
    private StaminaSystem _stamina;
    private InventoryController _inventoryController;
    private PlayerAssembler PlayerAssembler;
    

    #region initializate
        private void Awake() {
            if (_characterController == null) _characterController = GetComponent<CharacterController>();

            DetectPlatform();
            
            var playerActions = InputManager.Instance.actions.Player;
            _jumpAction = playerActions.Jump;
            _moveAction = playerActions.Move;
            _lookAction = playerActions.Look;
            _runAction = playerActions.Run;
            _crouchAction = playerActions.Crouch;
            _crawlAction = playerActions.Crawl;
            _slideAction = playerActions.Slide;
            _glideAction = playerActions.Glide;
            _jetpackAction = playerActions.Jetpack;
            _attackAction = playerActions.Attack;
            _reloadAction = playerActions.Reload;
            
            _health = GetComponent<HealthSystem>();
            _stamina = GetComponent<StaminaSystem>();
            
            _health.OnDeath += OnPlayerDeath;
            _health.OnDamageTaken += OnDamageTaken;
            
            _stamina.OnExhaustedStateChanged += OnExhaustedChanged;
            PlayerAssembler = FindAnyObjectByType<PlayerAssembler>();

            InitializeStance();
        }
        
        private void DetectPlatform() {
            #if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
                _platformType = PlatformType.Mobile;
            #else
                _platformType = PlatformType.PC;
            #endif
        }
        
        private void OnPlayerDeath() {
            DisableMovement();
            enabled = false;
            
            Debug.Log("Player Died!");
        }
        private void OnDamageTaken(float damage) {
            if (damage > STUN_DAMAGE_THRESHOLD) 
                StartCoroutine(StunEffect(STUN_DURATION));
        }
        private System.Collections.IEnumerator StunEffect(float duration) {
            float originalSpeed = _currentSpeed;
            _currentSpeed *= STUN_SPEED_MULTIPLIER;
            yield return new WaitForSeconds(duration);
            _currentSpeed = originalSpeed;
        }
        
        private void OnExhaustedChanged(bool isExhausted) {
            if (isExhausted){
                if (_isGliding) EndGlide();
                if (_currentSpeed >= _movementSettings.speedRun - RUN_SPEED_THRESHOLD) 
                    _currentSpeed = _movementSettings.speedWalk * _stamina._settings.exhaustedSpeedMultiplier;
            }
        }
        
        private void OnEnable() => SubscribeToInputActions();
        private void OnDisable() => UnsubscribeFromInputActions();
        public void DisableMovement() {
            _moveInput = _vector2Zero;
            _mouseInput = _vector2Zero;
            _currentSpeed = 0f;
            if (_isGliding) EndGlide();
        }
        public void EnableMovement() => _currentSpeed = _movementSettings.speedWalk;
        private void SubscribeToInputActions() {
            if (_platformType != PlatformType.PC) return;
            _jumpAction.performed += OnJump;
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMoveCanceled;
            _lookAction.performed += OnLook;
            _lookAction.canceled += OnLookCanceled;
            _runAction.performed += OnRun;
            _runAction.canceled += OnRunCanceled;
            _crouchAction.performed += OnCrouch;
            _crawlAction.performed += OnCrawl;
            _slideAction.performed += OnSlide;
            _glideAction.performed += OnGlideStart;
            _glideAction.canceled += OnGlideEnd;
            _jetpackAction.performed += OnJetpackStart;
            _jetpackAction.canceled += OnJetpackEnd;
            _reloadAction.performed += OnReloadStart;
        }
        private void UnsubscribeFromInputActions() {
            if (_platformType != PlatformType.PC) return;
            _jumpAction.performed -= OnJump;
            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMoveCanceled;
            _lookAction.performed -= OnLook;
            _lookAction.canceled -= OnLookCanceled;
            _runAction.performed -= OnRun;
            _runAction.canceled -= OnRunCanceled;
            _crouchAction.performed -= OnCrouch;
            _crawlAction.performed -= OnCrawl;
            _slideAction.performed -= OnSlide;
            _glideAction.performed -= OnGlideStart;
            _glideAction.canceled -= OnGlideEnd;
            _jetpackAction.performed -= OnJetpackStart;
            _jetpackAction.canceled -= OnJetpackEnd;
            _reloadAction.performed -= OnReloadStart;
        }
    #endregion
    private void Update() {
        float deltaTime = Time.deltaTime;
        bool _isGrounded = _characterController.isGrounded;
        if (_isGrounded && !_wasGrounded) {
            _movementSettings.jumpCount = _movementSettings.jumpCountMax;
            if (_isGliding) EndGlide();
        }
        _wasGrounded = _isGrounded;

        if (!_isSliding && !_isGliding) HandleStance(deltaTime);
        if (_isSliding) HandleSlide(deltaTime);
        if (_isGliding) HandleGlide(deltaTime);
        if (_isJetpackUsed) HandleJetpack(deltaTime);
        
        if (_platformType == PlatformType.Mobile && _useJoystick && _joystick != null) {
            HandleJoystickMovement();
        }
        
        HandleMovement(deltaTime);
        
        if (_platformType == PlatformType.PC) {
            if (_mouseInput != _vector2Zero) HandleMouseLook(deltaTime);
        }
        else {
            if (IsMobileUIEnabled) GetTouchInput();
            HandleMobileLook(deltaTime);
        }
    }
    #region Movement
        #region Stance
            private void InitializeStance() {
                _targetHeight = heightStand;
                _currentSpeed = _movementSettings.speedWalk;
                _characterController.height = _targetHeight;
            }
            private void HandleStance(float deltaTime) {
                if (Mathf.Abs(_characterController.height - _targetHeight) > STANCE_HEIGHT_THRESHOLD) {
                    float newHeight = Mathf.Lerp(_characterController.height, _targetHeight, heightSpeed * deltaTime);
                    _characterController.height = newHeight;
                    UpdateCameraPosition(newHeight);
                }
            }
            private void UpdateCameraPosition(float newHeight) {
                Vector3 cameraPos = _cameraTransform.localPosition;
                cameraPos.y = newHeight * CAMERA_HEIGHT_MULTIPLIER;
                cameraPos.y = Mathf.Clamp(cameraPos.y, newHeight * CAMERA_HEIGHT_MIN_MULTIPLIER, newHeight * CAMERA_HEIGHT_MAX_MULTIPLIER);
                _cameraTransform.localPosition = cameraPos;
            }
            private void SetStance(PlayerStance newStance) {
                _currentStance = newStance;
                if (newStance == PlayerStance.Standing) {
                    _targetHeight = heightStand;
                    _currentSpeed = _movementSettings.speedWalk;
                }
                else if (newStance == PlayerStance.Crouching) {
                    _targetHeight = heightCrouch;
                    _currentSpeed = _movementSettings.speedCrouch;
                }
                else if (newStance == PlayerStance.Crawl) {
                    _targetHeight = heightCrawl;
                    _currentSpeed = _movementSettings.speedCrawl;
                }
            }
            public void ForceStance(PlayerStance stance) => SetStance(stance);
        #endregion
        #region Move
            private void OnMove(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();
            private void OnMoveCanceled(InputAction.CallbackContext ctx) => _moveInput = _vector2Zero;
            
            public void SetMobileMoveInput(Vector2 moveInput) {
                _moveInput = moveInput;
            }
            
            public void SetJoystickInput(float horizontal, float vertical) {
                _moveInput.x = horizontal;
                _moveInput.y = vertical;
            }
            
            private void HandleJoystickMovement() {
                if (_joystick == null) return;

                float h = _joystick.Horizontal;
                float v = _joystick.Vertical;
                
                if (Mathf.Abs(h) > 0.001f || Mathf.Abs(v) > 0.001f) {
                    _moveInput.x = h;
                    _moveInput.y = v;
                }
                else if (_moveInput.sqrMagnitude < INPUT_DEADZONE * INPUT_DEADZONE) {
                    _moveInput = _vector2Zero;
                }
                else {
                    _moveInput = _vector2Zero;
                }
            }
            
            // private float GetValue(System.Type type, string name) {
            //     var prop = type.GetProperty(name);
            //     if (prop != null) {
            //         try { return (float)prop.GetValue(_joystick); }
            //         catch { }
            //     }
            //     var field = type.GetField(name);
            //     if (field != null) {
            //         try { return (float)field.GetValue(_joystick); }
            //         catch { }
            //     }
            //     return 0f;
            // }
            private void HandleMovement(float deltaTime) {
                if (_isSliding) {
                    _moveDirection = _slideDirection * _currentSlideSpeed;
                }
                else if (_isGliding) {
                    _tempCameraForward = _cameraTransform.forward;
                    _tempHorizontalVelocity.x = _velocity.x;
                    _tempHorizontalVelocity.y = 0f;
                    _tempHorizontalVelocity.z = _velocity.z;
                    
                    _tempHorizontalVelocity.x = transform.right.x * _moveInput.x + _tempCameraForward.x * (_moveInput.y + _movementSettings.speedGlideMove);
                    _tempHorizontalVelocity.y = 0f;
                    _tempHorizontalVelocity.z = transform.right.z * _moveInput.x + _tempCameraForward.z * (_moveInput.y + _movementSettings.speedGlideMove);
                    _tempHorizontalVelocity *= _movementSettings.glideControl;
                    
                    _tempHorizontalVelocity.x += _velocity.x;
                    _tempHorizontalVelocity.z += _velocity.z;
                    _moveDirection = Vector3.ClampMagnitude(_tempHorizontalVelocity, _movementSettings.speedGlideMove);
                    _velocity.y = Mathf.MoveTowards(_velocity.y, _movementSettings.glideFall, Mathf.Abs(_movementSettings.gravity) * deltaTime * GRAVITY_MULTIPLIER_GLIDE);
                }
                else {
                    _tempCameraForward = transform.forward;
                    _tempCameraRight = _cameraTransform.right;
                    _tempCameraForward.y = 0f;
                    _tempCameraRight.y = 0f;
                    _tempCameraForward.Normalize();
                    _tempCameraRight.Normalize();
                    _moveDirection.x = (_tempCameraRight.x * _moveInput.x + _tempCameraForward.x * _moveInput.y) * _currentSpeed;
                    _moveDirection.y = 0f;
                    _moveDirection.z = (_tempCameraRight.z * _moveInput.x + _tempCameraForward.z * _moveInput.y) * _currentSpeed;
                }
                
                if (_isJetpackUsed) {
                    _velocity.y = Mathf.MoveTowards(_velocity.y, JETPACK_VELOCITY_TARGET, JETPACK_VELOCITY_SPEED * deltaTime * GRAVITY_MULTIPLIER_GLIDE);
                }
                if (_characterController.isGrounded) {
                    if (_velocity.y < 0) _velocity.y = GROUNDED_VELOCITY_Y;
                }
                else if (!_isGliding) _velocity.y += _movementSettings.gravity * deltaTime;

                _tempHorizontalVelocity = _moveDirection + _vectorUp * _velocity.y;
                _characterController.Move(_tempHorizontalVelocity * deltaTime);
            }
        #endregion
        #region Mouse/Look
            private void OnLook(InputAction.CallbackContext ctx) => _mouseInput = ctx.ReadValue<Vector2>();
            private void OnLookCanceled(InputAction.CallbackContext ctx) => _mouseInput = _vector2Zero;
            
            public void LookMobile()
            {
                IsFisgunRotActive = !IsFisgunRotActive;
            }
            
            private void HandleMouseLook(float deltaTime) {
                ProcessLookInput(_mouseInput, deltaTime, true);
                _mouseInput = _vector2Zero;
            }
            
            private void GetTouchInput() {
                if (Touchscreen.current == null || _cameraControlPanel == null) return;
                Camera cam = _uiCamera != null ? _uiCamera : Camera.main;
                if (cam == null) return;
                
                for (int i = 0; i < Touchscreen.current.touches.Count; i++) {
                    var touch = Touchscreen.current.touches[i];
                    var phase = touch.phase.ReadValue();
                    var position = touch.position.ReadValue();
                    int touchId = touch.touchId.ReadValue();
                    bool inPanel = RectTransformUtility.RectangleContainsScreenPoint(_cameraControlPanel, position, cam);

                    if (phase == UnityEngine.InputSystem.TouchPhase.Began && inPanel && _rightTouchId == -1) {
                        _rightTouchId = touchId;
                    }
                    else if ((phase == UnityEngine.InputSystem.TouchPhase.Canceled || phase == UnityEngine.InputSystem.TouchPhase.Ended) && touchId == _rightTouchId) {
                        _rightTouchId = -1;
                        _lookInput = _vector2Zero;
                    }
                    else if (phase == UnityEngine.InputSystem.TouchPhase.Moved && touchId == _rightTouchId) {
                        _lookInput = touch.delta.ReadValue() * _movementSettings.mouseSens * Time.deltaTime;
                    }
                    else if (phase == UnityEngine.InputSystem.TouchPhase.Stationary && touchId == _rightTouchId) {
                        _lookInput = _vector2Zero;
                    }
                }
            }
            
            private void HandleMobileLook(float deltaTime) {
                if (_rightTouchId != -1 && _lookInput != _vector2Zero) {
                    ProcessLookInput(_lookInput, deltaTime, false);
                    _lookInput = _vector2Zero;
                }
                else if (_mouseInput != _vector2Zero) {
                    ProcessLookInput(_mouseInput, deltaTime, true);
                    _mouseInput = _vector2Zero;
                }
            }
            
            private void ProcessLookInput(Vector2 input, float deltaTime, bool useDeltaTime) {
                if (IsFisgunRotActive) {
                    PlayerAssembler.AddPropRotation(input);
                    return;
                }
                float sensitivity = useDeltaTime ? _movementSettings.mouseSens * MOUSE_SENSITIVITY_MULTIPLIER * deltaTime : MOUSE_SENSITIVITY_MULTIPLIER;
                float xInput = input.x * sensitivity * (_invertX ? -1f : 1f);
                float yInput = input.y * sensitivity * (_invertY ? 1f : -1f);
                transform.Rotate(_vectorUp, xInput);
                _targetCameraPitch = Mathf.Clamp(_targetCameraPitch + (useDeltaTime ? yInput : -yInput), CAMERA_PITCH_MIN, CAMERA_PITCH_MAX);
                _cameraPitch = Mathf.Lerp(_cameraPitch, _targetCameraPitch, _cameraSmoothness * deltaTime);
                _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            }
            
            public void SetMobileLookInput(Vector2 lookInput) {
                _mouseInput = lookInput;
            }
        #endregion 
        #region Run
            public void MobileOnRun()
            {
                if (!_isSliding && _currentStance == PlayerStance.Standing && !_stamina.IsExhausted) {
                    _currentSpeed = CurrentSpeed == _movementSettings.speedRun ? _movementSettings.speedWalk : _movementSettings.speedRun;
                }
            }
            private void OnRun(InputAction.CallbackContext ctx) {
                if (!_isSliding && _currentStance == PlayerStance.Standing && !_stamina.IsExhausted) {
                    _currentSpeed = ctx.performed ? _movementSettings.speedRun : _movementSettings.speedWalk;
                }
            }
            private void OnRunCanceled(InputAction.CallbackContext ctx) {
                if (!_isSliding) _currentSpeed = _movementSettings.speedWalk;
            }
        #endregion
        #region Jump
            private void OnJump(InputAction.CallbackContext ctx) => Jump();
            public void Jump() {
                if ((_characterController.isGrounded && !_isSliding && _currentStance != PlayerStance.Crawl) || _movementSettings.jumpCount > 0) {
                    float jumpPower = _movementSettings.jumpPower;

                    _movementSettings.jumpCount -= 1f;
                    _velocity.y = Mathf.Sqrt(jumpPower * -2f * _movementSettings.gravity);

                    if (_currentStance != PlayerStance.Standing) SetStance(PlayerStance.Standing);
                    if (_isSliding) EndSlide();
                    if (_isGliding) EndGlide();
                }
            }
    #endregion
        #region Crouch
            public void ToggleCrouch() => SetStance(_currentStance == PlayerStance.Crouching ? PlayerStance.Standing : PlayerStance.Crouching);
            private void OnCrouch(InputAction.CallbackContext ctx) {
                if (!_isSliding) ToggleCrouch();
            }
            
            public void MobileCrouch() => OnCrouch(default);
        #endregion
        #region Crawl
            public void ToggleCrawl() => SetStance(_currentStance == PlayerStance.Crawl ? PlayerStance.Standing : PlayerStance.Crawl);
            private void OnCrawl(InputAction.CallbackContext ctx) {
                if (!_isSliding) ToggleCrawl();
            }
            public void MobileCrawl() => OnCrawl(default);
        #endregion
        #region Slide
            private bool CanSlide() {
                return _characterController.isGrounded && !_isSliding && (_currentStance == PlayerStance.Standing || _currentStance == PlayerStance.Crouching) &&
                        _moveInput.sqrMagnitude > INPUT_DEADZONE * INPUT_DEADZONE;
            }
            private void OnSlide(InputAction.CallbackContext ctx) {
                if (CanSlide()) StartSlide();
            }
            
            public void MobileSlide() => OnSlide(default);
        #endregion
            private void StartSlide() {
                _isSliding = true;
                _slideTimer = 0f;
                _currentSlideSpeed = _movementSettings.speedSlide;
                if (_moveInput.sqrMagnitude > INPUT_DEADZONE * INPUT_DEADZONE) 
                    _slideDirection = (transform.right * _moveInput.x + transform.forward * _moveInput.y).normalized;
                else 
                    _slideDirection = transform.forward;
                SetStance(PlayerStance.Crouching);
                _characterController.height = heightCrouch;
            }
            private void HandleSlide(float deltaTime) {
                _slideTimer += deltaTime;
                _currentSlideSpeed = _movementSettings.speedSlide * Mathf.Pow(_movementSettings.slideSpeedDecay, _slideTimer);
                if (_slideTimer >= _movementSettings.slideDuration || _currentSlideSpeed <= _movementSettings.speedCrouch) EndSlide();
                if (_slideCooldownTimer > 0) _slideCooldownTimer -= deltaTime;
            }
            private void EndSlide()
            {
                _isSliding = false;
                if (_currentStance == PlayerStance.Crouching) _currentSpeed = _movementSettings.speedCrouch;
            }
        #endregion
        #region Glide
            private bool CanGlide() {
                return !_characterController.isGrounded && !_isGliding && _velocity.y < 0;
            }
            private void OnGlideStart(InputAction.CallbackContext ctx) {
                if (CanGlide()) StartGlide();
            }
            private void OnGlideEnd(InputAction.CallbackContext ctx) {
                if (_isGliding) EndGlide();
            }
            public void GlideMobile()
            {
                if (_isGliding) EndGlide();
                else 
                {
                    if (CanGlide()) StartGlide();
                }
            }

            private void StartGlide() {
                StartAerial(true);
            }
            private void HandleGlide(float deltaTime) {
                HandleAerialMovement(deltaTime, _movementSettings.glideFall);
            }
            private void EndGlide() {
                ObjGlider.SetActive(false);
                _isGliding = false;
            }
        #endregion
        #region Jetpack
            private void OnJetpackStart(InputAction.CallbackContext ctx) => StartJetpack();
            private void OnJetpackEnd(InputAction.CallbackContext ctx) {
                if (_isJetpackUsed) EndJetpack();
            }
            public void MobileJetpackStart() => StartJetpack();
            public void MobileJetpackEnd() {
                if (_isJetpackUsed) EndJetpack();
            }
            private void StartJetpack() {
                StartAerial(false);
            }
            private void StartAerial(bool isGlide) {
                if (isGlide) {
                    _isGliding = true;
                    ObjGlider.SetActive(true);
                }
                else {
                    _isJetpackUsed = true;
                    ObjJetpack.SetActive(true);
                }
                _tempHorizontalVelocity.x = _velocity.x;
                _tempHorizontalVelocity.y = 0f;
                _tempHorizontalVelocity.z = _velocity.z;
                float horizontalMagnitude = _tempHorizontalVelocity.magnitude;
                if (horizontalMagnitude < _movementSettings.speedGlideMove && horizontalMagnitude > 0.001f) {
                    _tempHorizontalVelocity = _tempHorizontalVelocity.normalized * _movementSettings.speedGlideMove;
                }
                else if (horizontalMagnitude < 0.001f) {
                    // Если нет горизонтальной скорости, используем направление камеры вперед
                    _tempHorizontalVelocity = _cameraTransform.forward;
                    _tempHorizontalVelocity.y = 0f;
                    _tempHorizontalVelocity = _tempHorizontalVelocity.normalized * _movementSettings.speedGlideMove;
                }
                _velocity.x = _tempHorizontalVelocity.x;
                _velocity.y = _movementSettings.glideFall;
                _velocity.z = _tempHorizontalVelocity.z;
            }
            private void HandleJetpack(float deltaTime) {
                HandleAerialMovement(deltaTime, _movementSettings.glideFall);
            }
            
            private void HandleAerialMovement(float deltaTime, float baseFallSpeed) {
                float absPitch = Mathf.Abs(_cameraPitch);
                float cameraTiltFactor = absPitch / CAMERA_TILT_DIVISOR;

                if (_cameraPitch < -CAMERA_TILT_THRESHOLD) {
                    float speedBoost = GLIDE_SPEED_BOOST_MULTIPLIER * cameraTiltFactor;
                    _moveDirection *= speedBoost;
                    _velocity.y = Mathf.MoveTowards(_velocity.y, baseFallSpeed * GLIDE_FALL_MULTIPLIER_DIVE, 
                        Mathf.Abs(_movementSettings.gravity) * deltaTime * GLIDE_GRAVITY_MULTIPLIER_DIVE);
                }
                else if (_cameraPitch > CAMERA_TILT_THRESHOLD) {
                    float speedReduce = 1f - (GLIDE_SPEED_REDUCE_MULTIPLIER * cameraTiltFactor);
                    _moveDirection *= speedReduce;
                    _velocity.y = Mathf.MoveTowards(_velocity.y, baseFallSpeed * GLIDE_FALL_MULTIPLIER_DIVE, 
                        Mathf.Abs(_movementSettings.gravity) * deltaTime);
                }
                else {
                    _velocity.y = Mathf.MoveTowards(_velocity.y, baseFallSpeed * GLIDE_FALL_MULTIPLIER_NORMAL, 
                        Mathf.Abs(_movementSettings.gravity) * deltaTime * GLIDE_GRAVITY_MULTIPLIER_NORMAL);
                }
            }
            private void EndJetpack(){ 
                ObjJetpack.SetActive(false);
                _isJetpackUsed = false;
            }
        #endregion
    #region Inventory
        private void OnAttackStart(InputAction.CallbackContext ctx) => StartAttack();
        private void StartAttack() => Debug.Log("df");
        public void MobileAttack() => StartAttack();
        
        private void OnReloadStart(InputAction.CallbackContext ctx) => StartReload();
        private void StartReload() => Debug.Log("Df");
        public void MobileReload() => StartReload();
    #endregion
}