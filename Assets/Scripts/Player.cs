using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;

    private float _mouseLookX;
    private float _mouseLookY;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private Vector3 _targetMovement;
    private Vector3 _movement;
    private Rigidbody _rigidbody;

    private bool _jumpThisFrame;
    private bool _jumping;
 
    private void Awake()
    {
        Time.fixedDeltaTime = 1.0f / 60.0f;
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
    } 

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
    } 

    private void Update()
    {
        RotateCamera();
        CalculateTargetMovement();
        CheckForJump();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void RotateCamera()
    {
        _lookInput = lookAction.action.ReadValue<Vector2>();
        _mouseLookX += _lookInput.x * 2.0f;
        _mouseLookY += _lookInput.y * 2.0f;

        while (_mouseLookY < -180.0f) _mouseLookY += 360.0f;
        while (_mouseLookY > 180.0f) _mouseLookY -= 360.0f;
        _mouseLookY = Mathf.Clamp(_mouseLookY, -15.0f, 15.0f);

        cameraTarget.localRotation = Quaternion.Euler(-_mouseLookY, _mouseLookX, 0.0f);
    }

    private void CalculateTargetMovement()
    {
        _moveInput = moveAction.action.ReadValue<Vector2>();
        Vector3 inputMovement = new Vector3(_moveInput.x, 0, _moveInput.y) * 5.0f;

        Vector3 cameraLookForwardVector = ProjectVectorOntoGroundPlane(cameraTarget.forward);
        Quaternion cameraLookForward = Quaternion.LookRotation(cameraLookForwardVector);

        _targetMovement = cameraLookForward * inputMovement;
    }

    private void CheckForJump()
    {
        // Jump if the space bar was pressed this frame and we're not already jumping, trigger the jump
        if (jumpAction.action.WasPressedThisFrame() && !_jumping)
            _jumpThisFrame = true;
    }

    private void MovePlayer()
    {
        Vector3 velocity = _rigidbody.linearVelocity;

        _movement = Vector3.Lerp(_movement, _targetMovement, Time.fixedDeltaTime * 5.0f);
        velocity.x = _movement.x;
        velocity.z = _movement.z;

        if (_jumpThisFrame)
        {
            velocity.y = 5.0f;

            _jumping = true;
            _jumpThisFrame = false;
        }

        if (_jumping && velocity.y < -0.1f)
            _jumping = false;

        _rigidbody.linearVelocity = velocity;
    }

    private static Vector3 ProjectVectorOntoGroundPlane(Vector3 vector)
    {
        Vector3 planeNormal = Vector3.up;
        Vector3.OrthoNormalize(ref planeNormal, ref vector);
        return vector;
    }
}
