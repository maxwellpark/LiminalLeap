// The seam. No UnityEngine.Input in it, so tests can drive a run.
public interface IInputSource
{
    float Horizontal { get; }
    bool JumpPressed { get; }
    bool JumpReleased { get; }
    bool RestartPressed { get; }
    bool LookingBack { get; }
    bool BankPressed { get; }
}
