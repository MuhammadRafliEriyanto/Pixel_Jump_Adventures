using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartUI : MonoBehaviour
{
    public GameObject gameStartPanel;
    public GameObject playerController;

    private void Start()
    {
        if (PlayerPrefs.GetInt("SkipStartPanel", 0) == 1)
        {
            // Mati sebelumnya, langsung main tanpa panel
            gameStartPanel.SetActive(false);
            playerController.SetActive(true);
            PlayerPrefs.SetInt("SkipStartPanel", 0); // Reset
        }
        else
        {
            // Pertama kali main, tampilkan panel
            gameStartPanel.SetActive(true);
            playerController.SetActive(false);
        }
    }

    public void OnPlayButtonClicked()
    {
        gameStartPanel.SetActive(false);
        playerController.SetActive(true);
    }

    public void OnQuitButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}