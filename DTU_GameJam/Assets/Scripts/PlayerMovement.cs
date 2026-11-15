using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 7f;

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;
    Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            input = Vector2.zero;
            return;
        }

        input = new Vector2(
            (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
            (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0)
        ).normalized;

        animator.SetFloat("MoveX", Mathf.Abs(input.x));
        animator.SetFloat("MoveY", input.y);
        if (keyboard.sKey.wasPressedThisFrame)
    Debug.Log($"S input: {input}; Animator MoveY: {animator.GetFloat("MoveY")}");

        if (input.x != 0)
            spriteRenderer.flipX = input.x < 0;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }
}