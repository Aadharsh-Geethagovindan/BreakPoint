using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using Mirror;
public class OnlineConfirmSelection : MonoBehaviour
{
    public Button confirmButton;
    public OnlineCharacterDisplayManager characterDisplayManager;

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

        // --- PRE-CHECK BEFORE CONFIRM --- 
        if (characterDisplayManager.RestrictionsEnabled()) 
        {
            var candidat = characterDisplayManager.PeekCurrentCharacter(); 
            if (candidat == null)                                           
            {                                                                
                characterDisplayManager.SetTemporaryStatus("Select a character first."); 
                return;                                                       
            }                                                                

            // Build current confirmed picks for the active player             
            List<CharacterData> confirmedList = characterDisplayManager.IsPlayer1Turn() 
                ? selectedCharactersP1
                : selectedCharactersP2;

            var rarities = confirmedList.Select(c => c.rarity).ToList();     
            rarities.Add(candidat.rarity);                                  

            if (!RestrictionEngine.IsValidTeam(rarities))                    
            {                                                                
                characterDisplayManager.SetTemporaryStatus("Selection violates restrictions!"); 

                // Compute and apply allowed set (based on current confirmed only) 
                var allowed = RestrictionEngine.AllowedNextRarities(         
                    confirmedList.Select(c => c.rarity).ToList());           
                characterDisplayManager.ApplyRestrictionVisualsForActivePlayer(allowed, confirmedList); 
                return;                                                      
            }
        }
        // --- END PRE-CHECK ---
        // server-authoritative: confirm button submits a pick request
        var candidate = characterDisplayManager.PeekCurrentCharacter();
        if (candidate == null)
        {
            characterDisplayManager.SetTemporaryStatus("Select a character first.");
            return;
        }

        int myTeam = OnlinePlayerIdentity.LocalTeamId;
        int currentPicker = OnlineDraftClient.Instance != null ? OnlineDraftClient.Instance.LastState.CurrentPickerTeamId : -1;

        if (myTeam != currentPicker)
        {
            characterDisplayManager.SetTemporaryStatus("Wait for your turn.");
            return;
        }

        // restriction pre-check stays (use server-confirmed picks instead of local lists)
        if (characterDisplayManager.RestrictionsEnabled())
        {
            // confirmed picks for my team come from last state
            var state = OnlineDraftClient.Instance.LastState;
            var confirmedNames = (myTeam == 1) ? state.P1Picks : state.P2Picks;
            var confirmedList = characterDisplayManager.GetCharacterDataList_ForOnline(confirmedNames); // see note below

            var rarities = confirmedList.Select(c => c.rarity).ToList();
            rarities.Add(candidate.rarity);

            if (!RestrictionEngine.IsValidTeam(rarities))
            {
                characterDisplayManager.SetTemporaryStatus("Selection violates restrictions!");
                var allowed = RestrictionEngine.AllowedNextRarities(confirmedList.Select(c => c.rarity).ToList());
                characterDisplayManager.ApplyRestrictionVisualsForActivePlayer(allowed, confirmedList);
                return;
            }
        }

        // send pick request to server
        NetworkClient.Send(new PickRequestNetMessage { CharacterName = candidate.name });

       
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
