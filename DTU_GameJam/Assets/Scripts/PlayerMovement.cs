using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    Rigidbody2D rb;
    Vector2 input;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        var k = Keyboard.current;
        input = new Vector2(
            (k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0),
            (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0)
        ).normalized; // normalized so diagonals aren't faster
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }
}