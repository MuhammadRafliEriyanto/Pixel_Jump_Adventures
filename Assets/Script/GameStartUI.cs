using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartUI : MonoBehaviour
{
    public GameObject gameStartPanel;      // Panel pemberitahuan
    public GameObject playerController;    // Referensi ke Player

    private void Start()
    {
        // Saat mulai, tampilkan panel dan nonaktifkan player
        gameStartPanel.SetActive(true);
        playerController.SetActive(false);
    }

    // Dipanggil oleh tombol "Play"
    public void OnPlayButtonClicked()
    {
        gameStartPanel.SetActive(false);         // Sembunyikan panel
        playerController.SetActive(true);        // Aktifkan player
    }

    // Dipanggil oleh tombol "Main Menu"
    public void OnQuitButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");      // Ganti sesuai nama scene Main Menu
    }
}