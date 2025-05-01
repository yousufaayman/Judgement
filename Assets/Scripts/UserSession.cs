using System;
using UnityEngine;

public static class UserSession
{
    public const string API_URL = "https://judgementbackend-production.up.railway.app";

    public static string UserId { get; private set; }
    public static string Token { get; private set; }
    public static string Username { get; private set; }

    public static void SetUserData(string userId, string token, string username)
    {
        UserId = userId;
        Token = token;
        Username = username;
    }

    public static bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(Token);
    }

    public static void ClearUserData()
    {
        UserId = null;
        Token = null;
        Username = null;
    }

    public static void SetUser(string userId, string username)
    {
        UserId = userId;
        Username = username;
    }

    public static void Clear()
    {
        UserId = null;
        Username = null;
    }
} 