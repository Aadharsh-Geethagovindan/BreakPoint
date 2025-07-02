using UnityEngine;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class ArenaCharacterLoader : MonoBehaviour
{
    public GameObject characterCardPrefab; 
    public Transform characterHolder; 

    private CharacterDataArray characterDataArray;
    private List<CharacterData> sortedCharacters;
    public GameObject hpBarPrefab;  // Assign in inspector

    

    private const float Radius = 280f;
    [SerializeField] private float yOffset = 50f; // y offset for card positioning in battleground
    [SerializeField] private float xOffset = 0f; // x offset for card positioning in battleground

    private void Awake()
    {
        LoadCharacterData();
        FetchSelectedCharacters();
        //ArrangeCharactersInCircle();

    }

    private void LoadCharacterData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Characters.json");

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            characterDataArray = JsonUtility.FromJson<CharacterDataArray>(jsonData);
            //Debug.Log("Character Data Loaded Successfully.");
        }
        else
        {
            Debug.LogError("Cannot find Characters.json file at " + filePath);
        }
    }

    private void FetchSelectedCharacters()
    {
        if (characterDataArray == null || characterDataArray.characters == null)
        {
            Debug.LogError("Character Data not loaded properly.");
            return;
        }

        sortedCharacters = new List<CharacterData>();

        // Add selected characters from both players
        foreach (var character in GameData.SelectedCharactersP1)
        {
            CharacterData match = FindCharacterByName(character.name);
            if (match != null) sortedCharacters.Add(match);
        }
        
        foreach (var character in GameData.SelectedCharactersP2)
        {
            CharacterData match = FindCharacterByName(character.name);
            if (match != null) sortedCharacters.Add(match);
        }

        // Sort by speed in descending order
        sortedCharacters.Sort((x, y) => y.speed.CompareTo(x.speed));
    }

    private CharacterData FindCharacterByName(string characterName)
    {
        foreach (var character in characterDataArray.characters)
        {
            if (character.name == characterName)
                return character;
        }
        Debug.LogWarning($"Character '{characterName}' not found in Characters.json");
        return null;
    }
/*
    private void ArrangeCharactersInCircle()
    {
        if (sortedCharacters == null || sortedCharacters.Count < 6)
        {
            Debug.LogError("Not enough characters to arrange. Need 6 characters.");
            return;
        }

        float angleStep = 360f / 6f;

        for (int i = 0; i < 6; i++)
        {
            CharacterData character = sortedCharacters[i];
            float angle = angleStep * i;
            Vector2 position = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * Radius;

            GameObject newCard = Instantiate(characterCardPrefab, characterHolder);
            RectTransform rectTransform = newCard.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.localScale = Vector3.one;

            // Set up card visuals
            TextMeshProUGUI nameText = newCard.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = character.name;

            // 🔹 Set up portrait image
            Image portrait = newCard.transform.Find("PortraitImage").GetComponent<Image>();
            Sprite characterSprite = Resources.Load<Sprite>($"Images/{character.imageName}");
            
            if (characterSprite != null)
                portrait.sprite = characterSprite;
            else
                Debug.LogWarning($"Image not found for character: {character.imageName}");

            // Optionally display other stats, like HP and Speed, if needed
        }
    }*/

    public void CreateCharacterCards(List<GameCharacter> characters)
    {
        ActiveCharPanel panel = Object.FindFirstObjectByType<ActiveCharPanel>();
        if (panel == null)
        {
            Debug.LogError("ActiveCharPanel not found.");
            return;
        }

        float angleStep = 360f / characters.Count;
        for (int i = 0; i < characters.Count; i++)
        {
            GameCharacter character = characters[i];
            float angle = -angleStep * i;
            Vector2 basePosition = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * Radius;
            Vector2 position = basePosition + new Vector2(xOffset, yOffset); // shift up

            GameObject cardObj = GameObject.Instantiate(characterCardPrefab, characterHolder);
           
           
            // Determine Border color for card based on player it belongs to
            bool isPlayer2 = GameData.SelectedCharactersP2.Any(c => c.name == character.Name);

            // Change border color based on player
            Image borderImage = cardObj.transform.Find("BorderImage").GetComponent<Image>();
            if (borderImage != null)
            {
                if (isPlayer2)
                    borderImage.color = new Color32(132, 0, 243, 255); // Purple for Player 2
                else
                    borderImage.color = new Color32(0, 0, 255, 255);  // Gold for Player 1
            }

            RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.localScale = Vector3.one;

            // Instantiate the HP bar
            GameObject hpBarObj = Instantiate(hpBarPrefab, characterHolder);
            RectTransform hpBarRect = hpBarObj.GetComponent<RectTransform>();

            // Position it slightly above the card (e.g., +60 on Y axis)
            hpBarRect.anchoredPosition = rectTransform.anchoredPosition + new Vector2(0, 92f);
            hpBarRect.localScale = Vector3.one;

            // Hook it up to the character
            HPBarController barController = hpBarObj.GetComponent<HPBarController>();
            if (barController != null)
            {
                barController.Initialize(character); 
            }


            // Set name and image (optional)
            TextMeshProUGUI nameText = cardObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = character.Name;

            Image portraitImage = cardObj.transform.Find("PortraitImage").GetComponent<Image>();
            portraitImage.sprite = Resources.Load<Sprite>($"Images/{character.ImageName}");
            // Hide the Stats text
            Transform statsBlock = cardObj.transform.Find("Stats");
            if (statsBlock != null)
                statsBlock.gameObject.SetActive(false);
            // Register with targeting system
            CharacterCardUI cardUI = cardObj.GetComponent<CharacterCardUI>();
            cardUI.SetCharacter(character);
            panel.RegisterCard(character, cardUI);
        }
    }

   public List<CharacterData> GetSortedCharacters()
    {
        if (sortedCharacters == null || sortedCharacters.Count == 0)
        {
            Debug.LogError("Sorted character list is either null or empty. Make sure characters are being properly loaded.");
        }
        return sortedCharacters;
    }

    public CharacterDataArray GetCharacterDataArray()
    {
        return characterDataArray;
    }
}