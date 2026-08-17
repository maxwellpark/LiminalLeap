using System;

public enum AttackLane
{
    Left = 0,
    Centre = 1,
    Right = 2,
}

public enum AttackPhase
{
    Idle,
    Warning,
    Telegraph,
    Locked,
    Fire,
    Resolve,
    Cooldown,
}

// Every gameplay constant for the attack lives here so tuning never means a recompile.
[Serializable]
public class PursuerAttackConfig
{
    public float MinAttackInterval = 5f;
    public float MaxAttackInterval = 9f;

    public float WarningDuration = 1f;      // audio only, lane not chosen yet
    public float TelegraphDuration = 0.9f;  // lane readable in the mirror
    public float LockDuration = 0.5f;       // target frozen, last chance to move
    public float FireDuration = 0.35f;
    public float CooldownDuration = 1.4f;

    public float LaneSpacing = 2f;          // centre to centre, inside the 3 unit strafe limit
    public float LaneHalfWidth = 1.1f;      // how wide the beam bites
    public float DodgeTolerance = 0.25f;    // shaves the hit box, so a near thing counts as clear

    public float[] LaneWeights = { 1f, 1f, 1f };

    public float PursuerSetbackOnDodge = 6f;
    public float ScoreRewardOnDodge = 250f;

    // Total time from the warning starting to the beam landing.
    public float LeadTime => WarningDuration + TelegraphDuration + LockDuration + FireDuration;
}
