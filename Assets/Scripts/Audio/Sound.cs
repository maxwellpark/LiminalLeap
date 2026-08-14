public enum Sound
{
    Jump,
    Land,
    Pickup,
    Death,
    Wind,
}

public interface IAudioLibrary
{
    UnityEngine.AudioClip Get(Sound sound);
}
