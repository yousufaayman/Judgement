using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance;
    private string serverUrl = "http://localhost:3000";

    public static NetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("NetworkManager");
                _instance = obj.AddComponent<NetworkManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator RegisterUser(string username, string email, System.Action<string> callback)
    {
        string jsonData = $"{{\"username\":\"{username}\", \"email\":\"{email}\"}}";
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            callback(request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text : request.error);
        }
    }

    public IEnumerator LoginUser(string username, string password, System.Action<bool, string> callback)
    {
        if (Application.isEditor || !IsServerConnected())
        {
            yield return new WaitForSeconds(1f);    

            bool success = (username == "test" && password == "password");
            string message = success ? "Login successful" : "Invalid username or password";

            Debug.Log("[MOCK] Login attempt: " + (success ? "Success" : "Failed"));
            callback(success, message);
            yield break;
        }

        string jsonData = $"{{\"username\":\"{username}\", \"password\":\"{password}\"}}";
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;
            string message = success ? request.downloadHandler.text : request.error;

            if (success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log("Login response: " + responseText);

                    PlayerPrefs.SetString("AuthToken", "sample-token");
                    PlayerPrefs.SetString("Username", username);
                    PlayerPrefs.Save();
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing login response: " + e.Message);
                }
            }

            callback(success, message);
        }
    }

    public IEnumerator ForgotPassword(string email, System.Action<bool, string> callback)
    {
        if (Application.isEditor || !IsServerConnected())
        {
            yield return new WaitForSeconds(1f);
            callback(true, "If the email exists in our system, password reset instructions have been sent.");
            yield break;
        }

        string jsonData = $"{{\"email\":\"{email}\"}}";
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/forgot-password", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;
            string message = success ? "Password reset email sent" : request.error;

            callback(success, message);
        }
    }

    public IEnumerator SaveGameProgress(string gameData, System.Action<bool, string> callback)
    {
        if (Application.isEditor || !IsServerConnected())
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("[MOCK] Game progress saved: " + gameData);
            callback(true, "Game progress saved successfully");
            yield break;
        }

        string authToken = PlayerPrefs.GetString("AuthToken", "");
        if (string.IsNullOrEmpty(authToken))
        {
            callback(false, "Not authenticated");
            yield break;
        }

        string jsonData = $"{{\"gameData\":{gameData}}}";
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/game/save", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + authToken);

            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;
            string message = success ? "Game progress saved" : request.error;

            callback(success, message);
        }
    }

    private bool IsServerConnected()
    {
        return false;
    }

    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_Text resultText;

    public void RegisterUser()
    {
        string username = usernameInput.text;
        string email = emailInput.text;

        StartCoroutine(RegisterUser(username, email, (response) =>
        {
            resultText.text = "Register Success";
            Debug.Log("Register Success");
        }));
    }
}