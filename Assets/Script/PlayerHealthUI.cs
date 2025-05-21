using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Slider healthSlider;

    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    void Update()
    {
        // Contoh pengurangan nyawa (bisa diganti sesuai game logic)
        if (Input.GetKeyDown(KeyCode.H))
        {
            currentHealth -= 10;
            if (currentHealth < 0) currentHealth = 0;
            healthSlider.value = currentHealth;
        }
    }
}
