using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private PlayerController playerController;

    private Vector2 moveInput;
    private bool isJumping = false;
    private bool isShooting = false;

    [Header("Jump Settings")]
    [SerializeField] private LayerMask jumpableGround;
    private BoxCollider2D coll;

    private Vector3 originalScale;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;  // Assign prefab peluru di Inspector
    public Transform firePoint;      // Assign fire point di Inspector (posisi spawn peluru)
    public float bulletSpeed = 10f;  // Kecepatan peluru

    private enum MovementState { idle, run, jump, shoot, defeat }

    // === Tambahan untuk mobile input ===
    private float mobileInputX = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        playerController = new PlayerController();

        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        playerController.Enable();

        playerController.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerController.Movement.Move.canceled += ctx => moveInput = Vector2.zero;

        playerController.Movement.Jump.performed += ctx => Jump();
        playerController.Movement.Shoot.performed += ctx => Shoot();
    }

    private void OnDisable()
    {
        playerController.Disable();
    }

    private void FixedUpdate()
    {
        // === Tambahan untuk gabungkan input keyboard dan mobile ===
        float combinedInputX = Mathf.Abs(moveInput.x) > 0 ? moveInput.x : mobileInputX;

        Vector2 targetVelocity = new Vector2(combinedInputX * moveSpeed, rb.velocity.y);
        rb.velocity = targetVelocity;

        // Flip sprite dengan aman tanpa ubah ukuran
        float direction = combinedInputX;

        if (direction != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(direction) * originalScale.x,
                originalScale.y,
                originalScale.z
            );
        }
        else
        {
            transform.localScale = new Vector3(
                transform.localScale.x > 0 ? originalScale.x : -originalScale.x,
                originalScale.y,
                originalScale.z
            );
        }

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        MovementState state;

        if (isShooting)
        {
            state = MovementState.shoot;
        }
        else if (rb.velocity.y > 0.1f || rb.velocity.y < -0.1f)
        {
            state = MovementState.jump;
        }
        else if (Mathf.Abs(moveInput.x) > 0.1f || Mathf.Abs(mobileInputX) > 0.1f)
        {
            state = MovementState.run;
        }
        else
        {
            state = MovementState.idle;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    private void Jump()
    {
        if (isGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void Shoot()
    {
        StartCoroutine(ShootAnimation());

        Debug.Log("Tembak!");

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();

            if (rbBullet != null)
            {
                float direction = Mathf.Sign(transform.localScale.x);
                rbBullet.velocity = new Vector2(bulletSpeed * direction, 0f);
            }
        }
        else
        {
            Debug.LogWarning("BulletPrefab atau FirePoint belum di-assign di Inspector!");
        }
    }

    private IEnumerator ShootAnimation()
    {
        isShooting = true;
        yield return new WaitForSeconds(0.3f);
        isShooting = false;
    }

    // === Fungsi tambahan untuk kontrol tombol mobile ===
    public void MoveRight(bool isPressed)
    {
        if (isPressed)
            mobileInputX = 1f;
        else if (mobileInputX == 1f)
            mobileInputX = 0f;
    }

    public void MoveLeft(bool isPressed)
    {
        if (isPressed)
            mobileInputX = -1f;
        else if (mobileInputX == -1f)
            mobileInputX = 0f;
    }

    public void MobileJump()
    {
        if (isGrounded())
        {
            Jump();
        }
    }
}
