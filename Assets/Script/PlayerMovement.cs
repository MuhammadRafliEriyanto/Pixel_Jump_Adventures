using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;

    public CoinManager cm;

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
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    [Header("Pause UI")]
    public RectTransform pauseSnackbar;

    private bool isPaused = false;

    private enum MovementState { idle, run, jump, shoot, defeat }

    private float mobileInputX = 0f;

    private Coroutine snackbarCoroutine;

    [Header("Health System")]
    public int maxHealth = 100;
    private int currentHealth;
    public TextMeshProUGUI healthText;

    [Header("Knockback Settings")]
    [SerializeField] private float knockBackTime = 0.2f;
    [SerializeField] private float knockBackThrust = 10f;

    private bool isKnockedBack = false;

    [Header("Fall Detection")]
    public float fallThresholdY = -10f;  // posisi Y jatuh (bisa diubah via Inspector)

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        playerController = new PlayerController();

        originalScale = transform.localScale;

        if (pauseSnackbar != null)
        {
            pauseSnackbar.gameObject.SetActive(false);
            pauseSnackbar.anchoredPosition = new Vector2(pauseSnackbar.anchoredPosition.x, -pauseSnackbar.rect.height);
        }

        currentHealth = maxHealth;
        UpdateHealthUI();
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

    private void Update()
    {
        // DETEKSI PLAYER JATUH
        if (transform.position.y < fallThresholdY)
        {
            RestartLevel();
        }
    }

    private void FixedUpdate()
    {
        if (isKnockedBack) return;

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

    public void TakeDamage(int damage, Vector2 direction)
    {
        if (isKnockedBack) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Player Mati");
        }

        StartCoroutine(HandleKnockback(direction.normalized));
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "Health: " + currentHealth;
    }

    private IEnumerator HandleKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;

        Vector2 force = direction * knockBackThrust * rb.mass;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockBackTime);

        rb.velocity = Vector2.zero;
        isKnockedBack = false;
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

        float directionX = moveInput.x != 0 ? moveInput.x : mobileInputX;
        if (directionX == 0)
            directionX = Mathf.Sign(transform.localScale.x);

        Vector2 shootDirection = new Vector2(Mathf.Sign(directionX), 0f);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        if (rbBullet != null)
        {
            rbBullet.velocity = shootDirection * bulletSpeed;
        }

        float rayDistance = 10f;
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, shootDirection, rayDistance, LayerMask.GetMask("Enemy", "Obstacle"));

        Debug.DrawRay(firePoint.position, shootDirection * rayDistance, Color.red, 1f);

        if (hit.collider != null)
        {
            var enemy = hit.collider.GetComponent<Musuh>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }
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

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (snackbarCoroutine != null) StopCoroutine(snackbarCoroutine);
        if (pauseSnackbar != null)
            snackbarCoroutine = StartCoroutine(SlideSnackbar(false));
        isPaused = false;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (snackbarCoroutine != null) StopCoroutine(snackbarCoroutine);
        if (pauseSnackbar != null)
            snackbarCoroutine = StartCoroutine(SlideSnackbar(true));
        isPaused = true;
    }

    private IEnumerator SlideSnackbar(bool show)
    {
        float duration = 0.3f;
        float hiddenY = pauseSnackbar.rect.height > 0 ? -pauseSnackbar.rect.height : -200f;

        Vector2 hiddenPos = new Vector2(pauseSnackbar.anchoredPosition.x, hiddenY);
        Vector2 visiblePos = new Vector2(pauseSnackbar.anchoredPosition.x, 20f);

        Vector2 startPos = show ? hiddenPos : visiblePos;
        Vector2 endPos = show ? visiblePos : hiddenPos;

        float elapsed = 0f;

        if (show)
            pauseSnackbar.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            pauseSnackbar.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        pauseSnackbar.anchoredPosition = endPos;

        if (!show)
            pauseSnackbar.gameObject.SetActive(false);
    }

    public void OnResumeButtonPressed()
    {
        ResumeGame();
    }

    // RESTART LEVEL SAAT JATUH
    private void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
