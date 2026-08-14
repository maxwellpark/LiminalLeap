// The seam. Everything gameplay needs from the player, with no UnityEngine.Input in it,
// so tests can drive a run and #7 becomes a second implementation rather than a rewrite.
public interface IInputSource
{
    float Horizontal { get; }
    bool JumpPressed { get; }
    bool JumpReleased { get; }
    bool RestartPressed { get; }
}
