using FPProject.Manager;
using UnityEngine;

namespace FPProject.PlayerControl
{
    public class PlayerController : MonoBehaviour
    {
        public bool CanMove { get; private set;  } = true;
        private bool CanRun => _inputManager.Move == Vector2.up;
        
        [SerializeField] private float animationBlendSpeed = 8.9f;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Transform playerCamera;
        
        [Header("Look Parameters")]
        [SerializeField] private float upperLimit = 90f;
        [SerializeField] private float lowerLimit = 80f;
        [SerializeField, Range(1, 100)] private float mouseSensitivity = 40f;
        
        private Rigidbody _playerRigidbody;
        private InputManager _inputManager;
        private Animator _animator;
        private bool _hasAnimator;
        private int _xVelHash;
        private int _yVelHash;
        private float _xRotation;

        private const float WalkSpeed = 5f;
        private const float RunSpeed = 9f;
        [SerializeField] private Vector2 currentVelocity;
        
        private void Start()
        {
            _hasAnimator = TryGetComponent<Animator>(out _animator);
            _playerRigidbody = GetComponent<Rigidbody>();
            _inputManager = GetComponent<InputManager>();

            _xVelHash = Animator.StringToHash("X_Velocity");
            _yVelHash = Animator.StringToHash("Y_Velocity");
        }

        private void FixedUpdate()
        {
            if (CanMove)
                Move();
        }

        private void LateUpdate()
        {
            
            CamMovement();
        }

        private void Move()
        {
            if (!_hasAnimator) 
                return;

            var targetSpeed = _inputManager.Run && CanRun ? RunSpeed : WalkSpeed;

            if (_inputManager.Move == Vector2.zero)
                targetSpeed = 0;
            
            Vector3 inputDirection = new Vector3(_inputManager.Move.x, _inputManager.Move.y);
            inputDirection = inputDirection.normalized;

            currentVelocity.x = 
                Mathf.Lerp(currentVelocity.x, _inputManager.Move.x * targetSpeed, animationBlendSpeed * Time.fixedDeltaTime);
            currentVelocity.y =
                Mathf.Lerp(currentVelocity.y, _inputManager.Move.y * targetSpeed, animationBlendSpeed * Time.fixedDeltaTime);

            var xVelDifference = currentVelocity.x - _playerRigidbody.velocity.x;
            var zVelDifference = currentVelocity.y - _playerRigidbody.velocity.z;
            
            _playerRigidbody.AddForce(transform.TransformVector(new Vector3(xVelDifference, 0, zVelDifference)), ForceMode.VelocityChange);
            
            _animator.SetFloat(_xVelHash, currentVelocity.x);
            _animator.SetFloat(_yVelHash, currentVelocity.y);
        }

        private void CamMovement()
        {
            if (!_hasAnimator) 
                return;

            var mouseX = _inputManager.Look.x;
            var mouseY = _inputManager.Look.y;
            playerCamera.position = cameraRoot.position;

            _xRotation -= mouseY * mouseSensitivity;
            _xRotation = Mathf.Clamp(_xRotation, -upperLimit, lowerLimit);
            
            playerCamera.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            _playerRigidbody.MoveRotation(_playerRigidbody.rotation * Quaternion.Euler(0, mouseX * mouseSensitivity, 0));
        }
    }
}
