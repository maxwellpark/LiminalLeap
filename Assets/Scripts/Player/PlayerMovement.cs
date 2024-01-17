using Events;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float minJumpForce = 10f;
    [SerializeField] private float maxJumpForce = 20f;
    [SerializeField] private float maxJumpHoldDuration = 0.2f;
    [SerializeField] private float startingSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isGrounded;

    private float jumpStartTime;
    private bool holdingJump;
    private Vector3 startingPosition;
    private Rigidbody rb;
    public static float CurrentSpeed { get; private set; }
    public static float DistanceCovered { get; private set; }
    public static Vector3 Position { get; private set; }
    private static TrackManager trackManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        trackManager = TrackManager.GetInstance();
        startingPosition = transform.position;
        CurrentSpeed = startingSpeed;
    }

    private void Update()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            jumpStartTime = Time.time;
            holdingJump = true;
        }

        if (holdingJump)
        {
            var jumpHoldDuration = Time.time - jumpStartTime;

            if (Input.GetKeyUp(KeyCode.Space) || jumpHoldDuration > maxJumpHoldDuration)
            {
                var jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, jumpHoldDuration / maxJumpHoldDuration);

                Jump(jumpForce);
                holdingJump = false;
                jumpStartTime = 0f;
            }
        }

        var closestPiece = trackManager.GetClosestPiece(transform.position);
        var newPosition = Vector3.MoveTowards(transform.position, closestPiece.GetEndPosition(), CurrentSpeed * Time.deltaTime);
        var newRotation = Quaternion.RotateTowards(transform.rotation, closestPiece.transform.rotation, rotationSpeed * Time.deltaTime);
        transform.SetPositionAndRotation(newPosition, newRotation);

        if (transform.position.ApproximatelyEquals(closestPiece.GetEndPosition()))
        {
            closestPiece.Passed = true;
        }

        Position = transform.position;
        DistanceCovered = Vector3.Distance(Position, startingPosition);
        CurrentSpeed += Time.deltaTime;
    }

    private void Jump(float force)
    {
        Debug.Log("Jumping with force: " + force);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f, groundLayer);
        //var velocity = new Vector3(rb.velocity.x, rb.velocity.y, CurrentSpeed);
        //rb.velocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KillFloor"))
        {
            Debug.Log("KillFloor trigger entered");
            GameManager.EventService.Dispatch(new OnDeathEvent(DistanceCovered));
            transform.SetPositionAndRotation(startingPosition, Quaternion.identity);
            DistanceCovered = 0f;
        }

        if (other.TryGetComponent<ITriggerable>(out var triggerable))
        {
            var result = triggerable.Trigger();

            if (triggerable is SpeedTriggerable)
            {
                CurrentSpeed += result;
            }
        }
    }
}
