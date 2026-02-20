// using UnityEngine;
// using UnityEngine.InputSystem;

// // namespace EPC.player
// // {
// [System.Serializable]
// public struct MovementSettings {
//     public float speedWalk;
//     public float speedRun;
//     public float speedCrouch;
//     public float speedCrawl;
//     public float speedSlide;
//     public float jumpPower;
//     public float jumpCountMax;
//     public float jumpCount;
//     public float gravity;

//     public float glideFall;
//     public float speedGlideMove;
//     public float glideControl;

//     public float mouseSens;
//     public float slideDuration;
//     public float slideCooldown;
//     public float slideSpeedDecay;
// }

// // [System.Serializable]
// // public struct HealthSettings
// // {
// //     public float healthMax;
// //     public
// //     public bool needRegen;
// //     public float healthRegen;
// //     public float healthRegenDelay;
// //     public bool infiniteHealth;
// // }

// public class control : MonoBehaviour {
//     [Header("References")]
//     [SerializeField] private CharacterController _characterController;
//     [SerializeField] private CapsuleCollider _coll;
//     [SerializeField] private Transform _cameraTransform;

//     [Header("Settings")]
//     [SerializeField]
//     private MovementSettings _movementSettings = new MovementSettings
//     {
//         speedWalk = 3f,
//         speedRun = 6f,
//         speedCrouch = 1.5f,
//         speedCrawl = 1f,
//         speedSlide = 8f,
//         mouseSens = 0.5f,
//         jumpPower = 2f,
//         jumpCount = 2f,
//         jumpCountMax = 2f,
//         gravity = -9.81f,
//         glideFall = -2f,
//         speedGlideMove = 4f,
//         glideControl = 2f,
//         slideDuration = 1f,
//         slideCooldown = 1.5f,
//         slideSpeedDecay = 0.8f
//     };

//     private float heightStand = 2f;
//     private float heightCrouch = 1f;
//     private float heightCrawl = 0.5f;
//     private float heightSpeed = 5f;


//     private readonly Vector3 _vectorUp = Vector3.up;
//     private readonly Vector3 _vectorRight = Vector3.right;
//     private readonly Vector3 _vectorForward = Vector3.forward;
//     private readonly Vector2 _vector2Zero = Vector2.zero;
//     private readonly Vector3 _vector3Zero = Vector3.zero;
//     private const float GROUNDED_VELOCITY_Y = -2f;
//     private const float CAMERA_PITCH_MIN = -90f;
//     private const float CAMERA_PITCH_MAX = 90f;
//     private const float INPUT_DEADZONE = 0.1f;
//     private const float STAMINA_MAX = 100f;

//     private Vector2 _moveInput;
//     private Vector2 _mouseInput;

//     [SerializeField] private Vector3 _velocity;
//     private Vector3 _moveDirection;
//     private float _cameraPitch;
//     [SerializeField] private float _currentSpeed;

//     private bool _wasGrounded;

//     private bool _isGliding;
//     private bool _isJetpackUsed;
//     private bool isStaminaUsed;

//     // [SerializeField] private float _currentStamina;
//     // [SerializeField] private float _currentHealth;

//     [Header("Camera Settings")]
//     [SerializeField] private float _cameraSmoothness = 5f;
//     [SerializeField] private bool _invertY = false;
//     [SerializeField] private bool _invertX = false;

//     private float _targetCameraPitch;

//     public enum PlayerStance { Standing, Crouching, Crawl, Gliding, Simming, Noclip, }
//     private PlayerStance _currentStance = PlayerStance.Standing;
//     private float _targetHeight;

//     private bool _isSliding = false;
//     private float _slideTimer = 0f;
//     private float _slideCooldownTimer = 0f;
//     private Vector3 _slideDirection;
//     private float _currentSlideSpeed;

//     private InputAction _jumpAction;
//     private InputAction _moveAction;
//     private InputAction _lookAction;
//     private InputAction _runAction;
//     private InputAction _crouchAction;
//     private InputAction _crawlAction;
//     private InputAction _slideAction;
//     private InputAction _glideAction;
//     private InputAction _jetpackAction;

//     public GameObject ObjGlider;
//     public GameObject ObjJetpack;

//     public bool IsSliding => _isSliding;
//     public bool IsGliding => _isGliding;
//     public PlayerStance CurrentStance => _currentStance;
//     public float CurrentSpeed => _currentSpeed;
//     public bool IsGrounded => _characterController.isGrounded;
//     // public bool IsExhausted => _isExhausted;
//     private HealthSystem _health;
//     private StaminaSystem _stamina;
    

