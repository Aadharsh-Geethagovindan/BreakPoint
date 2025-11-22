using System.Collections;
using UnityEngine;
public class LocalMatchController : IMatchController
{
    private readonly BattleManager _battleManager;
    private readonly TurnManager _turnManager;

    public LocalMatchController(BattleManager battleManager, TurnManager turnManager)
    {
        _battleManager = battleManager;
        _turnManager = turnManager;
    }

    public void HandleUseAbility(UseAbilityCommand command)
    {
        if (command == null || command.Caster == null || command.Ability == null)
            return;
        //Debug.Log("LocalMatch Controller Execute Ability");
        // In local mode, we just call the existing BattleManager coroutine
        _battleManager.StartCoroutine(
            _battleManager.ExecuteAbility(command.Caster, command.Ability, command.Targets)
        );
    }

    public void HandleSkipTurn(SkipTurnCommand command)
    {
        // For now we don’t even need the Character; we just advance the turn
        _turnManager.AdvanceTurn();
        //Debug.Log("LocalMatch Controller skip turn");
    }
}