using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float startingSpeed = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isGrounded;

    private float currentSpeed;
    private Vector3 startingPosition;
    private Rigidbody rb;
    public float DistanceCovered { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = transform.position;
        currentSpeed = startingSpeed;
    }

    private void Update()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        DistanceCovered = Vector3.Distance(transform.position, startingPosition);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f, groundLayer);
        var velocity = new Vector3(rb.velocity.x, rb.velocity.y, currentSpeed);
        rb.velocity = velocity;
    }

    private void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 48,
            fontStyle = FontStyle.Bold,
        };
        GUI.Label(new Rect(40, 40, 600, 60), $"Distance Covered: {DistanceCovered:F1} units", style);
    }
}
