using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;
    public int maxHits = 1; // Berapa musuh bisa kena sebelum peluru hancur
    public float destroyDelay = 0.5f; // Delay sebelum peluru hancur setelah kena musuh

    private int hitCount = 0;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed; // Atau sesuai arah spawn peluru
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Peluru bertabrakan dengan: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Musuh enemy = collision.gameObject.GetComponent<Musuh>();
            if (enemy != null)
            {
                enemy.TakeDamage(25);
                Debug.Log("Musuh terkena peluru. HP dikurangi.");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("Tag 'Enemy' ditemukan, tapi script Musuh tidak ada di objek!");
            }

            hitCount++;
            Debug.Log("Hit count peluru: " + hitCount);

            if (hitCount >= maxHits)
            {
                Debug.Log("Peluru mencapai batas hit. Menghentikan peluru dan menghancurkannya.");
                rb.velocity = Vector2.zero;
                rb.simulated = false;
                GetComponent<Collider2D>().enabled = false;
                StartCoroutine(DestroyAfterDelay());
            }
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Peluru menabrak obstacle. Dihancurkan.");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Peluru menabrak objek lain: " + collision.gameObject.tag);
        }
    }



    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
