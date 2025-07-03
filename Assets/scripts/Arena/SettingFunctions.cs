using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingFunctions : MonoBehaviour
{
    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}