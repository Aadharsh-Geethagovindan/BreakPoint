using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSelect : MonoBehaviour
{
    public void AutoSelectCharacters()
    {
        SoundManager.Instance.PlaySFX("click"); // play button noise
        // Clear previous selections just in case
        GameData.SelectedCharactersP1.Clear();
        GameData.SelectedCharactersP2.Clear();

        // Add predefined characters for testing
        GameData.SelectedCharactersP1.Add(new CharacterData { name = "Krakoa" });
        GameData.SelectedCharactersP1.Add(new CharacterData { name = "Avarice" });
        GameData.SelectedCharactersP1.Add(new CharacterData { name = "Jack" });

        GameData.SelectedCharactersP2.Add(new CharacterData { name = "Rover" });
        GameData.SelectedCharactersP2.Add(new CharacterData { name = "Sedra" });
        GameData.SelectedCharactersP2.Add(new CharacterData { name = "Ulmika" });

        Debug.Log("Auto-Select Complete. Loading Arena Scene...");

        // Load the Arena Scene
        SceneManager.LoadScene("Arena");
    }
}