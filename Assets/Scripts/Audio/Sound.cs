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
}

public interface IAudioLibrary
{
    UnityEngine.AudioClip Get(Sound sound);
}
