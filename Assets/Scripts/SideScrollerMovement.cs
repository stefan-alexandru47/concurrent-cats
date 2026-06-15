using UnityEngine;
// 1. We need to include the New Input System namespace
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class SideScrollerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float gravityScale = 5f;
    public float wallSlideSpeed = 2f;

    [Header("Smoothness Settings")]
    [Tooltip("How fast the character accelerates (Ease-In)")]
    public float acceleration = 10f;
    [Tooltip("How fast the character decelerates (Ease-Out)")]
    public float deceleration = 10f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float rawInputX;
    private float smoothedInputX;
    private bool jumpRequested;
    private bool isGrounded;
    private bool isFacingLeft = false; // Starts facing right (default idle is right)
    private bool isWallSliding;
    private string currentAnimationState;

    // Contact points list to check grounding
    private ContactPoint2D[] contacts = new ContactPoint2D[10];

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;

        // Automatically ensure Player has a collider component so collisions register
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

        // Force-refresh all colliders in the scene to ensure they register correctly with the physics engine
        foreach (var col in FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
        {
            if (col != null)
            {
                col.enabled = false;
                col.enabled = true;
            }
        }
    }

    void Update()
    {
        // 2. Read the keyboard/arrow input using the New Input System
        if (Keyboard.current != null)
        {
            float moveX = 0f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;

            rawInputX = moveX;

            // Trigger jump when Up or W is pressed this frame
            if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                jumpRequested = true;
            }
        }

        // Update facing direction based on input
        if (rawInputX < 0)
        {
            isFacingLeft = true;
        }
        else if (rawInputX > 0)
        {
            isFacingLeft = false;
        }

        // 3. Smooth ease-in / ease-out for horizontal movement
        float targetAcceleration = Mathf.Abs(rawInputX) > 0 ? acceleration : deceleration;
        smoothedInputX = Mathf.MoveTowards(smoothedInputX, rawInputX, targetAcceleration * Time.deltaTime);

        // 4. Update grounded state
        isGrounded = CheckGrounded();

        // 5. Update animation and sprite flipping
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        float targetVelocityX = smoothedInputX * moveSpeed;
        bool isBlocked = IsBlocked(targetVelocityX);

        isWallSliding = false;

        // Prevent applying horizontal velocity if we are blocked by a wall in that direction
        if (isBlocked)
        {
            targetVelocityX = 0f;

            // If we are in mid-air and falling down against a wall, slide down linearly (constant velocity)
            if (!isGrounded && rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity = new Vector2(targetVelocityX, -wallSlideSpeed);
                isWallSliding = true;
            }
            else
            {
                rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
            }
        }
        else
        {
            // 6. Move horizontally, preserving vertical velocity
            rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
        }

        // 7. Apply jump if requested and grounded (or blocked by wall)
        if (jumpRequested)
        {
            if (isGrounded || isBlocked)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            // Consume the jump request
            jumpRequested = false;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null || spriteRenderer == null) return;

        if (isWallSliding)
        {
            // Sliding on a wall
            PlayAnimation("kitty-slide");
            spriteRenderer.flipX = isFacingLeft; // Natively faces right, so flip if facing left (isFacingLeft = true)
        }
        else if (!isGrounded)
        {
            // Jumping or in mid-air
            PlayAnimation("kitty-jump");
            spriteRenderer.flipX = !isFacingLeft; // Natively faces left, so flip if facing right (isFacingLeft = false)
        }
        else if (rawInputX != 0)
        {
            // Running on the ground
            PlayAnimation("kitty-run");
            spriteRenderer.flipX = !isFacingLeft; // Natively faces left, so flip if running right (isFacingLeft = false)
        }
        else
        {
            // Idling on the ground
            PlayAnimation("kitty-idle");
            spriteRenderer.flipX = isFacingLeft; // Natively faces right, so flip if idling left (isFacingLeft = true)
        }
    }

    private void PlayAnimation(string newState)
    {
        if (currentAnimationState == newState) return;

        animator.Play(newState);
        currentAnimationState = newState;
    }

    private bool CheckGrounded()
    {
        int contactCount = rb.GetContacts(contacts);
        for (int i = 0; i < contactCount; i++)
        {
            // If contact normal is pointing mostly upwards (normal.y > 0.7f), we are on ground
            if (contacts[i].normal.y > 0.7f)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsBlocked(float directionX)
    {
        if (Mathf.Abs(directionX) < 0.01f) return false;

        int contactCount = rb.GetContacts(contacts);
        for (int i = 0; i < contactCount; i++)
        {
            Vector2 normal = contacts[i].normal;
            
            // Only check contacts that are not considered walkable ground (normal.y <= 0.7f)
            if (normal.y <= 0.7f)
            {
                // Wall on the right (normal points left, i.e., normal.x < -0.7f) and we want to move right
                if (directionX > 0f && normal.x < -0.7f)
                {
                    return true;
                }
                // Wall on the left (normal points right, i.e., normal.x > 0.7f) and we want to move left
                if (directionX < 0f && normal.x > 0.7f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}