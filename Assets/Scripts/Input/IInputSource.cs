// The seam. No UnityEngine.Input in it, so tests can drive a run.
public interface IInputSource
{
    float Horizontal { get; }
    bool JumpPressed { get; }
    bool JumpReleased { get; }
    bool RestartPressed { get; }
    bool RestartHeld { get; }
    bool LookingBack { get; }
    bool BankPressed { get; }
    UnityEngine.Vector2 Pointer { get; }   // screen pixels, origin bottom left
    bool PointerPressed { get; }
}
