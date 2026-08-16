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
}

public interface IAudioLibrary
{
    UnityEngine.AudioClip Get(Sound sound);
}
