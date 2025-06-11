using UnityEngine;

public class Musuh : MonoBehaviour
{
    public int health = 100;

    // Fungsi untuk menerima damage dari player
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy took " + damage + " damage. Remaining health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    // Fungsi ketika musuh mati
    private void Die()
    {
        Debug.Log("Enemy died.");
        Destroy(gameObject); // Hapus musuh dari scene
    }
}