//     #region initializate
//         private void Awake() {
//             if (_characterController == null) _characterController = GetComponent<CharacterController>();

//             var playerActions = InputManager.Instance.actions.Player;
//             _jumpAction = playerActions.Jump;
//             _moveAction = playerActions.Move;
//             _lookAction = playerActions.Look;
//             _runAction = playerActions.Run;
//             _crouchAction = playerActions.Crouch;
//             _crawlAction = playerActions.Crawl;
//             _slideAction = playerActions.Slide;
//             _glideAction = playerActions.Glide;
//             _jetpackAction = playerActions.Jetpack;
            
//             _health = GetComponent<HealthSystem>();
//             _stamina = GetComponent<StaminaSystem>();
            
//             _health.OnDeath += OnPlayerDeath;
//             _health.OnDamageTaken += OnDamageTaken;
            
//             _stamina.OnExhaustedStateChanged += OnExhaustedChanged;

//             InitializeStance();
//         }
//         private void OnPlayerDeath() {
//             DisableMovement();
//             enabled = false;
            
//             Debug.Log("Player Died!");
//         }
//         private void OnDamageTaken(float damage) {
//             if (damage > 20f) 
//                 StartCoroutine(StunEffect(0.5f));
//         }
//         private System.Collections.IEnumerator StunEffect(float duration) {
//             float originalSpeed = _currentSpeed;
//             _currentSpeed *= 0.3f;
//             yield return new WaitForSeconds(duration);
//             _currentSpeed = originalSpeed;
//         }
        
//         private void OnExhaustedChanged(bool isExhausted) {
//             if (isExhausted){
//                 if (_isGliding) EndGlide();
//                 if (_currentSpeed >= _movementSettings.speedRun - 0.1f) 
//                     _currentSpeed = _movementSettings.speedWalk * _stamina._settings.exhaustedSpeedMultiplier;
//             }
//         }
        
//         private void OnEnable() => SubscribeToInputActions();
//         private void OnDisable() => UnsubscribeFromInputActions();
//         public void DisableMovement() {
//             _moveInput = _vector2Zero;
//             _mouseInput = _vector2Zero;
//             _currentSpeed = 0f;
//             if (_isGliding) EndGlide();
//             // if (_isSliding) EndSlide();
//         }
//         public void EnableMovement() => _currentSpeed = _movementSettings.speedWalk;
//         private void SubscribeToInputActions() {
//             _jumpAction.performed += OnJump;
//             _moveAction.performed += OnMove;
//             _moveAction.canceled += OnMoveCanceled;
//             _lookAction.performed += OnLook;
//             _lookAction.canceled += OnLookCanceled;
//             _runAction.performed += OnRun;
//             _runAction.canceled += OnRunCanceled;
//             _crouchAction.performed += OnCrouch;
//             _crawlAction.performed += OnProne;
//             _slideAction.performed += OnSlide;
//             _glideAction.performed += OnGlideStart;
//             _glideAction.canceled += OnGlideEnd;
//             _jetpackAction.performed += OnJetpackStart;
//             _jetpackAction.canceled += OnJetpackEnd;
//         }
//         private void UnsubscribeFromInputActions()
//         {
//             _jumpAction.performed -= OnJump;
//             _moveAction.performed -= OnMove;
//             _moveAction.canceled -= OnMoveCanceled;
//             _lookAction.performed -= OnLook;
//             _lookAction.canceled -= OnLookCanceled;
//             _runAction.performed -= OnRun;
//             _runAction.canceled -= OnRunCanceled;
//             _crouchAction.performed -= OnCrouch;
//             _crawlAction.performed -= OnProne;
//             _slideAction.performed -= OnSlide;
//             _glideAction.performed -= OnGlideStart;
//             _glideAction.canceled -= OnGlideEnd;
//             _jetpackAction.performed -= OnJetpackStart;
//             _jetpackAction.canceled -= OnJetpackEnd;
//         }
//     #endregion
//     private void Update()
//     {
//         float deltaTime = Time.deltaTime;
//         bool _isGrounded = _characterController.isGrounded;
//         if (_isGrounded && !_wasGrounded)
//         {
//             _movementSettings.jumpCount = _movementSettings.jumpCountMax;
//             if (_isGliding) EndGlide();
//         }
//         _wasGrounded = _isGrounded;

