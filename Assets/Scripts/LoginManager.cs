using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Proyecto26;
using System;
using System.Text;

public class LoginManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private TextMeshProUGUI errorMessageText;

    private void Start()
    {
        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);

        if (loginButton != null)
            loginButton.onClick.AddListener(AttemptLogin);

        if (createAccountButton != null)
            createAccountButton.onClick.AddListener(GoToCreateAccount);
    }

    private void AttemptLogin()
    {
        MenuSFXManager.Instance.PlayButtonClick();

        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);

        string username = usernameField.text;
        string password = passwordField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter both username and password");
            MenuSFXManager.Instance.PlayErrorSound();
            return;
        }

        LoginData loginData = new LoginData
        {
            username = username,
            password = password
        };

        RestClient.Post("http://localhost:3000/login", loginData)
            .Then(response => {
                try
                {
                    var loginResponse = JsonUtility.FromJson<LoginResponse>(response.Text);
                    UserSession.SetUserData(loginResponse.id.ToString(), loginResponse.id.ToString(), loginResponse.username);
                    MenuSFXManager.Instance.PlaySuccessSound();
                    SceneManager.LoadScene("MainMenu");
                }
                catch (Exception)
                {
                    ShowError("An error occurred during login. Please try again.");
                    MenuSFXManager.Instance.PlayErrorSound();
                }
            })
            .Catch(error => {
                ShowError("Invalid username or password");
                MenuSFXManager.Instance.PlayErrorSound();
            });
    }

    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);
        }
    }

    private void GoToCreateAccount()
    {
        MenuSFXManager.Instance.PlayButtonClick();
        SceneManager.LoadScene("SignUp");
    }

    [System.Serializable]
    private class LoginData
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    private class LoginResponse
    {
        public int id;
        public string username;
    }
}