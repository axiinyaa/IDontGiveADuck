using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public GameObject levelPrefab;
    public Transform parent;

    public List<LevelButton> GetLevelButtons()
    {
        foreach (Transform transform in parent)
        {
            Destroy(transform.gameObject);
        }

        List<LevelButton> buttons = new();

        int levelCount = 1;

        if (PlayerPrefs.HasKey("Level"))
        {
            levelCount = PlayerPrefs.GetInt("Level") + 1;

            if (levelCount > LevelLoader.Instance.levels.Length - 1)
            {
                levelCount = LevelLoader.Instance.levels.Length;
            }
        }

        for (int i = 0; i < levelCount; i++)
        {
            LevelButton button = Instantiate(levelPrefab, parent).GetComponent<LevelButton>();
            button.GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
            button.level = i;
            buttons.Add(button);
        }

        return buttons;
    }
}
