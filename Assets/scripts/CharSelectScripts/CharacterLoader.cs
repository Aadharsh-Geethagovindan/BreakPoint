using UnityEngine;
using System.IO;

public class CharacterLoader : MonoBehaviour
{
    public CharacterDataArray characterDataArray;
private void Awake() // Changed from Start() to Awake() to load before other scripts need it
    {
        LoadCharacterData();
    }

    private void LoadCharacterData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Characters.json");
        
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            characterDataArray = JsonUtility.FromJson<CharacterDataArray>(json);
            //Debug.Log("Character data loaded successfully.");
        }
        else
        {
            Debug.LogError("Character data file not found at: " + filePath);
        }
    }
}