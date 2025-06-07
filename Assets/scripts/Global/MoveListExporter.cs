using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MoveListExporter : MonoBehaviour
{
    [System.Serializable]
    public class Move
    {
        public string name;
        public string description;
        public int cooldown;
    }

    [System.Serializable]
    public class Character
    {
        public string name;
        public int hp;
        public int speed;
        public string rarity;
        public int SigChargeReq;
        public string imageName;
        public List<Move> moves;
    }

    [System.Serializable]
    public class CharacterList
    {
        public List<Character> characters;
    }

    public string jsonFileName = "characters.json"; // Filename WITH .json extension

    void Start()
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Could not find JSON file at {fullPath}");
            return;
        }

        string jsonText = File.ReadAllText(fullPath);
        CharacterList characterData = JsonUtility.FromJson<CharacterList>(jsonText);

        string outputPath = Path.Combine(Application.dataPath, "MoveList.txt");

        using (StreamWriter writer = new StreamWriter(outputPath))
        {
            foreach (Character c in characterData.characters)
            {
                if (c.moves.Count != 4)
                {
                    Debug.LogWarning($"Character {c.name} does not have exactly 4 moves.");
                    continue;
                }

                writer.WriteLine($"{c.name} Rarity:{c.rarity}");
                writer.WriteLine($"{c.moves[0].name}: {c.moves[0].description}");
                writer.WriteLine($"{c.moves[1].name}: {c.moves[1].description} cd:{c.moves[1].cooldown}");
                writer.WriteLine($"{c.moves[2].name}: {c.moves[2].description} cd:{c.moves[2].cooldown}");
                writer.WriteLine($"{c.moves[3].name}: {c.moves[3].description} cd:{c.moves[3].cooldown}");
                writer.WriteLine(); // Empty line between characters
            }
        }

        Debug.Log($"Move list written to {outputPath}");
    }
}
