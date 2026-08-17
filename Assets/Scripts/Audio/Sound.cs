public enum Sound
{
    Jump,
    Land,
    Pickup,
    Death,
    Wind,
    TitleSting,
    Confirm,
    MirrorUp,
    MirrorDown,
    Success,
    Dread,
    Approach,
    Lunge,
    AttackWarning,
    AttackCharge,
    AttackImminent,
    AttackFire,
    AttackDodge,
    ExitNear,
    Bank,
}

public interface IAudioLibrary
{
    UnityEngine.AudioClip Get(Sound sound);
}
