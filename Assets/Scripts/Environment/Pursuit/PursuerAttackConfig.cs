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

    // Once the mirror stopped being a defence, a steady close rate was an unwinnable timer:
    // there was nothing left you could do about it. The fight is the attacks now, so it
    // drifts in slowly and a dodge has to be worth more than a cycle's drift.
    public float CloseRateDuringAttacks = 0.9f;

    // Less than the close rate would cancel: at full speed the old relief made it literally
    // uncatchable, which pinned Proximity at zero and muted the dread it drives.
    public float SpeedReliefDuringAttacks = 0.7f;

    // Has to clear a whole cycle's drift at a standstill, not just match it, or crawling
    // along dodging perfectly still slowly loses.
    public float PursuerSetbackOnDodge = 12f;
    public float ScoreRewardOnDodge = 250f;

    // One warning to the next, so the economy can be checked against it.
    public float CycleTime =>
        (MinAttackInterval + MaxAttackInterval) * 0.5f + LeadTime + CooldownDuration;

    // Total time from the warning starting to the beam landing.
    public float LeadTime => WarningDuration + TelegraphDuration + LockDuration + FireDuration;
}
