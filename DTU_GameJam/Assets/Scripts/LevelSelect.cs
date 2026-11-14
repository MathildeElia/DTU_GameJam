using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    [Tooltip("Buttons in order: level 1 to 6")]
    public Button[] levelButtons;

    void Start()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            string sceneName = $"Level_{i + 1:00}";   // Level_01, Level_02, ...
            Button button = levelButtons[i];

            // Enabled only if that scene exists in the Build Profiles scene list
            button.interactable = Application.CanStreamedLevelBeLoaded(sceneName);

            button.onClick.AddListener(() => SceneManager.LoadScene(sceneName));
        }
    }

    public void Back() => SceneManager.LoadScene("MainMenu");
}