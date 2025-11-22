using UnityEngine;

public static class MatchController
{
    private static IMatchController _instance;

    public static IMatchController Instance
    {
        get
        {
            if (_instance == null)
            {
                if (BattleManager.Instance != null && TurnManager.Instance != null)
                {
                    var local = new LocalMatchController(BattleManager.Instance, TurnManager.Instance);
                    _instance = new NetworkMatchController(local);
                }
                else
                {
                    Debug.LogError("MatchController: BattleManager or TurnManager not ready.");
                }
            }
            return _instance;
        }
        set { _instance = value; }
    }
}