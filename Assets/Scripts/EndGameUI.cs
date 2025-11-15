using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviour
{
    public TMP_Text resultText;
    public Button newGameButton;
    public Button quitButton;

    void Start()
    {
        string result = PlayerPrefs.GetString("GameResult", "Draw");
        resultText.text = result.ToUpper();

        if (result == "Victory")
            resultText.color = Color.green;
        else if (result == "Defeat")
            resultText.color = Color.red;
        else
            resultText.color = Color.yellow;

        newGameButton.onClick.AddListener(OnNewGame);
        quitButton.onClick.AddListener(OnQuit);
    }

    void OnNewGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void OnQuit()
    {
        Application.Quit();
        Debug.Log("[EndGameUI] Quit game pressed (will only exit in build).");
    }
}