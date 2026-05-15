using UnityEngine;
using UnityEngine.SceneManagement;

public class BrushEndScreen : MonoBehaviour
{
    [SerializeField] GameObject endScreenPanel;

    void Start()
    {
        if (endScreenPanel != null) endScreenPanel.SetActive(false);

        // Level 2 tongue game (click-based) is the true end of the game
        var tongue = FindObjectOfType<TongueGameManager>(true);
        if (tongue != null)
            tongue.TongueGameWon += ShowEndScreen;
    }

    void OnDestroy()
    {
        var tongue = FindObjectOfType<TongueGameManager>(true);
        if (tongue != null)
            tongue.TongueGameWon -= ShowEndScreen;
    }

    void ShowEndScreen()
    {
        if (endScreenPanel != null) endScreenPanel.SetActive(true);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
