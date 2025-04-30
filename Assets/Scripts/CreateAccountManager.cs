using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Proyecto26;
using System;

public class CreateAccountManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_InputField confirmPasswordField;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI errorMessageText;

    [Header("Account Requirements")]
    [SerializeField] private int minimumUsernameLength = 6;

    private void Start()
    {
        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);

        if (createAccountButton != null)
            createAccountButton.onClick.AddListener(AttemptCreateAccount);

        if (backButton != null)
            backButton.onClick.AddListener(GoBackToLogin);

        if (usernameField != null)
            usernameField.onValueChanged.AddListener(ValidateInput);

        if (passwordField != null)
            passwordField.onValueChanged.AddListener(ValidateInput);

        if (confirmPasswordField != null)
            confirmPasswordField.onValueChanged.AddListener(ValidateInput);

        if (createAccountButton != null)
            createAccountButton.interactable = false;
    }

    private void ValidateInput(string text)
    {
        if (usernameField == null || passwordField == null || confirmPasswordField == null)
            return;

        string errorMessage = null;

        bool validUsername = false;
        if (string.IsNullOrEmpty(usernameField.text))
        {
            validUsername = false;
        }
        else if (usernameField.text.Length < minimumUsernameLength)
        {
            errorMessage = $"Username must be at least {minimumUsernameLength} characters";
            validUsername = false;
        }
        else
        {
            validUsername = true;
        }

        if (errorMessage != null)
        {
            ShowError(errorMessage);
            if (createAccountButton != null)
                createAccountButton.interactable = false;
            return;
        }

        bool validPassword = false;
        if (!string.IsNullOrEmpty(passwordField.text))
        {
            bool hasCapital = Regex.IsMatch(passwordField.text, "[A-Z]");
            bool hasDigit = Regex.IsMatch(passwordField.text, "[0-9]");

            if (!hasCapital || !hasDigit)
            {
                errorMessage = "Password must contain at least one capital letter and one digit";
                validPassword = false;
            }
            else
            {
                validPassword = true;
            }
        }

        bool passwordsMatch = false;
        if (!string.IsNullOrEmpty(passwordField.text) && !string.IsNullOrEmpty(confirmPasswordField.text))
        {
            passwordsMatch = passwordField.text == confirmPasswordField.text;
            if (!passwordsMatch && errorMessage == null)
            {
                errorMessage = "Passwords do not match";
            }
        }

        if (errorMessage != null)
        {
            ShowError(errorMessage);
        }
        else if (errorMessageText != null && errorMessageText.gameObject.activeSelf)
        {
            errorMessageText.gameObject.SetActive(false);
        }

        bool isValid = validUsername && validPassword &&
                      (string.IsNullOrEmpty(confirmPasswordField.text) || passwordsMatch);

        if (createAccountButton != null)
            createAccountButton.interactable = isValid && passwordsMatch;
    }

    private void AttemptCreateAccount()
    {
        MenuSFXManager.Instance.PlayButtonClick();

        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);

        string username = usernameField.text;
        string password = passwordField.text;
        string confirmPassword = confirmPasswordField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowError("Please fill in all fields");
            MenuSFXManager.Instance.PlayErrorSound();
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Passwords do not match");
            MenuSFXManager.Instance.PlayErrorSound();
            return;
        }

        CreateAccountData accountData = new CreateAccountData
        {
            username = username,
            password = password
        };

        RestClient.Post("http://localhost:3000/register", accountData)
            .Then(response => {
                try
                {
                    var createResponse = JsonUtility.FromJson<CreateAccountResponse>(response.Text);
                    MenuSFXManager.Instance.PlaySuccessSound();
                    SceneManager.LoadScene("Login");
                }
                catch (Exception)
                {
                    ShowError("An error occurred during account creation. Please try again.");
                    MenuSFXManager.Instance.PlayErrorSound();
                }
            })
            .Catch(error => {
                ShowError("Username already exists");
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

    private void GoBackToLogin()
    {
        MenuSFXManager.Instance.PlayButtonClick();
        SceneManager.LoadScene("Login");
    }

    [System.Serializable]
    private class CreateAccountData
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    private class CreateAccountResponse
    {
        public int id;
        public string username;
    }
}