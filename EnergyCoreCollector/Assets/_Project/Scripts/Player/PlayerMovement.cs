using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";

    [Header("Camera Relative Movement")]
    [SerializeField] private Camera movementCamera;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float maximumSpeed = 6f;
    [SerializeField, Min(0f)] private float acceleration = 35f;
    [SerializeField, Min(0f)] private float deceleration = 45f;
    [SerializeField, Min(0f)] private float rotationSpeed = 720f;

    private Rigidbody body;
    private InputAction moveAction;
    private Vector2 moveInput;
    private Vector3 lastMoveDirection;

    public InputActionAsset InputActions
    {
        get { return inputActions; }
        set
        {
            if (inputActions == value)
            {
                return;
            }

            bool wasEnabled = isActiveAndEnabled;
            if (wasEnabled)
            {
                DisableMoveAction();
            }

            inputActions = value;
            ResolveMoveAction();

            if (wasEnabled)
            {
                EnableMoveAction();
            }
        }
    }

    public Camera MovementCamera
    {
        get { return movementCamera; }
        set { movementCamera = value; }
    }

    public float MaximumSpeed
    {
        get { return maximumSpeed; }
        set { maximumSpeed = Mathf.Max(0f, value); }
    }

    public float Acceleration
    {
        get { return acceleration; }
        set { acceleration = Mathf.Max(0f, value); }
    }

    public float Deceleration
    {
        get { return deceleration; }
        set { deceleration = Mathf.Max(0f, value); }
    }

    public float RotationSpeed
    {
        get { return rotationSpeed; }
        set { rotationSpeed = Mathf.Max(0f, value); }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        ResolveMoveAction();

        if (movementCamera == null)
        {
            movementCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        ResolveMoveAction();
        EnableMoveAction();
    }

    private void OnDisable()
    {
        DisableMoveAction();
        moveInput = Vector2.zero;
    }

    private void Update()
    {
        moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private void FixedUpdate()
    {
        Vector3 desiredDirection = GetCameraRelativeDirection(moveInput);
        Vector3 currentVelocity = body.linearVelocity;
        Vector3 currentHorizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        Vector3 targetHorizontalVelocity = desiredDirection * maximumSpeed;
        float rate = desiredDirection.sqrMagnitude > 0.0001f ? acceleration : deceleration;
        Vector3 nextHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetHorizontalVelocity,
            rate * Time.fixedDeltaTime);

        body.linearVelocity = new Vector3(nextHorizontalVelocity.x, currentVelocity.y, nextHorizontalVelocity.z);

        if (desiredDirection.sqrMagnitude > 0.0001f)
        {
            lastMoveDirection = desiredDirection;
            RotateTowards(lastMoveDirection);
        }
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (input.sqrMagnitude <= 0.0001f)
        {
            return Vector3.zero;
        }

        Transform referenceTransform = movementCamera != null ? movementCamera.transform : transform;
        Vector3 forward = referenceTransform.forward;
        Vector3 right = referenceTransform.right;
        forward.y = 0f;
        right.y = 0f;

        if (forward.sqrMagnitude <= 0.0001f)
        {
            forward = Vector3.forward;
        }

        if (right.sqrMagnitude <= 0.0001f)
        {
            right = Vector3.right;
        }

        forward.Normalize();
        right.Normalize();

        Vector3 direction = (forward * input.y) + (right * input.x);
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        return direction;
    }

    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        Quaternion nextRotation = Quaternion.RotateTowards(
            body.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime);

        body.MoveRotation(nextRotation);
    }

    private void ResolveMoveAction()
    {
        moveAction = null;
        if (inputActions == null || string.IsNullOrEmpty(actionMapName) || string.IsNullOrEmpty(moveActionName))
        {
            return;
        }

        InputActionMap actionMap = inputActions.FindActionMap(actionMapName, false);
        if (actionMap == null)
        {
            return;
        }

        moveAction = actionMap.FindAction(moveActionName, false);
    }

    private void EnableMoveAction()
    {
        if (moveAction != null && !moveAction.enabled)
        {
            moveAction.Enable();
        }
    }

    private void DisableMoveAction()
    {
        if (moveAction != null && moveAction.enabled)
        {
            moveAction.Disable();
        }
    }

    private void OnValidate()
    {
        maximumSpeed = Mathf.Max(0f, maximumSpeed);
        acceleration = Mathf.Max(0f, acceleration);
        deceleration = Mathf.Max(0f, deceleration);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
    }
}
