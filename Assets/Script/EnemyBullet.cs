using System.Collections;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 10;
    public float destroyDelay = 0.05f;

    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Enemy Bullet trigger with: " + other.name);

        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                Vector2 direction = (other.transform.position - transform.position).normalized;
                player.TakeDamage(damage, direction);
                Debug.Log("Player kena peluru musuh! Damage: " + damage);
            }
            StartCoroutine(DestroyAfterDelay());
        }
        else if (other.CompareTag("Obstacle"))
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        if (rb != null) rb.velocity = Vector2.zero;
        if (col != null) col.enabled = false;

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
