using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class CharacterJsonPatcher : MonoBehaviour
{
    public string jsonFileName = "Characters.json"; // Must be in StreamingAssets

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"Could not find JSON file at: {path}");
            return;
        }

        string json = File.ReadAllText(path);

        JObject root = JObject.Parse(json);
        JArray characters = (JArray)root["characters"];
        bool updated = false;

        foreach (JObject character in characters)
        {
            if (character["affiliation"] == null)
            {
                character["affiliation"] = "";
                updated = true;
            }
            if (character["species"] == null)
            {
                character["species"] = "";
                updated = true;
            }
            if (character["lore"] == null)
            {
                character["lore"] = "";
                updated = true;
            }
        }

        if (updated)
        {
            string updatedJson = root.ToString(); // Pretty print
            File.WriteAllText(path, updatedJson);
            Debug.Log("Characters.json successfully patched with missing fields.");
        }
        else
        {
            Debug.Log("No patching needed. All fields present.");
        }
    }
}
