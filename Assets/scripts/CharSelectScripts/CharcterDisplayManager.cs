using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
public class CharacterDisplayManager : MonoBehaviour
{
    public CharacterLoader characterLoader;
    public GameObject characterCardPrefab;
    public Transform characterSelectionArea;
    public Transform player1Panel;
    public Transform player2Panel;
    public Sprite defaultPortrait;

    private List<GameObject> confirmedSelectionsP1 = new List<GameObject>();
    private List<GameObject> confirmedSelectionsP2 = new List<GameObject>();
    private GameObject currentSelectionP1;
    private GameObject currentSelectionP2;
    private GameObject pendingCardP1;
    private GameObject pendingCardP2;

    private CharacterData currentCharacterP1;
    private CharacterData currentCharacterP2;
    private bool isPlayer1Turn = true;

    private const float CardSpacing = 220f;

    public bool IsPlayer1Turn() => isPlayer1Turn;
    private Coroutine messageCoroutine;
    public TextMeshProUGUI statusText;



    private void Start()
    {
        DisplayAllCharacterCards();
        SoundManager.Instance.PlayMusic("selection");
    }

    private void DisplayAllCharacterCards()
    {
        if (characterLoader.characterDataArray == null) return;

        foreach (CharacterData character in characterLoader.characterDataArray.characters)
        {
            GameObject card = Instantiate(characterCardPrefab, characterSelectionArea);
            SetupCard(card, character);
        }
    }

    private void SetupCard(GameObject card, CharacterData character)
    {
        TextMeshProUGUI nameText = card.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        nameText.text = $"({character.rarity}) {character.name}";

        Image portrait = card.transform.Find("PortraitImage").GetComponent<Image>();
        Sprite loadedSprite = Resources.Load<Sprite>("Images/" + character.imageName);

        if (loadedSprite != null)
            portrait.sprite = loadedSprite;
        else
            portrait.sprite = defaultPortrait;

        TextMeshProUGUI statsText = card.transform.Find("Stats").GetComponent<TextMeshProUGUI>();
        statsText.text = $"HP: {character.hp}  SPD: {character.speed}";

        //Image rarityGlow = card.transform.Find("rarityImage").GetComponent<Image>();
        //rarityGlow.gameObject.SetActive(true);

        // Tint NameText based on rarity
        switch (character.rarity)
        {
            case "C":
                nameText.color = new Color(255, 255, 255, 255); // White with light alpha
                break;
            case "UC":
                nameText.color = new Color(0.2f, 0.6f, 1f, 1f); // White with light alpha
                break;
            case "R":
                nameText.color = new Color(0.2f, 0.6f, 0f, 1f); // Green-ish
                break;
            case "UR":
                nameText.color = new Color(0.6f, 0f, 1f, 1f); // Purple
                 Debug.Log($"{character.name}'s color is set to purple");
                break;
            case "L":
                nameText.color = new Color(255, 192, 0, 255); // Gold
                Debug.Log($"{character.name}'s color is set to gold");
                break;
            default:
                nameText.color = new Color(255, 255, 255, 255); // Fallback
                break;
        }

        Button button = card.GetComponent<Button>();
        button.onClick.AddListener(() => OnCharacterCardClicked(card, character));
    }

