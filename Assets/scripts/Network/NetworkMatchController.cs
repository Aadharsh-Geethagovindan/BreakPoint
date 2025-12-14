using UnityEngine;
using Mirror;
public class NetworkMatchController : IMatchController
{
    // This is “server-side” match controller for now (still in same process)
    private readonly LocalMatchController _serverMatchController;

    public NetworkMatchController(LocalMatchController serverMatchController)
    {
        _serverMatchController = serverMatchController;
    }

    // Called by the local client UI
    public void HandleUseAbility(UseAbilityCommand command)
    {
        if (command == null) return;

        var data = command.ToData();

        if (NetworkClient.active)
        {
            Debug.Log("NetworkMatchController: sending UseAbilityNetMessage to server.");
            var msg = new UseAbilityNetMessage { Data = data };
            NetworkClient.Send(msg);
            return;
        }

        // Offline / editor-only fallback
        Debug.LogWarning("NetworkMatchController: NetworkClient inactive, using local simulation.");
        SimulateServerReceiveUseAbility(data);
    }

    public void HandleSkipTurn(SkipTurnCommand command)
    {
        if (command == null) return;

        var data = command.ToData();

        if (NetworkClient.active)
        {
            Debug.Log("NetworkMatchController: sending SkipTurnNetMessage to server.");
            var msg = new SkipTurnNetMessage { Data = data };
            NetworkClient.Send(msg);
            return;
        }

        Debug.LogWarning("NetworkMatchController: NetworkClient inactive, using local simulation.");
        SimulateServerReceiveSkipTurn(data);
    }

  

    // “Server” receive path (still local for now)
    private void SimulateServerReceiveUseAbility(UseAbilityCommandData data)
    {
        // SERVER SIDE: convert back from IDs to object references
        var serverCommand = UseAbilityCommand.FromData(data);
        if (serverCommand == null)
        {
            Debug.LogError("NetworkMatchController: Failed to reconstruct UseAbilityCommand from data.");
            return;
        }

        _serverMatchController.HandleUseAbility(serverCommand);

        // Later, after networking, this is where the server would:
        // - read BattleManager.Instance.LastAbilityResult
        // - read a GameStateSnapshot
        // - send those to all clients
    }

    private void SimulateServerReceiveSkipTurn(SkipTurnCommandData data)
    {
        var serverCommand = SkipTurnCommand.FromData(data);
        if (serverCommand == null)
        {
            Debug.LogWarning("NetworkMatchController: SkipTurnCommand.FromData returned null, skipping.");
            // Even with null we can still advance turn in local mode:
            serverCommand = new SkipTurnCommand(null);
        }

        _serverMatchController.HandleSkipTurn(serverCommand);
    }
}
