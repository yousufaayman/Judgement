using System;

[Serializable]
public class RegisterRequest
{
    public string username;
    public string password;

    public RegisterRequest(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}