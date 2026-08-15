public enum Sound
{
    Jump,
    Land,
    Pickup,
    Death,
    Wind,
    TitleSting,
    Confirm,
}

public interface IAudioLibrary
{
    UnityEngine.AudioClip Get(Sound sound);
}
