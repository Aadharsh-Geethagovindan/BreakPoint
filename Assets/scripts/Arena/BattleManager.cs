using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;
using Mirror;
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private List<GameCharacter> allCharacters;
    private ActiveCharPanel charPanel;

    private GameCharacter currentCharacter => TurnManager.Instance.GetCurrentCharacter();

    //private List<GameCharacter> charactersInOrder = new List<GameCharacter>();
    
    private Dictionary<int, GameCharacter> characterById = new Dictionary<int, GameCharacter>();


    [SerializeField] private GameObject characterCardPrefab;
    [SerializeField] private Transform characterHolder;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI briefText;

    private Breakpoint.Revamped.AffinityTracker trackerP1;
    private Breakpoint.Revamped.AffinityTracker trackerP2;
    private Breakpoint.Revamped.RevampTuningConfig revampCfg;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (GameModeService.IsRevamped) // initialize affinity trackers for revamp mode
        {
            revampCfg = Resources.Load<Breakpoint.Revamped.RevampTuningConfig>("RevampTuningConfig"); // NEW
            trackerP1 = new Breakpoint.Revamped.AffinityTracker(1, revampCfg); // NEW
            trackerP2 = new Breakpoint.Revamped.AffinityTracker(2, revampCfg); // NEW
        }

        charPanel = Object.FindFirstObjectByType<ActiveCharPanel>();
        // Character creation and team setup will happen here
    }


   public void InitializeBattle(CharacterDataArray allCharacterData)
    {
        allCharacters = new List<GameCharacter>();
        characterById.Clear();

        // --- Build roster from GameData (this will already be populated for online by ArenaCharacterLoader) ---
        foreach (var cd in GameData.SelectedCharactersP1)
        {
            var gc = CharacterFactory.CreateCharacterByName(cd.name, allCharacterData);
            if (gc == null)
            {
                Debug.LogError($"Could not create character: {cd.name}");
                continue;
            }

            gc.SetTeam(1); // local default; online will overwrite from roster mapping below
            allCharacters.Add(gc);
        }

        foreach (var cd in GameData.SelectedCharactersP2)
        {
            var gc = CharacterFactory.CreateCharacterByName(cd.name, allCharacterData);
            if (gc == null)
            {
                Debug.LogError($"Could not create character: {cd.name}");
                continue;
            }

            gc.SetTeam(2); // local default; online will overwrite from roster mapping below
            allCharacters.Add(gc);
        }

        // --- Allies/enemies strictly by TeamId ---
        foreach (var c in allCharacters)
        {
            c.ClearAllies();
            c.ClearEnemies(); 

            foreach (var other in allCharacters)
            {
                if (other == c) continue;
                if (c.TeamId == other.TeamId) c.AddAlly(other);
                else c.AddEnemy(other);
            }
        }

        // --- Turn order (same for local + online) ---
        //charactersInOrder = allCharacters.OrderByDescending(c => c.Speed).ToList();

        // --- ID assignment (DIFFERENCE: online uses roster mapping; local uses sequential IDs) ---
        if (OnlineMatchData.HasRoster)
        {
            // Expect 6 entries: Team1 IDs 1-3, Team2 IDs 4-6
            var rosterByName = OnlineMatchData.Roster.ToDictionary(r => r.CharacterName, r => r);

            foreach (var ch in allCharacters)
            {
                if (!rosterByName.TryGetValue(ch.Name, out var r))
                {
                    Debug.LogWarning($"[OnlineRoster] Missing roster entry for {ch.Name}. Falling back to local ID assignment later.");
                    continue;
                }

                ch.SetTeam(r.TeamId);
                ch.SetId(r.CharacterId);
                characterById[r.CharacterId] = ch;
            }

            // Safety: if anything wasn't mapped (name mismatch), fall back for those only
            int nextFallbackId = 1;
            foreach (var ch in allCharacters)
            {
                if (ch.Id != 0) continue;

                while (characterById.ContainsKey(nextFallbackId)) nextFallbackId++;
                ch.SetId(nextFallbackId);
                characterById[nextFallbackId] = ch;
                nextFallbackId++;
            }
        }
        else
        {
            AssignCharacterIds(); //   local method (sequential 1..N)
        }
    }
    
    private void AssignCharacterIds()
    {
        characterById.Clear();
        int nextId = 1; // or 0, your choice, just be consistent

        foreach (var ch in allCharacters)
        {
            ch.SetId(nextId);
            characterById[nextId] = ch;
            nextId++;
        }
    }

    public GameCharacter GetCharacterById(int id)
    {
        characterById.TryGetValue(id, out var character);
        return character;
    }
    public IEnumerator ExecuteAbility(GameCharacter user, Ability ability, List<GameCharacter> targets)
    {
        var result = new AbilityResult
        {
            CasterId = user.Id,
            AbilityType = ability.AbilityType
        };
        // Defensive copy so UI mutations don't affect us during yields
        var targetsCopy = (targets != null) ? new List<GameCharacter>(targets) : new List<GameCharacter>();

        if (!ability.IsUsable(user.Charge))
        {
            Debug.LogWarning($"{ability.Name} is not usable right now.");
            yield break;
        }

        // Passive overrides BEFORE visuals/mechanics
        foreach (var target in targetsCopy)
            PassiveManager.ApplyOverride(user, target, ability);

        // --- Resolve hits first ---
        var resolutions = ability.ResolveHits(user, targetsCopy);
        var resolvedDict = new Dictionary<GameCharacter, bool>(resolutions.Count);
        foreach (var r in resolutions) resolvedDict[r.Target] = r.WillHit;

        foreach (var hr in resolutions)
        {
            var tr = new TargetResult
            {
                TargetId = hr.Target.Id,
                Hit = hr.WillHit
            };

            result.Targets.Add(tr);
        }

        // send clients volley info
        if (MatchTypeService.IsOnline && Mirror.NetworkServer.active && targetsCopy.Count > 0 && ShouldFireProjectile(ability))
        {
            int n = resolutions.Count;
            var targetIds = new int[n];
            var willHit = new bool[n];

            for (int i = 0; i < n; i++)
            {
                targetIds[i] = resolutions[i].Target != null ? resolutions[i].Target.Id : -1;
                willHit[i] = resolutions[i].WillHit;
            }

            Mirror.NetworkServer.SendToAll(new VolleyStartNetMessage
            {
                CasterId = user.Id,
                AbilityType = ability.AbilityType,
                DamageType = ability.DamageType,
                TargetIds = targetIds,
                WillHit = willHit
            });
        }


        // VISUALS 
        if (targetsCopy.Count > 0 && ShouldFireProjectile(ability))
            yield return StartCoroutine(AnimationManager.Instance.PlayVolley(user, resolutions, ability.DamageType));


        //Debug.Log("projectile finished");

        // MECHANICS — now apply the ability using the copied list
        ability.Apply(user, targetsCopy, resolvedDict);
        ability.CustomDamageOverride = null;

        HandleImmediateAbilityEffects(ability, user, targetsCopy);

        //Networking info to capture
        foreach (var tr in result.Targets)
        {
            var target = characterById[tr.TargetId];

            // Damage: your system records last damage taken
            tr.Damage = target.LastDamageTaken;

            // HP after effects
            tr.HPAfter = target.HP;

            // Status Effects
            foreach (var se in target.StatusEffects)
            {
                tr.AppliedEffects.Add(new AppliedStatusEffectResult
                {
                    EffectType = se.Type.ToString(),
                    Duration = se.Duration,
                    Magnitude = se.Value
                });
            }
        }

        var evt = new GameEventData()
            .Set("User", user)
            .Set("Ability", ability)
            .Set("Targets", targetsCopy);
        EventManager.Trigger("OnAbilityUsed", evt);

        // Death checks
        foreach (var t in targetsCopy)
            if (t.HP <= 0) HandleDeath(t);

        HandlePostDeathAbilityEffects(ability, user, targetsCopy);

        // Summary
        string summary = "";
        foreach (var t in targetsCopy)
        {
            if (t.LastDamageTaken > 0) summary += $"{t.Name} took {t.LastDamageTaken} damage.\n";
            if (ability.Healing > 0) summary += $"{t.Name} healed {ability.Healing} HP.\n";
            if (ability.Shielding > 0) summary += $"{t.Name} gained a shield of {ability.Shielding}\n";
        }
        GameUI.Announce(summary.Trim());
        //var snapshot = BuildGameStateSnapshot();
        //Debug.Log($"[Snapshot] After ability, Round {snapshot.RoundNumber}, CurrentCharId {snapshot.CurrentCharacterId}");
        // Send ability Result to client
        NetworkServer.SendToAll(new AbilityResultNetMessage { Result = result });

        UIAnnouncer.Instance.DelayedAnnounceAndAdvance($"{TurnManager.Instance.PeekNextCharacter().Name} is choosing a move.");

        // Refresh status icons on the same stable list
        foreach (var t in targetsCopy)
        {
            CharacterCardUI card = charPanel.FindCardForCharacter(t);
            if (card != null) card.RefreshStatusEffects(t);
        }

        EventManager.Trigger("OnExecuteAbility", result);
        /* send result to all connected clients (server only)
        if (Mirror.NetworkServer.active)
        {
            var snap = BuildGameStateSnapshot();
            Mirror.NetworkServer.SendToAll(new GameStateSnapshotNetMessage { Snapshot = snap });
            Debug.Log("[ExecuteAbility] Server broadcasted GameStateSnapshot.");
        }*/
    }


    private bool ShouldFireProjectile(Ability ability)
    {
        // Simple example: only when it deals damage
        return ability.Damage > 0;
    }

    public List<GameCharacter> GetAllCharacters() => allCharacters;


    public void HandleDeath(GameCharacter character)
    {
        if (character.IsDead)
        {
            Debug.LogWarning($"{character.Name} is already marked as dead.");
            return;
        }

        character.deathStatus(true);
        character.SetSpeed(0);  // set deadcharacter speed to 0
        EventManager.Trigger("OnCharacterDied", new GameEventData()
                .Set("Character", character)
            );


        // Notify PassiveManager (Ra, Avarice, Trex, etc.)
        PassiveManager.OnCharacterDeath(character);

        // Visually dim the character card
        ActiveCharPanel panel = Object.FindFirstObjectByType<ActiveCharPanel>();
        if (panel != null)
        {
            CharacterCardUI cardUI = panel.FindCardForCharacter(character);
            if (cardUI != null)
            {
                cardUI.SetCardDimmed(true);
                //Debug.Log("Card dimmed");  
            }
        }

        // Check win/loss condition
        if (character.Allies.TrueForAll(a => a.IsDead))
        {
            int winner = GameData.SelectedCharactersP1.Exists(c => c.name == character.Name) ? 2 : 1;

            var evt = new GameEventData();
            evt.Set("Winner", winner);
            EventManager.Trigger("OnGameEnded", evt); // <-- Add this trigger

            PlayerPrefs.SetInt("Winner", winner);
            SceneManager.LoadScene("EndGame");
        }
    }





    private void HandleImmediateAbilityEffects(Ability ability, GameCharacter user, List<GameCharacter> targets)
    {
        if (ability.Name == "Necrosis")
        {
            foreach (var target in targets)
            {
                target.StatusEffects.RemoveAll(eff => eff.Type == StatusEffectType.HealingOverTime);
                Debug.Log($"Necrosis removed HoTs from {target.Name}");
            }
        }
        else if (ability.Name == "Constellian Command")
        {
            foreach (var ally in user.Allies)
            {
                int gain = Mathf.RoundToInt(ally.SignatureAbility.ChargeRequirement * 0.5f);
                ally.IncreaseCharge(gain);
                ally.StatusEffects.RemoveAll(e => e.IsDebuff);
            }

            GameUI.Announce($"{user.Name} empowered the team with Constellian Command!");
        }

        if (ability.Name == "Fully Stable Runic Array")
        {
            foreach (var target in targets)
            {
                if (target.SkillAbility.CurrentCooldown > 0)
                {
                    target.SkillAbility.CurrentCooldown = Mathf.Max(0, target.SkillAbility.CurrentCooldown - 2);
                    Debug.Log($"{target.Name}'s Skill cooldown reduced by 2 turns.");
                }
            }
        }

        if (ability.Name == "Engineered Setback")
        {
            foreach (var target in targets)
            {
                int reduction = Mathf.RoundToInt(target.SignatureAbility.ChargeRequirement * 0.3f);
                target.ReduceCharge(reduction);
                Debug.Log($"Vemk reduced {target.Name}'s sig charge by {reduction}");
            }
        }

    }


    private void HandlePostDeathAbilityEffects(Ability ability, GameCharacter user, List<GameCharacter> targets)
    {
        if (ability.Name == "Resurrection")
        {
            if (PassiveManager.ResurrectionTracker.Count > 0)
            {
                GameCharacter toRevive = PassiveManager.ResurrectionTracker[0];
                int reviveHP = Mathf.RoundToInt(toRevive.MaxHP * 0.25f);
                toRevive.Heal(reviveHP);
                toRevive.deathStatus(false);
                Debug.Log($"Avarice revived {toRevive.Name} with {reviveHP} HP.");

                EventManager.Trigger("OnCharacterRevived", new GameEventData()
                        .Set("Character", toRevive)
                    );
            }
            else
            {
                Debug.Log("No allies to resurrect with Avarice's ability.");
            }
        }
    }

    void OnDestroy()
    {
        //clean up listeners to avoid ghosts after scene changes
        trackerP1?.Dispose();
        trackerP2?.Dispose();
    }

    public List<GameCharacter> GetTeam(int teamId)
        => allCharacters.Where(c => c.TeamId == teamId).ToList();

    public List<GameCharacter> GetAllAliveCharacters()
        => allCharacters.Where(c => !c.IsDead).ToList();

    public GameStateSnapshot BuildGameStateSnapshot()
    {
        var snapshot = new GameStateSnapshot();

       
        snapshot.RoundNumber = TurnManager.Instance.GetCurrentRound(); 
        snapshot.CurrentCharacterId = TurnManager.Instance.GetCurrentCharacter()?.Id ?? -1;
        snapshot.CurrentTeamId = TurnManager.Instance.GetCurrentCharacter()?.TeamId ?? -1;

        foreach (var ch in allCharacters)
        {
            var cs = new CharacterState
            {
                Id = ch.Id,
                Name = ch.Name,
                TeamId = ch.TeamId,
                HP = ch.HP,
                MaxHP = ch.MaxHP,
                SigCharge = ch.Charge,          
                SigChargeRequired = ch.SigChargeReq, 
                IsDead = ch.HP <= 0,
                IsStunned = ch.HasStatusEffect(StatusEffectType.Stun) 
            };

            // Status effects
            foreach (var se in ch.StatusEffects)
            {
                var seState = new StatusEffectState
                {
                    Type = se.GetIconName(),            // or se.Id / enum name
                    SourceName = se.Source?.Name,
                    RemainingTurns = se.Duration,
                    Magnitude = se.Value
                };
                cs.StatusEffects.Add(seState);
            }

            // Abilities – adapt to your setup
            foreach (var ability in ch.GetAllAbilities()) // you might need to implement this helper
            {
                var aState = new AbilityState
                {
                    Name = ability.Name,
                    AbilityType = ability.AbilityType.ToString(),
                    CurrentCooldown = ability.CurrentCooldown,
                    BaseCooldown = ability.BaseCooldown,
                    IsUsable = ability.IsUsable(ch.Charge) // or replicate your existing logic
                };
                cs.Abilities.Add(aState);
            }
            snapshot.TurnOrderIds = TurnManager.Instance.GetTurnOrder().Select(c => c.Id).ToList();
            snapshot.Characters.Add(cs);
        }

        

        return snapshot;
    }

}
