using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CreateAccountManager : MonoBehaviour
{
    [Header("Create Account UI Elements")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_InputField confirmPasswordField;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI errorMessageText;

    [Header("Account Requirements")]
    [SerializeField] private int minimumUsernameLength = 6;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip successSound;

    private void Awake()
    {
        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);
    }

    private void Start()
    {
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
        PlaySound(buttonClickSound);

        if (string.IsNullOrEmpty(usernameField.text))
        {
            ShowError("Username is required");
            return;
        }
        else if (usernameField.text.Length < minimumUsernameLength)
        {
            ShowError($"Username must be at least {minimumUsernameLength} characters");
            return;
        }

        if (!Regex.IsMatch(passwordField.text, "[A-Z]") || !Regex.IsMatch(passwordField.text, "[0-9]"))
        {
            ShowError("Password must contain at least one capital letter and one digit");
            return;
        }

        if (passwordField.text != confirmPasswordField.text)
        {
            ShowError("Passwords do not match");
            return;
        }

        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);

        SetUIInteractable(false);

        StartCoroutine(CreateAccountCoroutine());
    }

    private IEnumerator CreateAccountCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        bool createSuccess = usernameField.text.ToLower() != "admin";

        if (createSuccess)
        {
            PlaySound(successSound);
            Debug.Log($"Account created: {usernameField.text}");
            SceneManager.LoadScene("Login");
        }
        else
        {
            PlaySound(errorSound);
            ShowError("Username already exists");
            SetUIInteractable(true);
        }
    }

    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);
        }

        PlaySound(errorSound);
    }

    private void GoBackToLogin()
    {
        PlaySound(buttonClickSound);
        SceneManager.LoadScene("Login");
    }

    private void SetUIInteractable(bool interactable)
    {
        if (usernameField != null)
            usernameField.interactable = interactable;

        if (passwordField != null)
            passwordField.interactable = interactable;

        if (confirmPasswordField != null)
            confirmPasswordField.interactable = interactable;

        if (createAccountButton != null)
            createAccountButton.interactable = interactable && ValidateAllFields();

        if (backButton != null)
            backButton.interactable = interactable;
    }

    private bool ValidateAllFields()
    {
        if (usernameField == null || passwordField == null || confirmPasswordField == null)
            return false;

        bool validUsername = !string.IsNullOrEmpty(usernameField.text) &&
                            usernameField.text.Length >= minimumUsernameLength;

        bool hasCapital = Regex.IsMatch(passwordField.text, "[A-Z]");
        bool hasDigit = Regex.IsMatch(passwordField.text, "[0-9]");
        bool validPassword = !string.IsNullOrEmpty(passwordField.text) && hasCapital && hasDigit;

        bool passwordsMatch = !string.IsNullOrEmpty(confirmPasswordField.text) &&
                             passwordField.text == confirmPasswordField.text;

        return validUsername && validPassword && passwordsMatch;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}