//         // if (isStaminaUsed) HandleStamina(deltaTime); // glide run
//         if (!_isSliding && !_isGliding) HandleStance(deltaTime);
//         if (_isSliding) HandleSlide(deltaTime);
//         if (_isGliding) HandleGlide(deltaTime);
//         if (_isJetpackUsed) HandleJetpack(deltaTime);
//         HandleMovement(deltaTime);
//         if (_mouseInput != _vector2Zero) HandleMouseLook(deltaTime);
//     }
//     #region Movement
//         #region Stance
//             private void InitializeStance() {
//                 _targetHeight = heightStand;
//                 _currentSpeed = _movementSettings.speedWalk;
//                 _characterController.height = _targetHeight;
//             }
//             private void HandleStance(float deltaTime) {
//                 if (Mathf.Abs(_characterController.height - _targetHeight) > 0.01f) {
//                     float newHeight = Mathf.Lerp(_characterController.height, _targetHeight, heightSpeed * deltaTime);
//                     _characterController.height = newHeight;
//                     UpdateCameraPosition(newHeight);
//                 }
//             }
//             private void UpdateCameraPosition(float newHeight) {
//                 Vector3 cameraPos = _cameraTransform.localPosition;
//                 cameraPos.y = newHeight * 0.6f;
//                 cameraPos.y = Mathf.Clamp(cameraPos.y, -newHeight * 3 / 5, newHeight / 2 * 3 / 5);
//                 _cameraTransform.localPosition = cameraPos;
//             }
//             private void SetStance(PlayerStance newStance) {
//                 _currentStance = newStance;
//                 switch (newStance) {
//                     case PlayerStance.Standing:
//                         _targetHeight = heightStand;
//                         _currentSpeed = _movementSettings.speedWalk;
//                         break;
//                     case PlayerStance.Crouching:
//                         _targetHeight = heightCrouch;
//                         _currentSpeed = _movementSettings.speedCrouch;
//                         break;
//                     case PlayerStance.Crawl:
//                         _targetHeight = heightCrawl;
//                         _currentSpeed = _movementSettings.speedCrawl;
//                         break;
//                     case PlayerStance.Gliding:
//                         break;
//                 }
//             }
//             public void ForceStance(PlayerStance stance) => SetStance(stance);
//         #endregion
//         #region Move
//             private void OnMove(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();
//             private void OnMoveCanceled(InputAction.CallbackContext ctx) => _moveInput = _vector2Zero;
//             private void HandleMovement(float deltaTime) {
//                 if (!_isSliding && !_isGliding)
//                 {
//                     // float speedMultiplier = _isExhausted ? _staminaSettings.exhaustedSpeedMultiplier : 1f;

//                     Vector3 cameraForward = transform.forward;
//                     Vector3 cameraRight = _cameraTransform.right;

//                     cameraForward.y = 0;
//                     cameraRight.y = 0;
//                     cameraForward.Normalize();
//                     cameraRight.Normalize();

//                     _moveDirection = (cameraRight * _moveInput.x + cameraForward * _moveInput.y) * _currentSpeed; // * speedMultiplier;
//                 }
//                 else if (_isSliding)
//                 {
//                     _moveDirection = _slideDirection * _currentSlideSpeed;
//                 }
//                 else if (_isGliding)
//                 {
//                     Vector3 glideControl = (transform.right * _moveInput.x + _cameraTransform.forward * _moveInput.y + _cameraTransform.forward * _movementSettings.speedGlideMove) * _movementSettings.glideControl;

//                     Vector3 currentHorizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);
//                     _moveDirection = currentHorizontalVelocity + glideControl;
//                     _moveDirection = Vector3.ClampMagnitude(_moveDirection, _movementSettings.speedGlideMove);

//                     _velocity.y = Mathf.MoveTowards(_velocity.y, _movementSettings.glideFall, Mathf.Abs(_movementSettings.gravity) * deltaTime * 0.5f);
//                 }
//                 if (_isJetpackUsed)
//                 {
                    
//                     _velocity.y = Mathf.MoveTowards(_velocity.y, 5f, 5f * deltaTime * 0.5f);
//                 }
//                 if (_characterController.isGrounded) {
//                     if (_velocity.y < 0) _velocity.y = GROUNDED_VELOCITY_Y;
//                 }
//                 else if (!_isGliding) _velocity.y += _movementSettings.gravity * deltaTime;

