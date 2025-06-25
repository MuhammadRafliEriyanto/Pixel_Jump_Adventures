using UnityEngine;
using UnityEngine.UI;

public class Musuh : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public Image healthBarFill;

    private EnemyShooter shooter;

    private void Start()
    {
        currentHealth = maxHealth;
        shooter = GetComponent<EnemyShooter>();
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Musuh terkena: " + damage + " HP sisa: " + currentHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float fillAmount = (float)currentHealth / maxHealth;
            healthBarFill.fillAmount = fillAmount;
        }
    }

    private void Die()
    {
        Debug.Log("Musuh mati.");
        // Matikan penembakan lebih dulu
        if (shooter != null) shooter.enabled = false;

        // Hancurkan musuh
        Destroy(gameObject);
    }
}
