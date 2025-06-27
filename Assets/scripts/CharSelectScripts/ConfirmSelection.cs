using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ConfirmSelection : MonoBehaviour
{
    public Button confirmButton;
    public CharacterDisplayManager characterDisplayManager;

    private List<CharacterData> selectedCharactersP1 = new List<CharacterData>();
    private List<CharacterData> selectedCharactersP2 = new List<CharacterData>();

    private const int MaxSelectionsPerPlayer = 3;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    private void OnConfirmClicked()
    {
        SoundManager.Instance.PlaySFX("click"); // play the button sound
        CharacterData confirmedCharacter = characterDisplayManager.ConfirmSelection();

        if (confirmedCharacter == null)
        {
            return;
        }

        if (confirmedCharacter != null)
        {
            if (characterDisplayManager.IsPlayer1Turn())
            {
                if (selectedCharactersP1.Count < MaxSelectionsPerPlayer)
                {
                    selectedCharactersP1.Add(confirmedCharacter);
                    //Debug.Log("Player 1 selected: " + confirmedCharacter.name);
                }
            }
            else
            {
                if (selectedCharactersP2.Count < MaxSelectionsPerPlayer)
                {
                    selectedCharactersP2.Add(confirmedCharacter);
                    //Debug.Log("Player 2 selected: " + confirmedCharacter.name);
                }
            }
        }

        characterDisplayManager.SwitchTurn();
        characterDisplayManager.statusText.text = characterDisplayManager.IsPlayer1Turn() ? "Player 1 selecting..." : "Player 2 selecting...";

        if (selectedCharactersP1.Count >= MaxSelectionsPerPlayer && selectedCharactersP2.Count >= MaxSelectionsPerPlayer)
        {
            DebugSelections();
            SaveSelectedCharacters();
            SceneManager.LoadScene("Arena");
        }
    }

    private void DebugSelections()
    {
        Debug.Log("Player 1's Final Selections:");
        foreach (var character in selectedCharactersP1)
        {
            Debug.Log(character.name);
        }

        Debug.Log("Player 2's Final Selections:");
        foreach (var character in selectedCharactersP2)
        {
            Debug.Log(character.name);
        }
    }

    private void SaveSelectedCharacters()
    {
        GameData.SelectedCharactersP1 = selectedCharactersP1;
        GameData.SelectedCharactersP2 = selectedCharactersP2;
    }

    public void ClearSelections()
    {
        selectedCharactersP1.Clear();
        selectedCharactersP2.Clear();
        GameData.SelectedCharactersP1.Clear();
        GameData.SelectedCharactersP2.Clear();
    }

}
