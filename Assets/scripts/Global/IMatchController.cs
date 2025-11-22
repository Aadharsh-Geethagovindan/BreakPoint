public interface IMatchController
{
    void HandleUseAbility(UseAbilityCommand command);
    void HandleSkipTurn(SkipTurnCommand command);
}