using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Proyecto26;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelButton
    {
        public Button button;
        public Image lockIcon; // Reference to the lock icon that's already a child of the button
        public TextMeshProUGUI highScoreText;
        public string levelName;
        public string sceneName;
    }

    [Header("Level Buttons")]
    [SerializeField] private List<LevelButton> levelButtons;
    [SerializeField] private ScrollRect scrollView;

    [Header("UI Buttons")]
    [SerializeField] private Button signOutButton;

    private void Start()
    {
        if (!UserSession.IsLoggedIn())
        {
            SceneManager.LoadScene("Login");
            return;
        }

        LoadUserProgress();

        if (signOutButton != null)
        {
            signOutButton.onClick.AddListener(SignOut);
        }
    }

    private void LoadUserProgress()
    {
        string url = $"http://localhost:3000/user/{UserSession.UserId}";
        
        RestClient.Get<UserProgress>(url)
            .Then(response => {
                UpdateLevelButtons(response.progress);
            })
            .Catch(error => {
                // Handle error silently
            });
    }

    private void UpdateLevelButtons(LevelProgress progress)
    {
        foreach (var levelButton in levelButtons)
        {
            LevelData levelData = GetLevelData(progress, levelButton.levelName);
            
            if (levelData != null)
            {
                levelButton.button.interactable = levelData.unlocked;
                
                if (levelButton.lockIcon != null)
                {
                    levelButton.lockIcon.gameObject.SetActive(!levelData.unlocked);
                }
                
                if (levelButton.highScoreText != null)
                {
                    if (levelData.high_score > 0)
                        levelButton.highScoreText.text = $"High Score: {levelData.high_score}";
                    else
                        levelButton.highScoreText.text = "No score yet";
                }

                if (levelData.unlocked)
                {
                    levelButton.button.onClick.RemoveAllListeners();
                    levelButton.button.onClick.AddListener(() => LoadLevel(levelButton.sceneName));
                }
            }
        }
    }

    private LevelData GetLevelData(LevelProgress progress, string levelName)
    {
        switch (levelName.ToUpper())
        {
            case "LEVEL1":
                return progress.LEVEL1;
            case "LEVEL2":
                return progress.LEVEL2;
            case "LEVEL3":
                return progress.LEVEL3;
            default:
                return null;
        }
    }

    private void LoadLevel(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void SignOut()
    {
        MenuSFXManager.Instance.PlayButtonClick();
        UserSession.ClearUserData();
        SceneManager.LoadScene("Login");
    }
} 