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
