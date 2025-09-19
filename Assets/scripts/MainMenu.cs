using UnityEngine;

using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    private void Start()
    {

        SoundManager.Instance.PlayMusic("selection");
    }
    public void LoadCharacterSelection()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    public void PlayClassic()
    {
        GameModeService.Set(GameMode.Classic); SceneManager.LoadScene("CharacterSelection");
    }
    public void PlayRevamped()
    {
        GameModeService.Set(GameMode.Revamped); SceneManager.LoadScene("CharacterSelection");
    } 


    public void OpenHowToPlay()
    {
        Application.OpenURL("https://breakpoint-site-three.vercel.app/");
    }

    public void OpenCharacterOverview()
    {
        Application.OpenURL("https://breakpoint-site-three.vercel.app/characters");
    }
    
    public void ExitGame()
    {
        Debug.Log("Exiting game..."); 
        Application.Quit();
    }
}
public enum GameMode { Classic, Revamped } // NEW

public static class GameModeService // NEW
{
    public static GameMode Mode { get; private set; } = GameMode.Classic;
    public static void Set(GameMode mode) => Mode = mode;
    public static bool IsRevamped => Mode == GameMode.Revamped;
}