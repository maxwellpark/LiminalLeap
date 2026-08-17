using UnityEngine;

public class KeyboardInput : IInputSource
{
    public float Horizontal => Input.GetAxisRaw("Horizontal");
    public bool JumpPressed => Input.GetKeyDown(KeyCode.Space);
    public bool JumpReleased => Input.GetKeyUp(KeyCode.Space);
    public bool RestartPressed => Input.GetKeyDown(KeyCode.R);
    public bool LookingBack => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Q);
    public bool BankPressed => Input.GetKeyDown(KeyCode.E);
}
