using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{

    [Header("Mode Toggle UI")]
    [SerializeField] private Image classicOrb;
    [SerializeField] private Image revampedOrb;

    // 255 = fully opaque, 100 = dim
    private const float ACTIVE_ALPHA = 1f;              // 255/255
    private const float INACTIVE_ALPHA = 100f / 255f;   // ~0.392


    private void Start()
    {
        ApplyModeVisuals(GameModeService.Mode);
        SoundManager.Instance.PlayMusic("selection");
    }

    private void ApplyModeVisuals(GameMode mode)
    {
        bool classicActive = (mode == GameMode.Classic);

        SetAlpha(classicOrb, classicActive ? ACTIVE_ALPHA : INACTIVE_ALPHA);
        SetAlpha(revampedOrb, classicActive ? INACTIVE_ALPHA : ACTIVE_ALPHA);
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color;
        c.a = a;
        img.color = c;
    }
    public void LoadCharacterSelection()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    public void SelectClassicMode()
    {
        GameModeService.Set(GameMode.Classic);
        ApplyModeVisuals(GameMode.Classic);
    }

    public void SelectRevampedMode()
    {
        GameModeService.Set(GameMode.Revamped);
        ApplyModeVisuals(GameMode.Revamped);
    }

    public void PlayLocal()
    {
        MatchTypeService.Set(MatchType.Local);
        SceneManager.LoadScene("CharacterSelection");
    }

    public void PlayOnline()
    {
        MatchTypeService.Set(MatchType.Online);
        SceneManager.LoadScene("Lobby"); // new scene you’ll create next
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

public enum MatchType { Local, Online }

public static class MatchTypeService
{
    public static MatchType Type { get; private set; } = MatchType.Local;
    public static void Set(MatchType type) => Type = type;
    public static bool IsOnline => Type == MatchType.Online;
}