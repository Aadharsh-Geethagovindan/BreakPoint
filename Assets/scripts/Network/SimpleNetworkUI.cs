using Mirror;
using UnityEngine;

public class SimpleNetworkUI : MonoBehaviour
{
    public void StartHost()
    {
        if (!NetworkManager.singleton.isNetworkActive)
        {
            NetworkManager.singleton.StartHost();
        }
        else
        {
            Debug.LogWarning("Network is already active, cannot StartHost again.");
        }
    }

    public void StartClient()
    {
        if (!NetworkManager.singleton.isNetworkActive)
        {
            NetworkManager.singleton.StartClient();
        }
        else
        {
            Debug.LogWarning("Network is already active, cannot StartClient again.");
        }
    }

    public void StopNetwork()
    {
        if (!NetworkManager.singleton.isNetworkActive)
            return;

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