//                 Vector3 finalMovement = (_moveDirection + _vectorUp * _velocity.y) * deltaTime;
//                 _characterController.Move(finalMovement);
//             }
//         #endregion
//         #region Mouse
//             private void OnLook(InputAction.CallbackContext ctx) => _mouseInput = ctx.ReadValue<Vector2>();
//             private void OnLookCanceled(InputAction.CallbackContext ctx) => _mouseInput = _vector2Zero;
//             private void HandleMouseLook(float deltaTime) {
//                 float sensitivity = _movementSettings.mouseSens * 100f * deltaTime;
//                 float xInput = _mouseInput.x * sensitivity * (_invertX ? -1f : 1f);
//                 transform.Rotate(_vectorUp, xInput);

//                 float yInput = _mouseInput.y * sensitivity * (_invertY ? 1f : -1f);
//                 _targetCameraPitch = Mathf.Clamp(_targetCameraPitch + yInput, CAMERA_PITCH_MIN, CAMERA_PITCH_MAX);

//                 _cameraPitch = Mathf.Lerp(_cameraPitch, _targetCameraPitch, _cameraSmoothness * deltaTime);
//                 _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);

//                 _mouseInput = _vector2Zero;
//             }
//         #endregion 
//         #region Run
//             private void OnRun(InputAction.CallbackContext ctx) {
//                 if (!_isSliding && _currentStance == PlayerStance.Standing && !_stamina.IsExhausted) {
//                     if (ctx.performed) _currentSpeed = _movementSettings.speedRun;
//                     else if (ctx.canceled) _currentSpeed = _movementSettings.speedWalk;
//                 }
//             }
//             private void OnRunCanceled(InputAction.CallbackContext ctx) {
//                 if (!_isSliding) _currentSpeed = _movementSettings.speedWalk;
//             }
//         #endregion
//         #region Jump
//             private void OnJump(InputAction.CallbackContext ctx) => Jump();
//             private void Jump() {
//                 if ((_characterController.isGrounded && !_isSliding && _currentStance != PlayerStance.Crawl) || _movementSettings.jumpCount > 0) {
//                     float jumpPower = _movementSettings.jumpPower;

//                     _movementSettings.jumpCount -= 1f;
//                     _velocity.y = Mathf.Sqrt(jumpPower * -2f * _movementSettings.gravity);

//                     if (_currentStance != PlayerStance.Standing) SetStance(PlayerStance.Standing);
//                     if (_isSliding) EndSlide();
//                     if (_isGliding) EndGlide();
//                 }
//             }
//     #endregion
//         #region Crouch
//             private void ToggleCrouch() => SetStance(_currentStance == PlayerStance.Crouching ? PlayerStance.Standing : PlayerStance.Crouching);
//             private void OnCrouch(InputAction.CallbackContext ctx)
//             {
//                 if (!_isSliding) ToggleCrouch();
//             }
//         #endregion
//         #region Crawl
//             private void ToggleProne() => SetStance(_currentStance == PlayerStance.Crawl ? PlayerStance.Standing : PlayerStance.Crawl);
//             private void OnProne(InputAction.CallbackContext ctx) {
//                 if (!_isSliding) ToggleProne();
//             }
//         #endregion
//         #region Slide
//             private bool CanSlide() {
//                 return _characterController.isGrounded && !_isSliding /*&& _slideCooldownTimer <= 0*/ && (_currentStance == PlayerStance.Standing || _currentStance == PlayerStance.Crouching) &&
//                         _moveInput.sqrMagnitude > INPUT_DEADZONE * INPUT_DEADZONE;
//             }
//             private void OnSlide(InputAction.CallbackContext ctx) {
//                 if (CanSlide()) StartSlide();
//             }
//             private void StartSlide() {
//                 _isSliding = true;
//                 _slideTimer = 0f;
//                 // _slideCooldownTimer = _movementSettings.slideCooldown;
//                 _currentSlideSpeed = _movementSettings.speedSlide;
//                 if (_moveInput.sqrMagnitude > INPUT_DEADZONE * INPUT_DEADZONE) _slideDirection = (transform.right * _moveInput.x + transform.forward * _moveInput.y).normalized;
//                 else _slideDirection = transform.forward;
//                 SetStance(PlayerStance.Crouching);
//                 _characterController.height = heightCrouch;
//             }
//             private void HandleSlide(float deltaTime) {
//                 _slideTimer += deltaTime;
//                 _currentSlideSpeed = _movementSettings.speedSlide * Mathf.Pow(_movementSettings.slideSpeedDecay, _slideTimer);
//                 if (_slideTimer >= _movementSettings.slideDuration || _currentSlideSpeed <= _movementSettings.speedCrouch) EndSlide();
//                 if (_slideCooldownTimer > 0) _slideCooldownTimer -= deltaTime;
//             }
//             private void EndSlide()
//             {
//                 _isSliding = false;
//                 if (_currentStance == PlayerStance.Crouching) _currentSpeed = _movementSettings.speedCrouch;
//             }
//         #endregion
//         #region Glide
//             private bool CanGlide() {
//                 return !_characterController.isGrounded && !_isGliding && _velocity.y < 0; // && _currentStamina > 0 ; // && _currentStance != PlayerStance.Crawl;
//             }
//             private void OnGlideStart(InputAction.CallbackContext ctx) {
//                 if (CanGlide()) StartGlide();
//             }
//             private void OnGlideEnd(InputAction.CallbackContext ctx) {
//                 if (_isGliding) EndGlide();
//             }

