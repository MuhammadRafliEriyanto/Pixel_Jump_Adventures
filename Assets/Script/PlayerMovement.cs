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

    public CoinManager cm; // <- pastikan ini sudah diisi di Inspector

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

    [Header("Pause UI")]
    // Ganti pauseMenuUI (full screen) dengan pauseSnackbar (panel kecil)
    public RectTransform pauseSnackbar; // Assign panel snackbar di Inspector

    private bool isPaused = false;

    private enum MovementState { idle, run, jump, shoot, defeat }

    private float mobileInputX = 0f;

    private Coroutine snackbarCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        playerController = new PlayerController();

        originalScale = transform.localScale;

        // Pastikan snackbar awalnya tersembunyi dan di posisi hidden
        if (pauseSnackbar != null)
        {
            pauseSnackbar.gameObject.SetActive(false);
            // Posisi di bawah layar, misal
            pauseSnackbar.anchoredPosition = new Vector2(pauseSnackbar.anchoredPosition.x, -pauseSnackbar.rect.height);
        }
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
        float combinedInputX = Mathf.Abs(moveInput.x) > 0 ? moveInput.x : mobileInputX;

        Vector2 targetVelocity = new Vector2(combinedInputX * moveSpeed, rb.velocity.y);
        rb.velocity = targetVelocity;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            cm.coinCount++;
        }
    }

    private IEnumerator ShootAnimation()
    {
        isShooting = true;
        yield return new WaitForSeconds(0.3f);
        isShooting = false;
    }

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

    public void MobileShoot()
    {
        Shoot();
    }

    public void OnPauseButtonPressed()
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    // 1. Tambah debug di PauseGame dan SlideSnackbar
    public void ResumeGame()
    {
        Debug.Log("ResumeGame called");
        Time.timeScale = 1f;
        if (snackbarCoroutine != null) StopCoroutine(snackbarCoroutine);
        if (pauseSnackbar != null)
            snackbarCoroutine = StartCoroutine(SlideSnackbar(false));
        isPaused = false;
    }

    public void PauseGame()
    {
        Debug.Log("PauseGame called");
        Time.timeScale = 0f;
        if (snackbarCoroutine != null) StopCoroutine(snackbarCoroutine);
        if (pauseSnackbar != null)
            snackbarCoroutine = StartCoroutine(SlideSnackbar(true));
        isPaused = true;
    }

    private IEnumerator SlideSnackbar(bool show)
    {
        Debug.Log("Starting SlideSnackbar coroutine with show = " + show);
        float duration = 0.3f;
        // Kalau pauseSnackbar.rect.height = 0 (sering terjadi kalau UI belum siap), ganti dengan nilai tetap, misal -200
        float hiddenY = pauseSnackbar.rect.height > 0 ? -pauseSnackbar.rect.height : -200f;

        Vector2 hiddenPos = new Vector2(pauseSnackbar.anchoredPosition.x, hiddenY);
        Vector2 visiblePos = new Vector2(pauseSnackbar.anchoredPosition.x, 20f);

        Vector2 startPos = show ? hiddenPos : visiblePos;
        Vector2 endPos = show ? visiblePos : hiddenPos;

        float elapsed = 0f;

        if (show)
        {
            Debug.Log("Activating snackbar");
            pauseSnackbar.gameObject.SetActive(true);
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            pauseSnackbar.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        pauseSnackbar.anchoredPosition = endPos;

        if (!show)
        {
            pauseSnackbar.gameObject.SetActive(false);
        }
    }

    // 2. Tambah method Resume dari snackbar button
    public void OnResumeButtonPressed()
    {
        ResumeGame();
    }

}
