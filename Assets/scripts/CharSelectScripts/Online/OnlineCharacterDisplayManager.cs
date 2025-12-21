using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;  
using System.Linq;

public class OnlineCharacterDisplayManager : MonoBehaviour
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
    [SerializeField] private CharPreviewPanelController previewPanel;

    private const float CardSpacing = 220f;

    public bool IsPlayer1Turn() => isPlayer1Turn;
    private Coroutine messageCoroutine;
    public TextMeshProUGUI statusText;

    [SerializeField] private bool restrictionsEnforced = true;
    private readonly List<GameObject> allCards = new();        // NEW
    private readonly Dictionary<GameObject, CharacterData> cardToData = new();

    private void Start()
    {
        //if (previewPanel != null) previewPanel.Hide();
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
                break;
            case "L":
                nameText.color = new Color(255, 192, 0, 255); // Gold
                //Debug.Log($"{character.name}'s color is set to gold");
                break;
            default:
                nameText.color = new Color(255, 255, 255, 255); // Fallback
                break;
        }

        Button button = card.GetComponent<Button>();
        button.onClick.AddListener(() => OnCharacterCardClicked(card, character));

        if (characterSelectionArea != null && card.transform.IsChildOf(characterSelectionArea)) 
        {                                                                                       
            allCards.Add(card);                                                                 
            cardToData[card] = character;                                                       
        }   

        // ---------- HOVER → PREVIEW WIRING  ----------
        var c = character;
        var trigger = card.GetComponent<EventTrigger>()
                    ?? card.AddComponent<EventTrigger>();

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((BaseEventData _) =>
        {
            if (previewPanel != null) previewPanel.Show(c);
        });
        /*
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((BaseEventData _) =>
        {
            if (previewPanel != null) previewPanel.Hide();
        });*/

        trigger.triggers.Add(enter);
        //trigger.triggers.Add(exit);

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
            currentSelectionP1.GetComponent<CharacterCardUI>()?.SetCardDimmed(false);  // NEW
            currentSelectionP1.GetComponent<Button>().interactable = false; 

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
            currentSelectionP2.GetComponent<CharacterCardUI>()?.SetCardDimmed(false);  // NEW
            currentSelectionP2.GetComponent<Button>().interactable = false;  
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

            // Also clear our dimming state on grid cards (uses your CharacterCardUI)  
            var ui = card.GetComponent<CharacterCardUI>();                              
            if (ui != null) ui.SetCardDimmed(false);    
        }


        // Reset turn and UI
        isPlayer1Turn = true;
        statusText.text = "Player 1 selecting...";
    }
    public bool RestrictionsEnabled() => restrictionsEnforced; // NEW

    public void SetTemporaryStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
    public CharacterData PeekCurrentCharacter() // NEW
    {
        return isPlayer1Turn ? currentCharacterP1 : currentCharacterP2;
    }

    public void ApplyRestrictionVisualsForActivePlayer(HashSet<int> allowedRanks, List<CharacterData> confirmed) // NEW
    {
        // Map rarity string to rank
        int RankOf(string r) => r == "L" ? 4 : r == "UR" ? 3 : r == "R" ? 2 : r == "UC" ? 1 : 0;

        var confirmedNames = new HashSet<string>(confirmed.Select(c => c.name));

        //var allowedList = allowedRanks.OrderBy(x => x).Select(x => x.ToString()).ToArray();
        //Debug.Log($"[RestrictUI] allowedRanks={string.Join(",", allowedList)} (0=C,1=UC,2=R,3=UR,4=L)");

        foreach (var card in allCards)
        {
            if (!cardToData.TryGetValue(card, out var data) || data == null) continue;

            // Explicitly ensure we NEVER touch P1/P2 preview cards                  
            if ((player1Panel && card.transform.IsChildOf(player1Panel)) ||
                (player2Panel && card.transform.IsChildOf(player2Panel)))
                continue;

            // Already confirmed cards for either player should remain dim/disabled anyway
            bool alreadyPicked = confirmedNames.Contains(data.name);

            if (alreadyPicked) { card.GetComponent<CharacterCardUI>()?.SetCardDimmed(true); continue; }

            // If restrictions are off, clear dim and move on
            if (!restrictionsEnforced)
            {
                card.GetComponent<CharacterCardUI>()?.SetCardDimmed(false);
                continue;
            }

            int rank = RankOf(data.rarity);
            bool allowed = allowedRanks.Contains(rank);
            //Debug.Log($"[RestrictUI] card={data.name} rarity={data.rarity} rank={rank} allowed={allowed}");
            card.GetComponent<CharacterCardUI>()?.SetCardDimmed(!allowed);

        }
    }

    public void ApplyDraftState(DraftStateNetMessage state)
    {
        // 1) Update local "turn" concept for preview placement
        // Team 1 picking => treat as Player1Turn (for your existing click-preview logic)
        isPlayer1Turn = (state.CurrentPickerTeamId == 1);

        // 2) Status text
        int myTeam = OnlinePlayerIdentity.LocalTeamId;
        if (statusText != null)
        {
            statusText.text = (myTeam == state.CurrentPickerTeamId)
                ? "Your turn to pick..."
                : "Opponent picking...";
        }

        // 3) Disable/enable grid input depending on whether it's my turn
        bool myTurn = (myTeam == state.CurrentPickerTeamId);
        SetGridInteractable(myTurn);

        // 4) Disable already picked cards in grid for everyone
        ApplyPickedToGrid(state.Picked);

        // 5) Rebuild side panels from authoritative picks
        RebuildSidePanels(state.P1Picks, state.P2Picks);

        // 6) After rebuild, apply restrictions dimming for the ACTIVE picker
        if (restrictionsEnforced)
        {
            var confirmedForActive = (state.CurrentPickerTeamId == 1)
                ? GetCharacterDataList(state.P1Picks)
                : GetCharacterDataList(state.P2Picks);

            var allowed = RestrictionEngine.AllowedNextRarities(confirmedForActive.Select(c => c.rarity).ToList());
            ApplyRestrictionVisualsForActivePlayer(allowed, confirmedForActive);
        }
    }

    private void SetGridInteractable(bool interactable)
    {
        foreach (var card in allCards)
        {
            var btn = card.GetComponent<Button>();
            if (btn != null) btn.interactable = interactable;
        }
    }

    private void ApplyPickedToGrid(string[] pickedNames)
    {
        if (pickedNames == null) return;

        var pickedSet = new HashSet<string>(pickedNames);

        foreach (var card in allCards)
        {
            if (!cardToData.TryGetValue(card, out var data) || data == null) continue;

            bool isPicked = pickedSet.Contains(data.name);

            // Visually dim + disable if picked
            card.GetComponent<CharacterCardUI>()?.SetCardDimmed(isPicked);

            var btn = card.GetComponent<Button>();
            if (btn != null) btn.interactable = !isPicked && btn.interactable; // keep whatever SetGridInteractable decided
            if (isPicked)
            {
                var img = card.GetComponent<Image>();
                if (img != null) img.color = Color.gray;
            }
        }
    }

    // Rebuild the picked panels from names, authoritative state
    private void RebuildSidePanels(string[] p1Names, string[] p2Names)
    {
        ClearPanel(player1Panel);
        ClearPanel(player2Panel);

        confirmedSelectionsP1.Clear();
        confirmedSelectionsP2.Clear();

        // reset "current selection" previews (server state is source of truth)
        if (currentSelectionP1 != null) Destroy(currentSelectionP1);
        if (currentSelectionP2 != null) Destroy(currentSelectionP2);
        currentSelectionP1 = null;
        currentSelectionP2 = null;
        currentCharacterP1 = null;
        currentCharacterP2 = null;

        if (p1Names != null)
            BuildConfirmedPanel(player1Panel, confirmedSelectionsP1, p1Names);

        if (p2Names != null)
            BuildConfirmedPanel(player2Panel, confirmedSelectionsP2, p2Names);
    }

    private void ClearPanel(Transform panel)
    {
        if (panel == null) return;
        foreach (Transform child in panel) Destroy(child.gameObject);
    }

    private void BuildConfirmedPanel(Transform panel, List<GameObject> confirmedList, string[] pickedNames)
    {
        foreach (var name in pickedNames)
        {
            var data = FindCharacterByName(name);
            if (data == null) continue;

            var card = Instantiate(characterCardPrefab, panel);
            card.GetComponent<RectTransform>().localScale = Vector3.one;
            card.GetComponent<RectTransform>().anchoredPosition = new Vector2(confirmedList.Count * CardSpacing, 0);

            SetupCard(card, data);
            card.GetComponent<CharacterCardUI>()?.SetCardDimmed(false);

            var btn = card.GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            confirmedList.Add(card);
        }
    }

    // Build list<CharacterData> from names (for restrictions)
    private List<CharacterData> GetCharacterDataList(string[] names)
    {
        var list = new List<CharacterData>();
        if (names == null) return list;

        foreach (var n in names)
        {
            var d = FindCharacterByName(n);
            if (d != null) list.Add(d);
        }
        return list;
    }

    public List<CharacterData> GetCharacterDataList_ForOnline(string[] names)
    {
        var list = new List<CharacterData>();
        if (names == null) return list;

        foreach (var n in names)
        {
            var d = FindCharacterByName(n);
            if (d != null) list.Add(d);
        }
        return list;
    }

    private CharacterData FindCharacterByName(string name)
    {
        if (characterLoader?.characterDataArray?.characters == null) return null;
        return characterLoader.characterDataArray.characters.FirstOrDefault(c => c.name == name);
    }
}