//             //private System.Collections.IEnumerator PerformGlideSlam() {
//             //    float duration = 0.8f;
//             //    float elapsed = 0f;
//             //    float startPitch = _cameraPitch;
//             //    float targetPitch = -80f;

//             //    while (elapsed < duration && _isGliding) {
//             //        _cameraPitch = Mathf.Lerp(startPitch, targetPitch, elapsed / duration);
//             //        _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);

//             //        _velocity.y = Mathf.Lerp(_velocity.y, _movementSettings.gravity * 2f, elapsed / duration);

//             //        elapsed += Time.deltaTime;
//             //        yield return null;
//             //    }
//             //    _currentSpeed += 3f;
//             //}
//             private void StartGlide() {
//                 _isGliding = true;
//                 ObjGlider.SetActive(true);

//                 Vector3 horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);
//                 if (horizontalVelocity.magnitude < _movementSettings.speedGlideMove) {
//                     horizontalVelocity = horizontalVelocity.normalized * _movementSettings.speedGlideMove;
//                 }
//                 _velocity = new Vector3(horizontalVelocity.x, _movementSettings.glideFall, horizontalVelocity.z);
//             }
//             private void HandleGlide(float deltaTime) {
//                 float absPitch = Mathf.Abs(_cameraPitch);
//                 float cameraTiltFactor = absPitch / 45f;

//                 if (_cameraPitch < -35f) {
//                     float speedBoost = 0.8f * cameraTiltFactor;
//                     _moveDirection *= speedBoost;
//                     _velocity.y = Mathf.MoveTowards(_velocity.y, _movementSettings.glideFall * 8f, Mathf.Abs(_movementSettings.gravity) * deltaTime * 0.75f);
//                 }
//                 else if (_cameraPitch > 35f) {
//                     float speedReduce = 1f - (0.3f * cameraTiltFactor);
//                     _moveDirection *= speedReduce;
//                     _velocity.y = Mathf.MoveTowards(_velocity.y, _movementSettings.glideFall * 8f, Mathf.Abs(_movementSettings.gravity) * deltaTime);
//                 }
//                 else {
//                     _velocity.y = Mathf.MoveTowards(_velocity.y, _movementSettings.glideFall * 4f, Mathf.Abs(_movementSettings.gravity) * deltaTime * 0.65f);
//                 }
//                 // float targetPitch = Mathf.Clamp(_cameraPitch - 10f, CAMERA_PITCH_MIN, CAMERA_PITCH_MAX);
//                 // _cameraPitch = Mathf.Lerp(_cameraPitch, targetPitch, 5f * deltaTime);
//                 // _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
//             }
//             private void EndGlide()
//             {
//                 ObjGlider.SetActive(false);
//                 _isGliding = false;
//             }
//         #endregion
//         #region Jetpack
//             private void OnJetpackStart(InputAction.CallbackContext ctx) {
//                 StartGlide();
//             }
//             private void OnJetpackEnd(InputAction.CallbackContext ctx) {
//                 if (_isJetpackUsed) EndJetpack();
//             }
//             private void StartJetpack() {
//                 _isJetpackUsed = true;
//                 ObjJetpack.SetActive(true);

