using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CharacterStatEditor : MonoBehaviour
{
    [System.Serializable]
    public class Move
    {
        public string name;
        public string description;
        public int cooldown;
    }

    [System.Serializable]
    public class CharacterData
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
    public class CharacterDataArray
    {
        public List<CharacterData> characters;
    }

    public string jsonFileName = "characters.json"; // must be inside StreamingAssets

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"Could not find JSON file at: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        CharacterDataArray data = JsonUtility.FromJson<CharacterDataArray>(json);

        foreach (var character in data.characters)
        {
            character.hp = Mathf.RoundToInt(character.hp * 1.5f);
            Debug.Log($"Updated {character.name}'s HP to {character.hp}");
        }

        string updatedJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, updatedJson);

        Debug.Log("All character HP values modified and saved.");
    }
}