    private void OnCharacterCardClicked(GameObject originalCard, CharacterData character)
    {
        //originalCard.GetComponent<Button>().interactable = false;
        //originalCard.GetComponent<Image>().color = Color.gray;

        if (isPlayer1Turn)
        {
            // Replace current selection if it exists and hasn't been confirmed
            if (currentSelectionP1 != null)
            {
                Destroy(currentSelectionP1);
            }

            // Create the new card and store it as the current selection
            currentSelectionP1 = Instantiate(characterCardPrefab, player1Panel);
            currentSelectionP1.GetComponent<RectTransform>().localScale = Vector3.one;
            currentSelectionP1.GetComponent<RectTransform>().anchoredPosition = new Vector2(confirmedSelectionsP1.Count * CardSpacing, 0);
            SetupCard(currentSelectionP1, character);

            currentCharacterP1 = character;
            pendingCardP1 = originalCard; // store clicked card for disabling later
        }
        else
        {
            if (currentSelectionP2 != null)
            {
                Destroy(currentSelectionP2);
            }

            currentSelectionP2 = Instantiate(characterCardPrefab, player2Panel);
            currentSelectionP2.GetComponent<RectTransform>().localScale = Vector3.one;
            currentSelectionP2.GetComponent<RectTransform>().anchoredPosition = new Vector2(confirmedSelectionsP2.Count * CardSpacing, 0);
            SetupCard(currentSelectionP2, character);

            currentCharacterP2 = character;
            pendingCardP2 = originalCard; // store clicked card for disabling later
        }
    }

    public CharacterData ConfirmSelection()
    {
        CharacterData confirmedCharacter = null;

        if (isPlayer1Turn)
        {
            if (currentCharacterP1 == null)
            {
                Debug.LogWarning("Player 1 tried to confirm without selecting a character.");
                SetTemporaryStatus("Player 1 tried to confirm without selecting a character.");
                return null;
            }

            confirmedCharacter = currentCharacterP1;
            confirmedSelectionsP1.Add(currentSelectionP1);
            currentSelectionP1 = null;
            currentCharacterP1 = null;

            if (pendingCardP1 != null)
            {
                pendingCardP1.GetComponent<Button>().interactable = false;
                pendingCardP1.GetComponent<Image>().color = Color.gray;
                pendingCardP1 = null;
            }
        }
        else
        {
            if (currentCharacterP2 == null)
            {
                Debug.LogWarning("Player 2 tried to confirm without selecting a character.");
                SetTemporaryStatus("Player 2 tried to confirm without selecting a character.");
                return null;
            }

            confirmedCharacter = currentCharacterP2;
            confirmedSelectionsP2.Add(currentSelectionP2);
            currentSelectionP2 = null;
            currentCharacterP2 = null;

            if (pendingCardP2 != null)
            {
                pendingCardP2.GetComponent<Button>().interactable = false;
                pendingCardP2.GetComponent<Image>().color = Color.gray;
                pendingCardP2 = null;
            }
        }

        return confirmedCharacter;
    }


    public void SwitchTurn()
    {
        isPlayer1Turn = !isPlayer1Turn;
    }

    public void SetTemporaryStatus(string message, float duration = 3f)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(TemporaryMessageRoutine(message, duration));
    }

    private IEnumerator TemporaryMessageRoutine(string message, float duration)
    {
        string originalMessage = statusText.text;
        statusText.text = message;
        yield return new WaitForSeconds(duration);
        statusText.text = originalMessage;
    }
    
    public void ClearSelections()
{
    SoundManager.Instance.PlaySFX("click");

    // Destroy confirmed visual cards
    foreach (var card in confirmedSelectionsP1) Destroy(card);
    foreach (var card in confirmedSelectionsP2) Destroy(card);
    confirmedSelectionsP1.Clear();
    confirmedSelectionsP2.Clear();

    // Destroy current selections (unconfirmed)
    if (currentSelectionP1 != null) Destroy(currentSelectionP1);
    if (currentSelectionP2 != null) Destroy(currentSelectionP2);
    currentSelectionP1 = null;
    currentSelectionP2 = null;
    currentCharacterP1 = null;
    currentCharacterP2 = null;

    foreach (Transform card in characterSelectionArea.transform)
    {
        Button btn = card.GetComponent<Button>();
        if (btn != null) btn.interactable = true;

        Image img = card.GetComponent<Image>();
        if (img != null) img.color = Color.white;
    }


    // Reset turn and UI
    isPlayer1Turn = true;
    statusText.text = "Player 1 selecting...";
}
}
