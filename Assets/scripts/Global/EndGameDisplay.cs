using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EndGameDisplay : MonoBehaviour
{
    public TextMeshProUGUI victoryText;
    public Transform cardHolder; // Parent object to hold cards
    public GameObject characterCardPrefab;

    void Start()
    {
        int winningPlayer = PlayerPrefs.GetInt("Winner", 1);
        victoryText.text = $"Victory for Player {winningPlayer}!";

        List<CharacterData> winningTeam = GameData.SelectedCharactersP1;

        if (winningPlayer == 2)
            winningTeam = GameData.SelectedCharactersP2;

        foreach (var character in winningTeam)
        {
            GameObject card = Instantiate(characterCardPrefab, cardHolder);
            TextMeshProUGUI nameText = card.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            Image portraitImage = card.transform.Find("PortraitImage").GetComponent<Image>();

            nameText.text = character.name;
            portraitImage.sprite = Resources.Load<Sprite>($"Images/{character.imageName}");
        }
    }
}
