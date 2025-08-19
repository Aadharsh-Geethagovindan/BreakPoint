using System.Collections.Generic;
using UnityEngine;

public class ResistanceGridUI : MonoBehaviour
{
    [SerializeField] private ResistanceCellUI cellElemental;
    [SerializeField] private ResistanceCellUI cellArcane;
    [SerializeField] private ResistanceCellUI cellForce;
    [SerializeField] private ResistanceCellUI cellCorrupt;

    private GameCharacter current;

    public void UpdateFor(GameCharacter character)
    {
        current = character;
        Refresh();
    }

    private void Refresh()
    {
        if (current == null) return;

        cellElemental?.SetValue(current.GetModifiedResistance(DamageType.Elemental));
        cellArcane?.SetValue(current.GetModifiedResistance(DamageType.Arcane));
        cellForce?.SetValue(current.GetModifiedResistance(DamageType.Force));
        cellCorrupt?.SetValue(current.GetModifiedResistance(DamageType.Corrupt));
    }

    private void OnEnable()
    {
        // Turn changed → show new character
        EventManager.Subscribe("OnTurnStarted", OnTurnStarted);

        // Status changes can modify resistances; refresh only if it affects the current character
        EventManager.Subscribe("OnStatusApplied", OnStatusChanged);
        EventManager.Subscribe("OnStatusEffectExpired", OnStatusChanged);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe("OnTurnStarted", OnTurnStarted);
        EventManager.Unsubscribe("OnStatusApplied", OnStatusChanged);
        EventManager.Unsubscribe("OnStatusEffectExpired", OnStatusChanged);
    }

    private void OnTurnStarted(object data)
    {
        // You triggered OnTurnStarted with the GameCharacter directly
        var actor = data as GameCharacter;
        if (actor != null) UpdateFor(actor);
    }

    private void OnStatusChanged(object data)
    {
        var evt = data as GameEventData;
        if (evt == null || current == null) return;

        var target = evt.Get<GameCharacter>("Target");
        if (target == current) Refresh();
    }
}
