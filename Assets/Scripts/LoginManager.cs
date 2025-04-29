using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginManager : MonoBehaviour
{
    [Header("Login UI Elements")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private TextMeshProUGUI errorMessageText;

    [Header("Audio")]
    [SerializeField] private AudioSource loginAudioSource;
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
        if (loginButton != null)
            loginButton.onClick.AddListener(AttemptLogin);

        if (createAccountButton != null)
            createAccountButton.onClick.AddListener(GoToCreateAccount);

        if (usernameField != null)
            usernameField.onValueChanged.AddListener(ValidateInput);

        if (passwordField != null)
            passwordField.onValueChanged.AddListener(ValidateInput);

        if (loginButton != null)
            loginButton.interactable = false;
    }

    private void ValidateInput(string text)
    {
        if (loginButton != null && usernameField != null && passwordField != null)
            loginButton.interactable = !string.IsNullOrEmpty(usernameField.text) &&
                                      !string.IsNullOrEmpty(passwordField.text);
    }

    private void AttemptLogin()
    {
        PlaySound(buttonClickSound);

        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);

        SetUIInteractable(false);

        StartCoroutine(LoginCoroutine());
    }

    private IEnumerator LoginCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        bool loginSuccess = (usernameField.text == "test" && passwordField.text == "password");

        if (loginSuccess)
        {
            PlaySound(successSound);

            PlayerPrefs.SetString("LoggedInUser", usernameField.text);
            PlayerPrefs.Save();

            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            PlaySound(errorSound);

            if (errorMessageText != null)
            {
                errorMessageText.text = "Invalid username or password";
                errorMessageText.gameObject.SetActive(true);
            }

            SetUIInteractable(true);
        }
    }

    private void GoToCreateAccount()
    {
        PlaySound(buttonClickSound);

        SceneManager.LoadScene("SignUp");
    }

    private void SetUIInteractable(bool interactable)
    {
        if (usernameField != null)
            usernameField.interactable = interactable;

        if (passwordField != null)
            passwordField.interactable = interactable;

        if (loginButton != null)
            loginButton.interactable = interactable &&
                                     usernameField != null &&
                                     passwordField != null &&
                                     !string.IsNullOrEmpty(usernameField.text) &&
                                     !string.IsNullOrEmpty(passwordField.text);

        if (createAccountButton != null)
            createAccountButton.interactable = interactable;
    }

    private void PlaySound(AudioClip clip)
    {
        if (loginAudioSource != null && clip != null)
        {
            loginAudioSource.clip = clip;
            loginAudioSource.Play();
        }
    }
}