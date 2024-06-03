using Events;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float minJumpForce = 2f;
    [SerializeField] private float maxJumpForce = 5f;
    [SerializeField] private float maxJumpHoldDuration = 0.2f;
    [SerializeField] private float startingSpeed = 5f;
    [SerializeField] private float minSpeed = 2f;
    //[SerializeField] private float mouseRotationSpeed = 10f;
    //[SerializeField] private float strafeSpeed = 5f;
    [SerializeField] private float speedIncreasePerJump = 0.5f;
    [SerializeField] private float speedDecreaseRate = 1f;
    [SerializeField] private float speedDecreaseDelay = 2f;
    [SerializeField] private float jumpBufferWindow = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isGrounded;

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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = transform.position;
        CurrentSpeed = startingSpeed;
        lastJumpTime = Time.time;
    }

    private void Update()
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

            if (Input.GetKeyUp(KeyCode.Space)/* || jumpHoldDuration > maxJumpHoldDuration*/)
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
            // Jump start time includes buffer start time 
            jumpStartTime = Time.time - bufferedJumpTime;
            holdingJump = true;
            bufferedJump = false;
        }

        if (Time.time - lastJumpTime > speedDecreaseDelay)
        {
            CurrentSpeed = Mathf.Max(minSpeed, CurrentSpeed * Mathf.Exp(-speedDecreaseRate * Time.deltaTime));
        }

        // Mouse strafing 
        //var mouseX = Input.GetAxis("Mouse X");
        //var xMovement = mouseX * Time.deltaTime * strafeSpeed * transform.right;
        //transform.position += xMovement;

        //transform.position += new Vector3(transform.position.x, transform.position.y, transform.position.z + Time.deltaTime * CurrentSpeed);

        Position = transform.position;
        DistanceCovered = Vector3.Distance(Position, startingPosition);

        if (Input.GetKeyDown(KeyCode.R))
        {
            KillPlayer();
        }
    }

    private void Jump(float force)
    {
        Debug.Log("Jumping with force: " + force);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f, groundLayer);
        var velocity = new Vector3(rb.velocity.x, rb.velocity.y, CurrentSpeed);
        rb.velocity = velocity;
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
