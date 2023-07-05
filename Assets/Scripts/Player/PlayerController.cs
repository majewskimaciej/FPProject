using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private bool shouldRun => _inputManager.Move == Vector2.up;
        
        [SerializeField] private bool canMove = true;
        [SerializeField] private bool canLook = true;
        [SerializeField] private bool canRun = true;
        [SerializeField] private float animationBlendSpeed = 8.9f;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Transform playerCamera;

        [Header("Look Parameters")] 
        [SerializeField] private float upperLimit = 90f;
        [SerializeField] private float lowerLimit = 80f;
        [SerializeField] private float horizontalLimit = 45f;
        [SerializeField] private float mouseSensitivity = 21.9f;

        [Header("Jump Parameters")] 
        [SerializeField] private float jumpFactor = 260f;
        [SerializeField] private float groundedDistance = 0.8f;
        [SerializeField] private LayerMask groundCheck;
        [SerializeField] private float airResistance = 0.8f;

        private Rigidbody _playerRigidbody;
        private InputManager _inputManager;
        private Animator _animator;
        private PlayerState _playerState;
        private bool _hasAnimator;
        private int _xVelHash;
        private int _yVelHash;
        private int _zVelHash;
        private int _moveHash;
        private int _runHash;
        private int _jumpHash;
        private int _groundHash;
        private int _fallingHash;

        private float _xRotation;
        private float _yRotation;
        private float _yRotationLastGrounded;
        private bool _isGrounded;

        private Vector2 _currentVelocity;

        private const float WalkSpeed = 5f;
        private const float RunSpeed = 9f;

        private void Start()
        {
            _hasAnimator = TryGetComponent<Animator>(out _animator);
            _playerRigidbody = GetComponent<Rigidbody>();
            _inputManager = GetComponent<InputManager>();

            _xVelHash = Animator.StringToHash("X_Velocity");
            _yVelHash = Animator.StringToHash("Y_Velocity");
            _zVelHash = Animator.StringToHash("Z_Velocity");
            _jumpHash = Animator.StringToHash("Jump");
            _moveHash = Animator.StringToHash("isMoving");
            _runHash = Animator.StringToHash("isRunning");
            _groundHash = Animator.StringToHash("isGrounded");
            _fallingHash = Animator.StringToHash("isFalling");
        }

        private void FixedUpdate()
        {
            SampleGround();
            SetState();
            HandleStates();
            HandleJump();
        }

        private void LateUpdate()
        {
            if (canLook)
                HandleLook();
        }
        
        private void SetState()
        {
            if (_inputManager.Move != Vector2.zero && canMove)
            {
                _playerState = PlayerState.Moving;
            }
            else
            {
                _playerState = PlayerState.Idle;
            }

            if (_inputManager.Run && canRun && shouldRun)
            {
                _playerState = PlayerState.Running;
            }

            if (!_isGrounded)
            {
                _playerState = PlayerState.Falling;
            }
        }

        private void HandleStates()
        {
            if (!_hasAnimator)
                return;

            var targetSpeed = GetTargetSpeed();
            
            switch (_playerState)
            {
                case PlayerState.Idle:
                    HandleIdle();
                    break;
                case PlayerState.Moving:
                    HandleMove(targetSpeed);
                    break;
                case PlayerState.Running:
                    HandleMove(targetSpeed);
                    break;
                case PlayerState.Falling:
                    HandleFall(targetSpeed);
                    break;
            }
            
            _animator.SetFloat(_xVelHash, _currentVelocity.x);
            _animator.SetFloat(_yVelHash, _currentVelocity.y);
            _animator.SetBool(_runHash, _playerState == PlayerState.Running);
            _animator.SetBool(_moveHash, _playerState == PlayerState.Moving);
        }
        
        private void HandleIdle()
        {
            RotatePlayer();
        }
        
        private void HandleMove(float targetSpeed)
        {
            GetCurrentVelocity(targetSpeed);

            var playerVelocity = _playerRigidbody.velocity;

            var xVelDifference = _currentVelocity.x - playerVelocity.x;
            var zVelDifference = _currentVelocity.y - playerVelocity.z;

            _playerRigidbody.AddForce(transform.TransformVector(new Vector3(xVelDifference, 0, zVelDifference)),
                ForceMode.VelocityChange);
            RotatePlayer();
        }
        
        private void HandleFall(float targetSpeed)
        {
            GetCurrentVelocity(targetSpeed);
            
            _playerRigidbody.AddForce(
                transform.TransformVector(new Vector3(_currentVelocity.x * airResistance, 0,
                    _currentVelocity.y * airResistance)), ForceMode.VelocityChange);
        }
        
        private void HandleLook()
        {
            if (!_hasAnimator)
                return;

            playerCamera.position = cameraRoot.position;

            _xRotation -= _inputManager.Look.y * mouseSensitivity;
            _xRotation = Mathf.Clamp(_xRotation, -upperLimit, lowerLimit);

            _yRotation = Mathf.Repeat((_yRotation + _inputManager.Look.x * mouseSensitivity), 360);

            if (!_isGrounded)
            {
                _yRotation = Helper.CustomClamp.Clamp(_yRotation, _yRotationLastGrounded, horizontalLimit);
            }

            playerCamera.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }
        
        private void HandleJump()
        {
            if (!_hasAnimator)
                return;
            if (!_inputManager.Jump)
                return;
            if (!_isGrounded)
                return;

            _animator.SetTrigger(_jumpHash);
        }
        
        private void SampleGround()
        {
            if (!_hasAnimator)
                return;
            
            if (Physics.Raycast(_playerRigidbody.worldCenterOfMass, Vector3.down, groundedDistance + 0.1f,
                    groundCheck))
            {
                _isGrounded = true;
                _yRotationLastGrounded = playerCamera.localEulerAngles.y;
                SetGroundedAnimationState();
                return;
            }

            _isGrounded = false;
            _animator.SetFloat(_zVelHash, _playerRigidbody.velocity.y);
            SetGroundedAnimationState();
        }
        
        private void RotatePlayer()
        {
            _playerRigidbody.MoveRotation(Quaternion.Euler(0, _yRotation, 0));
        }
        
        private float GetTargetSpeed()
        {
            return _playerState == PlayerState.Running ? RunSpeed :
                _playerState == PlayerState.Idle ? 0 : WalkSpeed;;
        }

        private void GetCurrentVelocity(float targetSpeed)
        {
            _currentVelocity.x =
                Mathf.Lerp(_currentVelocity.x, _inputManager.Move.x * targetSpeed,
                    animationBlendSpeed * Time.fixedDeltaTime);
            _currentVelocity.y =
                Mathf.Lerp(_currentVelocity.y, _inputManager.Move.y * targetSpeed,
                    animationBlendSpeed * Time.fixedDeltaTime);
        }

        private void SetGroundedAnimationState()
        {
            _animator.SetBool(_fallingHash, !_isGrounded);
            _animator.SetBool(_groundHash, _isGrounded);
        }

        public void JumpAddForce()
        {
            _playerRigidbody.AddForce(-_playerRigidbody.velocity.y * Vector3.up, ForceMode.VelocityChange);
            _playerRigidbody.AddForce(Vector3.up * jumpFactor, ForceMode.Impulse);
            _animator.ResetTrigger(_jumpHash);
        }
    }
}
