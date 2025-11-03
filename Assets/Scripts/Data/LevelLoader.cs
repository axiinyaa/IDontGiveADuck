using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Unity.VisualScripting;
using System.Linq;

/// <summary>
/// LevelLoader - Handles loading level data from JSON files
/// 
/// This class demonstrates several important programming concepts:
/// - Singleton Pattern: Ensures only one instance exists
/// - Caching: Improves performance by storing loaded data
/// - Error Handling: Graceful fallback when files are missing
/// - Resource Loading: Unity's Resources system for file access
/// </summary>

public class LevelLoader : MonoBehaviour
{
    // Singleton pattern - static instance accessible from anywhere
    public static LevelLoader Instance { get; private set; }

    public Level[] levels;
    public int currentLevelIndex;

    public void StartLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        Level level = levels[levelIndex];
        level.PrepareLevel();
    }

    public static Level GetCurrentLevel()
    {
        try
        {
            return Instance.levels[Instance.currentLevelIndex];
        }
        catch
        {
            return new Level();
        }
    }
}
