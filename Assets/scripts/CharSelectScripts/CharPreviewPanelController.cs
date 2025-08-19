using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharPreviewPanelController : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] Image portrait;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text rarityText;

    [Header("Stats")]
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text speedText;
    [SerializeField] TMP_Text sigReqText;

    [Header("Abilities")]
    [SerializeField] TMP_Text passiveTitle;
    [SerializeField] TMP_Text passiveMech;

    [SerializeField] TMP_Text normalTitle;
    [SerializeField] TMP_Text normalMech;

    [SerializeField] TMP_Text skillTitle;
    [SerializeField] TMP_Text skillMech;

    [SerializeField] TMP_Text signatureTitle;
    [SerializeField] TMP_Text signatureMech;

    [Header("FX (optional)")]
    [SerializeField] CanvasGroup cg;
    [SerializeField] float fadeTime = 0.08f;

    public void Show(CharacterData data)
    {
        if (data == null) { Hide(); return; }

        // Header
        nameText.text   = data.name;
        rarityText.text = data.rarity;
        rarityText.color = RarityColor(data.rarity);

        // Stats
        hpText.text     = $"HP: {data.hp}";
        speedText.text  = $"SPD: {data.speed}";
        sigReqText.text = $"Sig: {data.SigChargeReq}";

        // Portrait (adjust path to your setup: Resources, Addressables, or sprite atlas)
        // Example if using Resources/Characters/<imageName>.png
        if (!string.IsNullOrEmpty(data.imageName))
            portrait.sprite = Resources.Load<Sprite>($"Images/{data.imageName}");

        // Moves: assume order = [0]=Passive, [1]=Normal, [2]=Skill, [3]=Signature
        BindMove(data.moves, 0, passiveTitle,   passiveMech,   "");
        BindMove(data.moves, 1, normalTitle,    normalMech,    "");
        BindMove(data.moves, 2, skillTitle,     skillMech,     "");
        BindMove(data.moves, 3, signatureTitle, signatureMech, "");

        SetVisible(true);
    }

    public void Hide() => SetVisible(false);

    void BindMove(MoveData[] arr, int i, TMP_Text title, TMP_Text mech, string label)
    {
        if (arr == null || arr.Length <= i || arr[i] == null) {
            title.text = $"{label}: —";
            mech.text  = "";
            return;
        }
        var m = arr[i];
        title.text = $"{label}: {m.name}";
        // If you want cooldown surfaced here too:
        mech.text  = string.IsNullOrEmpty(m.mechanics)
                     ? m.description
                     : $"{m.mechanics}{(m.cooldown > 0 ? $" (CD {m.cooldown})" : "")}";
    }

    void SetVisible(bool on)
    {
        if (!cg) { gameObject.SetActive(on); return; }
        StopAllCoroutines();
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        StartCoroutine(Fade(on ? 1f : 0f));
        cg.blocksRaycasts = on;
        if (!on && cg.alpha <= 0f) gameObject.SetActive(true); // keep object present for fast fade-ins
    }

    System.Collections.IEnumerator Fade(float target)
    {
        if (!cg) yield break;
        float t = 0f, start = cg.alpha;
        while (t < fadeTime) {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        cg.alpha = target;
    }

    Color RarityColor(string r)
    {
        switch (r) {
            case "Common":    return new Color(0.78f,0.78f,0.78f);
            case "UC":  return new Color(0.55f,0.9f,0.65f);
            case "R":      return new Color(0.55f,0.7f,1.0f);
            case "UR":
            case "UltraRare": return new Color(0.9f,0.55f,1.0f);
            case "L": return new Color(1.0f,0.85f,0.35f);
            default:          return Color.white;
        }
    }
}
