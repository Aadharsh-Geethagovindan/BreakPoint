using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPBarController : MonoBehaviour
{
    private GameCharacter character;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image fillImage;
    public Slider shieldSlider; // new slider for shield

    // Optional: Colors for gradient effect
    public Color highHPColor = new Color(1f, 0.84f, 0f);  // Gold
    public Color lowHPColor = new Color(0.54f, 0f, 0.54f); // Violet

    private bool useSnapshotOverride = false;
    private int snapshotHP;
    private int snapshotMaxHP;

    public void Initialize(GameCharacter character)
    {
        this.character = character;
        UpdateBar(); // Set initial state
    }

    private void Update()
    {
        if (useSnapshotOverride)
        return;
        
        if (character == null) return;
        UpdateBar();
    }

    private void UpdateBar()
    {
        hpSlider.maxValue = character.MaxHP;
        hpSlider.value = character.HP;

        shieldSlider.maxValue = character.MaxHP; // match max HP for scaling
        shieldSlider.value = character.Shield;

        if (hpText != null)
            hpText.text = $"{character.HP}/{character.MaxHP}";

        if (fillImage != null)
        {
            float t = (float)character.HP / character.MaxHP;
            fillImage.color = Color.Lerp(lowHPColor, highHPColor, t);
        }

         // Show shield bar only if shield > 0
            if (character.Shield > 0)
            {
                //Debug.Log($"{character.Name} has shield {character.Shield}");
                shieldSlider.gameObject.SetActive(true);
                shieldSlider.value = character.Shield;
            }
            else
            {
                shieldSlider.gameObject.SetActive(false);
            }

    }

    public void ApplySnapshotHP(int hp, int maxHp)
    {
        useSnapshotOverride = true;
        snapshotHP = hp;
        snapshotMaxHP = maxHp;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = hp;
        }

        if (hpText != null)
        {
            hpText.text = $"{hp}/{maxHp}";
        }
    }
}
