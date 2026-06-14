using UnityEngine;
// 1. We need to include the New Input System namespace
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Smoothness Settings")]
    [Tooltip("How fast the character accelerates (Ease-In)")]
    public float acceleration = 10f;
    [Tooltip("How fast the character decelerates (Ease-Out)")]
    public float deceleration = 10f;

    private Rigidbody2D rb;
    private Vector2 rawInput;
    private Vector2 smoothedInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 2. Read the keyboard/arrow input using the New Input System
        // This looks at WASD and Arrow keys automatically if they are bound to a vector action
        if (Keyboard.current != null)
        {
            float moveX = 0f;
            float moveY = 0f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;

            rawInput = new Vector2(moveX, moveY);
        }

        // 3. Prevent diagonal speed boost
        if (rawInput.magnitude > 1f)
        {
            rawInput.Normalize();
        }

        // 4. Manually recreate the Smooth Ease-In / Ease-Out 
        // Because the New Input System snaps instantly to 1 or 0, we use Mathf.MoveTowards 
        // to smoothly slide our movement values over time.
        float targetAcceleration = rawInput.magnitude > 0 ? acceleration : deceleration;
        smoothedInput = Vector2.MoveTowards(smoothedInput, rawInput, targetAcceleration * Time.deltaTime);
    }

    void FixedUpdate()
    {
        // 5. Move using our beautifully smoothed vector
        rb.MovePosition(rb.position + smoothedInput * moveSpeed * Time.fixedDeltaTime);
    }
}