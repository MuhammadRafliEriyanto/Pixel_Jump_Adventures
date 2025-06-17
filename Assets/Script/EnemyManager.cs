using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour
{
    public GameObject berhasilPanel; // Panel yang muncul saat berhasil

    private void Start()
    {
        // Matikan panel di awal permainan
        if (berhasilPanel != null)
        {
            berhasilPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Cek jumlah musuh tersisa
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Jika semua musuh sudah mati dan panel belum muncul
        if (remainingEnemies.Length == 0 && berhasilPanel != null && !berhasilPanel.activeSelf)
        {
            berhasilPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game saat berhasil
        }
    }

    // Dipanggil tombol "Lanjut"
    public void LanjutKeLevelBerikutnya()
    {
        Time.timeScale = 1f;

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Jika masih ada level berikutnya
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Jika sudah di level terakhir, kembali ke Main Menu
            SceneManager.LoadScene("MainMenu");
        }
    }

    // Dipanggil tombol "Quit" untuk kembali ke Main Menu
    public void KembaliKeMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
