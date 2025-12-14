using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField addressInput;  // optional; can be null if you don’t want it yet
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text labelText;

    [Header("Buttons")]
    [SerializeField] private GameObject continueButton;    // set active/inactive (or use Button.interactable)

    [Header("Scenes")]
    [SerializeField] private string onlineCharacterSelectSceneName = "OnlineCharacterSelect";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private BreakpointNetworkManager nm;

    private void Awake()
    {
        nm = FindFirstObjectByType<BreakpointNetworkManager>();
        if (nm == null)
        {
            SetStatus("ERROR: No BreakpointNetworkManager found in scene.");
            return;
        }

        // Default address if input exists
        if (addressInput != null && string.IsNullOrWhiteSpace(addressInput.text))
            addressInput.text = "localhost";

        UpdateUI();
    }

    private void Update()
    {
        // Keep UI responsive (simple approach for now)
        UpdateUI();
    }

    public void OnClickHost()
    {
        if (nm == null) return;

        nm.networkAddress = GetAddress();
        SetStatus($"Hosting on {nm.networkAddress}:{GetPortString()}");
        nm.StartHost();
    }

    public void OnClickJoin()
    {
        if (nm == null) return;

        nm.networkAddress = GetAddress();
        SetStatus($"Joining {nm.networkAddress}:{GetPortString()}");
        nm.StartClient();
    }

    public void OnClickStop()
    {
        if (nm == null) return;

        // Stop whatever is running
        if (nm.isNetworkActive)
            nm.StopHost(); // StopHost safely stops host or client

        SetStatus("Stopped.");
    }

    public void OnClickContinue()
    {
        if (nm == null) return;

        // Host-only continue
        if (!Mirror.NetworkServer.active)
        {
            SetStatus("Only host can continue.");
            return;
        }

        // Require both players (host + 1 client)
        int connected = nm.ConnectedClientCount;
        if (connected < 2)
        {
            SetStatus($"Need 2 players. Connected: {connected}");
            return;
        }

        SetStatus("Loading Online Character Select...");
        nm.ServerChangeScene(onlineCharacterSelectSceneName);
    }

    public void OnClickBackToMenu()
    {
        // Optional: if you add a Back button
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void UpdateUI()
    {
        if (continueButton != null)
        {
            bool hostActive = Mirror.NetworkServer.active;
            bool enoughPlayers = hostActive && nm != null && nm.ConnectedClientCount >= 2;
            continueButton.SetActive(enoughPlayers);
        }

        if (labelText != null)
        {
            if (Mirror.NetworkServer.active && Mirror.NetworkClient.active)
                labelText.text = $"HOST | Connected: {nm.ConnectedClientCount}";
            else if (Mirror.NetworkClient.active)
                labelText.text = "CLIENT | Connected";
            else
                labelText.text = "OFFLINE";
        }
    }

    private string GetAddress()
    {
        if (addressInput == null) return "localhost";
        return string.IsNullOrWhiteSpace(addressInput.text) ? "localhost" : addressInput.text.Trim();
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        if (Logger.Instance != null) Logger.Instance.PostLog(msg, LogType.Status);
        else Debug.Log(msg);
    }

    private string GetPortString()
    {
        // TelepathyTransport is what you’re using
        var tp = nm != null ? nm.transport as Mirror.TelepathyTransport : null;
        return tp != null ? tp.port.ToString() : "7777";
    }
}
