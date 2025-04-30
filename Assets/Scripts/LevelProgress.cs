using System;

[System.Serializable]
public class LevelData
{
    public bool unlocked;
    public int high_score;
    public string last_played;
}

[System.Serializable]
public class UserProgress
{
    public int id;
    public string username;
    public string created_at;
    public LevelProgress progress;
}

[System.Serializable]
public class LevelProgress
{
    public LevelData LEVEL1;
    public LevelData LEVEL2;
    public LevelData LEVEL3;
} 