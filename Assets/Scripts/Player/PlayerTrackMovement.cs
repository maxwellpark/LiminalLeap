using Events;
using UnityEngine;

// On-rails runner. Kinematic along the track so movement and the jump arc can't fight.
[RequireComponent(typeof(Rigidbody))]
public class PlayerTrackMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float startingSpeed = 8f;
    [SerializeField] private float minSpeed = 4f;
    [SerializeField] private float maxSpeed = 32f;
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
    [SerializeField] private float maxFovBoost = 24f;

    [Header("Juice")]
    [SerializeField] private float pickupFovKick = 7f;
    [SerializeField] private float fovKickDecay = 4f;
    [SerializeField] private CameraShakeSettings deathShake = new() { Amplitude = 0.35f, Duration = 0.5f };
    [SerializeField] private float deathTimeScale = 0.25f;
    [SerializeField] private float deathPause = 0.75f;
    [SerializeField] private float bobHeight = 0.07f;
    [SerializeField] private float bobStridesPerSecond = 3.4f; // at full speed
    [SerializeField] private float bobRollDegrees = 0.9f;
    [SerializeField] private float landingDip = 0.12f;

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
    private float bobPhase;
    private float bobOffset;
    private float bobRoll;
    private float dip;
    private float travelledThisFrame;
    private float fovBase;
    private float fovKick;
    private bool dying;
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
        if (dying)
        {
            return;
        }

        var dt = Time.deltaTime;

        HandleJump(dt);
        AdvanceAlongTrack(dt);
        HandleStrafe(dt);

        HandleBob(dt);

        transform.SetPositionAndRotation(
            basePos + (trackRot * Vector3.right) * strafeOffset + Vector3.up * (jumpOffset + bobOffset - dip),
            trackRot * Quaternion.Euler(0f, 0f, -tilt + bobRoll));

        ApplyFeel(dt);

        Position = transform.position;

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
            AudioManager.GetInstance().Play(Sound.Jump);
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
                dip = landingDip;
                AudioManager.GetInstance().Play(Sound.Land);
            }
        }
    }

    private void AdvanceAlongTrack(float dt)
    {
        // nothing to run along, so don't ramp speed or bank distance either
        travelledThisFrame = 0f;

        var piece = trackManager.GetClosestPiece(basePos);
        if (piece == null)
        {
            return;
        }

        CurrentSpeed = Mathf.Clamp(Mathf.MoveTowards(CurrentSpeed, maxSpeed, acceleration * dt), minSpeed, maxSpeed);

        var before = basePos;
        basePos = Vector3.MoveTowards(basePos, piece.GetEndPosition(), CurrentSpeed * dt);
        travelledThisFrame = (basePos - before).magnitude;
        DistanceCovered += travelledThisFrame;
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

    // Bob is what sells running. Driven off travelled distance, not time, so it
    // stays in step with the speed and stops dead when the track runs out.
    private void HandleBob(float dt)
    {
        if (airborne)
        {
            bobOffset = Mathf.Lerp(bobOffset, 0f, 12f * dt);
            bobRoll = Mathf.Lerp(bobRoll, 0f, 12f * dt);
        }
        else
        {
            bobPhase += travelledThisFrame * bobStridesPerSecond;
            bobOffset = -Mathf.Abs(Mathf.Sin(bobPhase)) * bobHeight * SpeedT;
            bobRoll = Mathf.Cos(bobPhase * 0.5f) * bobRollDegrees * SpeedT;
        }

        dip = Mathf.Lerp(dip, 0f, 8f * dt);
    }

    private void ApplyFeel(float dt)
    {
        if (camComponent == null)
        {
            return;
        }

        // Base and kick tracked separately: lerping the camera's own value would eat
        // last frame's kick and the punch would never read.
        fovBase = Mathf.Lerp(fovBase, baseFov + maxFovBoost * SpeedT, 5f * dt);
        fovKick = Mathf.Lerp(fovKick, 0f, fovKickDecay * dt);
        camComponent.fieldOfView = fovBase + fovKick;
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
                fovKick = pickupFovKick;
            }
        }
    }

    private void KillPlayer()
    {
        if (!dying)
        {
            StartCoroutine(DeathSequence());
        }
    }

    // Shake, slow down, wipe to black, then reset behind the wipe so the respawn
    // isn't a teleport in your face.
    private System.Collections.IEnumerator DeathSequence()
    {
        dying = true;
        GameManager.EventService.Dispatch(new OnDeathEvent(DistanceCovered));
        CameraManager.GetInstance().Shake(deathShake);
        ScreenFade.GetInstance().To(1f, deathPause * 0.8f);
        Time.timeScale = deathTimeScale;

        // Realtime, or the pause would stretch by however much we slowed the game.
        yield return new WaitForSecondsRealtime(deathPause);

        Time.timeScale = 1f;
        ResetRun();
        ScreenFade.GetInstance().To(0f, 0.35f);
        dying = false;
    }

    // Nothing should be able to leave the game in slow motion.
    private void OnDisable()
    {
        if (dying)
        {
            Time.timeScale = 1f;
            dying = false;
        }
    }

    private void ResetRun()
    {
        trackManager.ResetRun();
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
