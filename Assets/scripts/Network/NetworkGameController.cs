using Mirror;
using UnityEngine;

public class NetworkGameController : NetworkBehaviour
{
    public static NetworkGameController Instance { get;  set; }

    private LocalMatchController _localMatchController;

   

    public override void OnStartServer()
    {
        Debug.Log("Server start called");
        base.OnStartServer();

        Instance = this;

        if (BattleManager.Instance == null || TurnManager.Instance == null)
        {
            Debug.LogError("NetworkGameController: BattleManager or TurnManager not found on server.");
            return;
        }

        _localMatchController = new LocalMatchController(BattleManager.Instance, TurnManager.Instance);
    }

    public override void OnStartClient()
    {
        Debug.Log("Client start called");
        base.OnStartClient();

        // On clients, we ALSO want Instance to reference this spawned object
        if (!isServer) // host will already set it in OnStartServer
        {
            Debug.Log("NetworkGameController: OnStartClient on client, setting Instance.");
            Instance = this;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdUseAbility(UseAbilityCommandData data)
    {
        if (_localMatchController == null)
        {
            Debug.LogWarning("NetworkGameController: _localMatchController is null in CmdUseAbility.");
            return;
        }

        var serverCommand = UseAbilityCommand.FromData(data);
        if (serverCommand == null)
        {
            Debug.LogError("NetworkGameController: Failed to reconstruct UseAbilityCommand from data.");
            return;
        }

        _localMatchController.HandleUseAbility(serverCommand);
    }

    [Command(requiresAuthority = false)]
    public void CmdSkipTurn(SkipTurnCommandData data)
    {
        if (_localMatchController == null)
        {
            Debug.LogWarning("NetworkGameController: _localMatchController is null in CmdSkipTurn.");
            return;
        }

        var serverCommand = SkipTurnCommand.FromData(data) ?? new SkipTurnCommand(null);
        _localMatchController.HandleSkipTurn(serverCommand);
    }

    // -------- Broadcast to all clients when an ability resolves --------
    [ClientRpc]
    public void RpcAbilityResolved(AbilityResult result)
    {
        Debug.Log("In RPC ABILITY RESOLVED");
        // This runs on every client (including host’s local client)
        if (result == null)
        {
            Debug.LogWarning("RpcAbilityResolved received null result.");
            return;
        }

        var msg = $"[RPC] AbilityResult: Caster {result.CasterId}, Ability {result.AbilityType}, targets {result.Targets.Count}";
        foreach (var tr in result.Targets)
        {
            msg += $"\n  Target {tr.TargetId}: Hit={tr.Hit}, Damage={tr.Damage}, HPAfter={tr.HPAfter}, Effects={tr.AppliedEffects.Count}";
        }

        Debug.Log(msg);
        GameUI.Announce(msg);
    }
}
