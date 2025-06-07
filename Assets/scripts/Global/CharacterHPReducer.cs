using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CharacterHPReducer : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string name;
        public int hp;
        public int speed;
        public string rarity;
        public int SigChargeReq;
        public string imageName;
        public List<MoveData> moves;
    }

    [System.Serializable]
    public class MoveData
    {
        public string name;
        public string description;
        public int cooldown;
    }

    [System.Serializable]
    public class CharacterDataArray
    {
        public List<CharacterData> characters;
    }

    public string jsonFileName = "Characters.json";
    public bool writeLogFile = true;

    void Start()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        string logPath = Path.Combine(Application.dataPath, "ReducedHP_Results.txt");

        if (!File.Exists(jsonPath))
        {
            Debug.LogError("Characters.json file not found.");
            return;
        }

        string json = File.ReadAllText(jsonPath);
        CharacterDataArray dataArray = JsonUtility.FromJson<CharacterDataArray>(json);

        List<string> logLines = new List<string>();

        foreach (var character in dataArray.characters)
        {
            int originalHP = character.hp;
            character.hp = Mathf.RoundToInt(character.hp * 0.8f);
            logLines.Add($"{character.name}: {originalHP} → {character.hp}");
        }

        // Write updated JSON back
        string updatedJson = JsonUtility.ToJson(dataArray, true);
        File.WriteAllText(jsonPath, updatedJson);
        Debug.Log("HP values updated in Characters.json.");

        if (writeLogFile)
        {
            File.WriteAllLines(logPath, logLines);
            Debug.Log($"Change log written to {logPath}");
        }
    }
}
