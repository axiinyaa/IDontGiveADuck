using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "ScriptableObject/New Level")]
public class Level : ScriptableObject
{
    public enum SpawnOrder { Normal, Random }

    [Header("Level Settings")]
    public string LevelName = "Level";
    public AudioClip Music;

    [Header("Prefabs to Spawn")]
    public GameObject[] CharactersToSpawn;

    [Header("Spawn Settings")]
    public float SecondsBeforeSpawn = 3;
    public SpawnOrder Order = SpawnOrder.Normal;

    private int selectionIndex = 0;
    private List<GameObject> charactersAllowedToSpawn = new();

    public int GeeseLeft => CalculateGeeseCount();
    public int GeeseToSpawn => CalculateTotalGeeseCount();

    public int CalculateGeeseCount()
    {
        int count = 0;

        foreach (GameObject character in charactersAllowedToSpawn)
        {
            if (character == null) continue;

            if (character.TryGetComponent(out Goose goose))
            {
                count += 1;
            }
        }

        return count;
    }

    public int CalculateTotalGeeseCount()
    {
        int count = 0;

        foreach (GameObject character in CharactersToSpawn)
        {
            if (character == null) continue;

            if (character.TryGetComponent(out Goose goose))
            {
                count += 1;
            }
        }

        return count;
    }

    public List<Goose> GetGeese()
    {
        List<Goose> geese = new();

        foreach (GameObject character in CharactersToSpawn)
        {
            if (character == null) continue;

            if (character.TryGetComponent(out Goose goose))
            {
                geese.Add(goose);
            }
        }

        return geese;
    }

    public List<Duck> GetDucks()
    {
        List<Duck> ducks = new();

        foreach (GameObject character in CharactersToSpawn)
        {
            if (character == null) continue;

            if (character.TryGetComponent(out Duck duck))
            {
                ducks.Add(duck);
            }
        }

        return ducks;
    }

    public void PrepareLevel()
    {
        charactersAllowedToSpawn.Clear();

        charactersAllowedToSpawn = CharactersToSpawn.ToList();
    }

    public GameObject PickNext()
    {
        if (Order == SpawnOrder.Random) selectionIndex = Random.Range(0, charactersAllowedToSpawn.Count);

        if (selectionIndex >= charactersAllowedToSpawn.Count) return null;

        GameObject selected = charactersAllowedToSpawn[selectionIndex];
        charactersAllowedToSpawn.RemoveAt(selectionIndex);

        return selected;
    }
}