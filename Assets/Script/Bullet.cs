using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;
    public float destroyDelay = 0.05f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;

        // Deteksi benturan lebih presisi
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Peluru trigger dengan: " + other.gameObject.name);

        if (other.CompareTag("Enemy"))
        {
            Musuh enemy = other.GetComponent<Musuh>();

            if (enemy != null)
            {
                int damageAmount = 25;
                enemy.TakeDamage(damageAmount);
                Debug.Log("Musuh terkena peluru dan menerima damage: " + damageAmount);
            }
            else
            {
                Debug.LogWarning("Tag Enemy terdeteksi tapi tidak ditemukan komponen Musuh.");
            }

            StartCoroutine(DestroyAfterDelay());
        }
        else if (other.CompareTag("Enemy"))
        {
            Debug.Log("Peluru menabrak obstacle.");
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        rb.velocity = Vector2.zero;

        // Cegah tabrakan ganda
        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}