using Events;
using UnityEngine;

// On-rails runner. Kinematic along the track so movement and the jump arc can't fight.
[RequireComponent(typeof(Rigidbody))]
public class PlayerTrackMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float startingSpeed = 8f;
    [SerializeField] private float minSpeed = 4f;
    [SerializeField] private float maxSpeed = 22f;
    [SerializeField] private float acceleration = 1.4f;   // ramp toward maxSpeed, units/s^2
    [SerializeField] private float turnSpeed = 200f;      // deg/s the heading follows the track

    [Header("Strafe")]
    [SerializeField] private float strafeSpeed = 8f;
    [SerializeField] private float strafeAccel = 60f;     // units/s^2, ramp in and out
    [SerializeField] private float strafeLimit = 3f;      // half the usable track width
    [SerializeField] private float strafeTiltDegrees = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2.2f;
    [SerializeField] private float jumpUpTime = 0.32f;    // time to apex on a full jump
    [SerializeField] private float jumpBuffer = 0.15f;    // press slightly early, still fires on land
    [SerializeField, Range(0.1f, 1f)] private float shortHopCut = 0.45f; // release early = shorter hop

    [Header("Feel")]
    [SerializeField] private float baseFov = 60f;
    [SerializeField] private float maxFovBoost = 18f;

    public static float CurrentSpeed { get; private set; }
    public static float DistanceCovered { get; private set; }
    public static Vector3 Position { get; private set; }

    private static TrackManager trackManager;
    private Rigidbody rb;
    private Camera camComponent;
    private Vector3 startingPosition;
    private Vector3 basePos;         // position on the track centre, before offsets
    private Quaternion trackRot;     // clean heading, kept separate from the display bank

    private float strafeOffset;
    private float strafeVel;
    private float tilt;
    private float jumpOffset;
    private float jumpVy;
    private bool airborne;
    private float bufferedJumpAt = -999f;

    private float Gravity => (2f * jumpHeight) / (jumpUpTime * jumpUpTime);
    private float InitialJumpVy => Gravity * jumpUpTime;
    private float SpeedT => Mathf.InverseLerp(startingSpeed, maxSpeed, CurrentSpeed);

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // we drive the transform; keep triggers, drop the physics fight
        trackManager = TrackManager.GetInstance();
        startingPosition = transform.position;
        basePos = transform.position;
        trackRot = transform.rotation;
        CurrentSpeed = startingSpeed;

        if (Camera.main != null)
        {
            camComponent = Camera.main;
        }
    }

    private void Update()
    {
        var dt = Time.deltaTime;

        HandleJump(dt);
        AdvanceAlongTrack(dt);
        HandleStrafe(dt);

        transform.SetPositionAndRotation(
            basePos + (trackRot * Vector3.right) * strafeOffset + Vector3.up * jumpOffset,
            trackRot * Quaternion.Euler(0f, 0f, -tilt));

        ApplyFeel(dt);

        Position = transform.position;
        DistanceCovered += CurrentSpeed * dt;

        if (Input.GetKeyDown(KeyCode.R))
        {
            KillPlayer();
        }
    }

    private void HandleJump(float dt)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bufferedJumpAt = Time.time;
        }

        if (!airborne && Time.time - bufferedJumpAt <= jumpBuffer)
        {
            airborne = true;
            jumpVy = InitialJumpVy;
            bufferedJumpAt = -999f;
        }

        if (Input.GetKeyUp(KeyCode.Space) && airborne && jumpVy > 0f)
        {
            jumpVy *= shortHopCut;
        }

        if (airborne)
        {
            jumpVy -= Gravity * dt;
            jumpOffset += jumpVy * dt;
            if (jumpOffset <= 0f)
            {
                jumpOffset = 0f;
                jumpVy = 0f;
                airborne = false;
            }
        }
    }

    private void AdvanceAlongTrack(float dt)
    {
        CurrentSpeed = Mathf.Clamp(Mathf.MoveTowards(CurrentSpeed, maxSpeed, acceleration * dt), minSpeed, maxSpeed);

        var piece = trackManager.GetClosestPiece(basePos);
        if (piece == null)
        {
            return;
        }

        basePos = Vector3.MoveTowards(basePos, piece.GetEndPosition(), CurrentSpeed * dt);
        trackRot = Quaternion.RotateTowards(trackRot, piece.transform.rotation, turnSpeed * dt);

        if (basePos.ApproximatelyEquals(piece.GetEndPosition()))
        {
            piece.Passed = true;
        }
    }

    private void HandleStrafe(float dt)
    {
        var input = Input.GetAxisRaw("Horizontal"); // A/D + arrows, decoupled from look
        strafeVel = Mathf.MoveTowards(strafeVel, input * strafeSpeed, strafeAccel * dt);
        strafeOffset = Mathf.Clamp(strafeOffset + strafeVel * dt, -strafeLimit, strafeLimit);

        // stop the velocity winding up while pinned against a rail
        if (Mathf.Abs(strafeOffset) >= strafeLimit)
        {
            strafeVel = 0f;
        }

        // bank off actual movement, not raw input, so it eases with the strafe
        var lean = strafeSpeed > 0f ? strafeVel / strafeSpeed : 0f;
        tilt = Mathf.Lerp(tilt, lean * strafeTiltDegrees, 10f * dt);
    }

    private void ApplyFeel(float dt)
    {
        if (camComponent != null)
        {
            camComponent.fieldOfView = Mathf.Lerp(camComponent.fieldOfView, baseFov + maxFovBoost * SpeedT, 5f * dt);
        }
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
                CurrentSpeed = Mathf.Clamp(CurrentSpeed + result, minSpeed, maxSpeed);
            }
        }
    }

    private void KillPlayer()
    {
        GameManager.EventService.Dispatch(new OnDeathEvent(DistanceCovered));
        basePos = startingPosition;
        trackRot = Quaternion.identity;
        strafeOffset = 0f;
        jumpOffset = 0f;
        jumpVy = 0f;
        airborne = false;
        DistanceCovered = 0f;
        CurrentSpeed = startingSpeed;
        transform.SetPositionAndRotation(startingPosition, Quaternion.identity);
    }
}
