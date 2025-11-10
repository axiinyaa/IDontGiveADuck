using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "ScriptableObject/New Level")]
public class Level : ScriptableObject
{
    public enum SpawnOrder { Normal, Random }

    [Header("Level Settings")]
    public string LevelName = "Level";
    public AudioClip Music;

    [Header("Prefabs to Spawn")]
    public GameObject[] DucksToSpawn;
    public GameObject[] GeeseToSpawn;

    [Header("Spawn Settings")]
    public float SecondsBeforeSpawn = 3;
    public SpawnOrder Order = SpawnOrder.Normal;

    private int selectionIndex = 0;
    private List<GameObject> ducksAllowedToSpawn = new();
    private List<GameObject> geeseAllowedToSpawn = new();

    public int GeeseLeft => geeseAllowedToSpawn.Count;
    public int DucksLeft => ducksAllowedToSpawn.Count;

    public void PrepareLevel()
    {
        ducksAllowedToSpawn.Clear();
        geeseAllowedToSpawn.Clear();

        ducksAllowedToSpawn = DucksToSpawn.ToList();
        geeseAllowedToSpawn = GeeseToSpawn.ToList();
    }

    public GameObject PickNextDuck()
    {
        if (Order == SpawnOrder.Random) selectionIndex = Random.Range(0, ducksAllowedToSpawn.Count);

        GameObject selected = ducksAllowedToSpawn[selectionIndex];
        ducksAllowedToSpawn.RemoveAt(selectionIndex);

        return selected;
    }

    public GameObject PickNextGoose()
    {
        if (Order == SpawnOrder.Random) selectionIndex = Random.Range(0, geeseAllowedToSpawn.Count);

        GameObject selected = geeseAllowedToSpawn[selectionIndex];
        geeseAllowedToSpawn.RemoveAt(selectionIndex);

        return selected;
    }
}