//                 Vector3 horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);
//                 if (horizontalVelocity.magnitude < _movementSettings.speedGlideMove) {
//                     horizontalVelocity = horizontalVelocity.normalized * _movementSettings.speedGlideMove;
//                 }
//                 _velocity = new Vector3(horizontalVelocity.x, _movementSettings.glideFall, horizontalVelocity.z);
//             }
//             private void HandleJetpack(float deltaTime) {
//                 float absPitch = Mathf.Abs(_cameraPitch);
//                 float cameraTiltFactor = absPitch / 45f;

//                 if (_cameraPitch < -35f) {
//                     float speedBoost = 0.8f * cameraTiltFactor;
//                     _moveDirection *= speedBoost;
//                     _velocity.y = Mathf.MoveTowards(_velocity.y, _movementSettings.glideFall * 8f, Mathf.Abs(_movementSettings.gravity) * deltaTime * 0.75f);
//                 }
//                 else if (_cameraPitch > 35f) {
//                     float speedReduce = 1f - (0.3f * cameraTiltFactor);
//                     _moveDirection *= speedReduce;
//                     _velocity.y = Mathf.MoveTowards(_velocity.y, _movementSettings.glideFall * 8f, Mathf.Abs(_movementSettings.gravity) * deltaTime);
//                 }
//                 else {
//                     _velocity.y = Mathf.MoveTowards(_velocity.y, _movementSettings.glideFall * 4f, Mathf.Abs(_movementSettings.gravity) * deltaTime * 0.65f);
//                 }
//                 // float targetPitch = Mathf.Clamp(_cameraPitch - 10f, CAMERA_PITCH_MIN, CAMERA_PITCH_MAX);
//                 // _cameraPitch = Mathf.Lerp(_cameraPitch, targetPitch, 5f * deltaTime);
//                 // _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
//             }
//             private void EndJetpack(){ 
//                 ObjJetpack.SetActive(false);
//                 _isJetpackUsed = false;
//             }
//         #endregion
//         /* #region Stamina
//         private void HandleStamina(float deltaTime) {
//             if (_staminaSettings.infiniteStamina) return;
//             if (_currentStamina <= 0) {
//                 _currentStamina = 0;
//                 _isExhausted = true;

//                 if (_currentSpeed >= _movementSettings.speedRun - 0.1f) _currentSpeed = _movementSettings.speedWalk * _staminaSettings.exhaustedSpeedMultiplier;
//             }
//             else {
//                 if (_currentSpeed >= _movementSettings.speedRun - 0.1f && _moveInput != Vector2.zero && _characterController.isGrounded){
//                     _currentStamina -= _staminaSettings.StaminaDrainRun * deltaTime;
//                     isStaminaUsed = true;
//                 }
//             }

//             if (!isStaminaUsed && _currentStamina < _staminaSettings.staminaMax) {
//                 _staminaRegenTimer += deltaTime;
//                 if (_staminaRegenTimer >= _staminaSettings.staminaRegenDelay) {
//                     _currentStamina += _staminaSettings.staminaRegen * deltaTime;
//                     _currentStamina = Mathf.Min(_currentStamina, _staminaSettings.staminaMax);

//                     if (_currentStamina >= _staminaSettings.staminaMax * 0.3f) _isExhausted = false;
//                 }
//             }
//             else _staminaRegenTimer = 0f;
//         }
//         public void AddStamina(float amount) {
//             _currentStamina = Mathf.Clamp(_currentStamina + amount, 0, _staminaSettings.staminaMax);
//         }
//         public void SetInfiniteStamina(bool enabled) {
//             _staminaSettings.infiniteStamina = enabled;
//             if (enabled) {
//                 _currentStamina = _staminaSettings.staminaMax;
//                 _isExhausted = false;
//             }
//         }
//         public void ResetStamina() {
//             _currentStamina = _staminaSettings.staminaMax;
//             _isExhausted = false;
//             _staminaRegenTimer = 0f;
//         }
//         public void ToggleInfiniteStamina()
//         {
//             SetInfiniteStamina(!_staminaSettings.infiniteStamina);
//             Debug.Log($"Infinity: {_staminaSettings.infiniteStamina}");
//         }
//         #endregion */
//     #endregion
//     // gizmoz
//     // private void OnDrawGizmosSelected() {
//     //     if (_characterController != null) {
//     //         Gizmos.color = Color.green;
//     //         Gizmos.DrawWireSphere(transform.position + _characterController.center, 0.1f);

//     //         if (_isGliding) {
//     //             Gizmos.color = Color.blue;
//     //             Gizmos.DrawRay(transform.position, _moveDirection.normalized * 2f);
//     //         }
//     //     }
//     // }
// }