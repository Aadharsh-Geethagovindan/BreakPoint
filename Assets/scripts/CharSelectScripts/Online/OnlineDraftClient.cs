using Mirror;
using UnityEngine;

public class OnlineDraftClient : MonoBehaviour
{
    public static OnlineDraftClient Instance { get; private set; }

    private OnlineCharacterDisplayManager _display;

    public DraftStateNetMessage LastState { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _display = FindFirstObjectByType<OnlineCharacterDisplayManager>();
    }

    // registered as NetworkClient handler
    public static void OnDraftStateMessage(DraftStateNetMessage msg)
    {
        if (Instance == null) return;
        Instance.Apply(msg);
    }

    private void Apply(DraftStateNetMessage msg)
    {
        LastState = msg;

        if (_display == null)
            _display = FindFirstObjectByType<OnlineCharacterDisplayManager>();

        if (_display == null)
            return;

        _display.ApplyDraftState(msg);
    }
}
