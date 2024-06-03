using Events;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float minJumpForce = 2f;
    [SerializeField] private float maxJumpForce = 5f;
    [SerializeField] private float maxJumpHoldDuration = 0.2f;
    [SerializeField] private float startingSpeed = 5f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float speedIncreasePerJump = 0.5f;
    [SerializeField] private float speedDecreaseRate = 1f;
    [SerializeField] private float speedDecreaseDelay = 2f;
    [SerializeField] private float jumpBufferWindow = 0.25f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isGrounded;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float maxLookAngle = 60f;

    private float jumpStartTime;
    private bool holdingJump;
    private Vector3 startingPosition;
    private Rigidbody rb;
    public static float CurrentSpeed { get; private set; }
    public static float DistanceCovered { get; private set; }
    public static Vector3 Position { get; private set; }

    private float lastJumpTime;
    private bool bufferedJump;
    private float bufferedJumpTime;
    private float verticalLookRotation;
    private float horizontalLookRotation;
    private Transform playerCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = transform.position;
        CurrentSpeed = startingSpeed;
        lastJumpTime = Time.time;
        playerCamera = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleJumpInput();
        HandleMovement();
        HandleMouseLook();

        Position = transform.position;
        DistanceCovered = Vector3.Distance(Position, startingPosition);

        if (Input.GetKeyDown(KeyCode.R))
        {
            KillPlayer();
        }
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                jumpStartTime = Time.time;
                holdingJump = true;
            }
            else
            {
                bufferedJump = true;
                bufferedJumpTime = Time.time;
            }
        }

        if (holdingJump)
        {
            var jumpHoldDuration = Time.time - jumpStartTime;

            if (Input.GetKeyUp(KeyCode.Space))
            {
                var jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, jumpHoldDuration / maxJumpHoldDuration);

                Jump(jumpForce);
                holdingJump = false;
                jumpStartTime = 0f;

                CurrentSpeed += speedIncreasePerJump;
                lastJumpTime = Time.time;
            }
        }

        if (bufferedJump && isGrounded && (Time.time - bufferedJumpTime <= jumpBufferWindow))
        {
            jumpStartTime = Time.time - bufferedJumpTime;
            holdingJump = true;
            bufferedJump = false;
        }

        if (Time.time - lastJumpTime > speedDecreaseDelay)
        {
            CurrentSpeed = Mathf.Max(minSpeed, CurrentSpeed * Mathf.Exp(-speedDecreaseRate * Time.deltaTime));
        }
    }

    private void HandleMovement()
    {
        var velocity = new Vector3(rb.velocity.x, rb.velocity.y, CurrentSpeed);
        rb.velocity = velocity;
    }

    private void HandleMouseLook()
    {
        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

        horizontalLookRotation += mouseX;
        horizontalLookRotation = Mathf.Clamp(horizontalLookRotation, -maxLookAngle, maxLookAngle);

        playerCamera.localRotation = Quaternion.Euler(verticalLookRotation, horizontalLookRotation, 0f);
        transform.localRotation = Quaternion.Euler(0f, horizontalLookRotation, 0f);
    }

    private void Jump(float force)
    {
        Debug.Log("Jumping with force: " + force);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f, groundLayer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KillFloor"))
        {
            KillPlayer();
        }

        if (other.TryGetComponent<ITriggerable>(out var triggerable))
        {
            var result = triggerable.Trigger();

            if (triggerable is SpeedTriggerable)
            {
                CurrentSpeed = Mathf.Max(CurrentSpeed + result, minSpeed);
            }
        }
    }

    private void KillPlayer()
    {
        Debug.Log("KillFloor trigger entered");
        GameManager.EventService.Dispatch(new OnDeathEvent(DistanceCovered));
        transform.SetPositionAndRotation(startingPosition, Quaternion.identity);
        DistanceCovered = 0f;
        CurrentSpeed = startingSpeed;
        lastJumpTime = Time.time;
    }
}
